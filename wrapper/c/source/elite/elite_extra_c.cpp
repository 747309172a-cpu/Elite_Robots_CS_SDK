// SPDX-License-Identifier: MIT
#include <Elite/ControllerLog.hpp>
#include <Elite/Log.hpp>
#include <Elite/RemoteUpgrade.hpp>
#include <Elite/VersionInfo.hpp>
#include <EliteC/Elite/elite_extra_c.h>

#include <cstring>
#include <memory>
#include <mutex>
#include <string>

namespace {
std::mutex g_log_handler_mutex;
elite_log_handler_cb_t g_log_handler_cb = nullptr;
void* g_log_handler_user_data = nullptr;

class CLogHandler : public ELITE::LogHandler {
   public:
    void log(const char* file, int line, ELITE::LogLevel loglevel, const char* log) override {
        elite_log_handler_cb_t cb = nullptr;
        void* user_data = nullptr;
        {
            std::lock_guard<std::mutex> lock(g_log_handler_mutex);
            cb = g_log_handler_cb;
            user_data = g_log_handler_user_data;
        }
        if (cb) {
            cb(file, line, static_cast<int32_t>(loglevel), log, user_data);
        }
    }
};

inline ELITE::LogLevel to_log_level(elite_log_level_t level) {
    switch (level) {
        case ELITE_LOG_LEVEL_DEBUG:
            return ELITE::LogLevel::ELI_DEBUG;
        case ELITE_LOG_LEVEL_INFO:
            return ELITE::LogLevel::ELI_INFO;
        case ELITE_LOG_LEVEL_WARN:
            return ELITE::LogLevel::ELI_WARN;
        case ELITE_LOG_LEVEL_ERROR:
            return ELITE::LogLevel::ELI_ERROR;
        case ELITE_LOG_LEVEL_FATAL:
            return ELITE::LogLevel::ELI_FATAL;
        case ELITE_LOG_LEVEL_NONE:
        default:
            return ELITE::LogLevel::ELI_NONE;
    }
}

inline void fill_version_info(const ELITE::VersionInfo& in, elite_version_info_t2* out) {
    out->major = in.major;
    out->minor = in.minor;
    out->bugfix = in.bugfix;
    out->build = in.build;
}

inline ELITE::VersionInfo to_version_info(const elite_version_info_t2* in) {
    return ELITE::VersionInfo(static_cast<int>(in->major), static_cast<int>(in->minor), static_cast<int>(in->bugfix),
                              static_cast<int>(in->build));
}

inline elite_c_status_t copy_string_to_buffer(const std::string& value, char* out_buffer, int32_t buffer_len,
                                              int32_t* out_required_len) {
    if (!out_required_len) {
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    const int32_t required_len = static_cast<int32_t>(value.size()) + 1;
    *out_required_len = required_len;

    if (!out_buffer || buffer_len <= 0) {
        return ELITE_C_STATUS_OK;
    }

    const int32_t copy_len = (buffer_len < required_len) ? (buffer_len - 1) : static_cast<int32_t>(value.size());
    if (copy_len > 0) {
        std::memcpy(out_buffer, value.data(), static_cast<size_t>(copy_len));
    }
    out_buffer[(copy_len >= 0) ? copy_len : 0] = '\0';
    return ELITE_C_STATUS_OK;
}
}  // namespace

elite_c_status_t elite_controller_log_download_system_log(const char* robot_ip, const char* password, const char* path,
                                                          elite_controller_log_progress_cb_t cb, void* user_data,
                                                          int32_t* out_success) {
    if (!robot_ip || !password || !path || !out_success) {
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        auto progress_cb = [cb, user_data](int f_z, int r_z, const char* err) {
            if (cb) {
                cb(f_z, r_z, err, user_data);
            }
        };
        *out_success = ELITE::ControllerLog::downloadSystemLog(robot_ip, password, path, std::move(progress_cb)) ? 1 : 0;
        return ELITE_C_STATUS_OK;
    } catch (...) {
        return ELITE_C_STATUS_EXCEPTION;
    }
}

elite_c_status_t elite_upgrade_control_software(const char* ip, const char* file, const char* password, int32_t* out_success) {
    if (!ip || !file || !password || !out_success) {
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        *out_success = ELITE::UPGRADE::upgradeControlSoftware(ip, file, password) ? 1 : 0;
        return ELITE_C_STATUS_OK;
    } catch (...) {
        return ELITE_C_STATUS_EXCEPTION;
    }
}

elite_c_status_t elite_register_log_handler(elite_log_handler_cb_t cb, void* user_data) {
    if (!cb) {
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        {
            std::lock_guard<std::mutex> lock(g_log_handler_mutex);
            g_log_handler_cb = cb;
            g_log_handler_user_data = user_data;
        }
        ELITE::registerLogHandler(std::make_unique<CLogHandler>());
        return ELITE_C_STATUS_OK;
    } catch (...) {
        return ELITE_C_STATUS_EXCEPTION;
    }
}

void elite_unregister_log_handler() {
    {
        std::lock_guard<std::mutex> lock(g_log_handler_mutex);
        g_log_handler_cb = nullptr;
        g_log_handler_user_data = nullptr;
    }
    ELITE::unregisterLogHandler();
}

void elite_set_log_level(elite_log_level_t level) { ELITE::setLogLevel(to_log_level(level)); }

void elite_log_message(const char* file, int32_t line, elite_log_level_t level, const char* message) {
    ELITE::log(file ? file : "", line, to_log_level(level), "%s", message ? message : "");
}

void elite_log_debug_message(const char* file, int32_t line, const char* message) {
    elite_log_message(file, line, ELITE_LOG_LEVEL_DEBUG, message);
}
void elite_log_info_message(const char* file, int32_t line, const char* message) {
    elite_log_message(file, line, ELITE_LOG_LEVEL_INFO, message);
}
void elite_log_warn_message(const char* file, int32_t line, const char* message) {
    elite_log_message(file, line, ELITE_LOG_LEVEL_WARN, message);
}
void elite_log_error_message(const char* file, int32_t line, const char* message) {
    elite_log_message(file, line, ELITE_LOG_LEVEL_ERROR, message);
}
void elite_log_fatal_message(const char* file, int32_t line, const char* message) {
    elite_log_message(file, line, ELITE_LOG_LEVEL_FATAL, message);
}
void elite_log_none_message(const char* file, int32_t line, const char* message) {
    elite_log_message(file, line, ELITE_LOG_LEVEL_NONE, message);
}

elite_c_status_t elite_version_info_from_string(const char* version, elite_version_info_t2* out_version) {
    if (!version || !out_version) {
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        const auto v = ELITE::VersionInfo::fromString(version);
        fill_version_info(v, out_version);
        return ELITE_C_STATUS_OK;
    } catch (...) {
        return ELITE_C_STATUS_EXCEPTION;
    }
}

elite_c_status_t elite_version_info_to_string(const elite_version_info_t2* version, char* out_buffer, int32_t buffer_len,
                                              int32_t* out_required_len) {
    if (!version || !out_required_len) {
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        const auto v = to_version_info(version);
        return copy_string_to_buffer(v.toString(), out_buffer, buffer_len, out_required_len);
    } catch (...) {
        return ELITE_C_STATUS_EXCEPTION;
    }
}

int32_t elite_version_info_eq(const elite_version_info_t2* a, const elite_version_info_t2* b) {
    if (!a || !b) {
        return 0;
    }
    return to_version_info(a) == to_version_info(b) ? 1 : 0;
}
int32_t elite_version_info_ne(const elite_version_info_t2* a, const elite_version_info_t2* b) {
    if (!a || !b) {
        return 0;
    }
    return to_version_info(a) != to_version_info(b) ? 1 : 0;
}
int32_t elite_version_info_lt(const elite_version_info_t2* a, const elite_version_info_t2* b) {
    if (!a || !b) {
        return 0;
    }
    return to_version_info(a) < to_version_info(b) ? 1 : 0;
}
int32_t elite_version_info_le(const elite_version_info_t2* a, const elite_version_info_t2* b) {
    if (!a || !b) {
        return 0;
    }
    return to_version_info(a) <= to_version_info(b) ? 1 : 0;
}
int32_t elite_version_info_gt(const elite_version_info_t2* a, const elite_version_info_t2* b) {
    if (!a || !b) {
        return 0;
    }
    return to_version_info(a) > to_version_info(b) ? 1 : 0;
}
int32_t elite_version_info_ge(const elite_version_info_t2* a, const elite_version_info_t2* b) {
    if (!a || !b) {
        return 0;
    }
    return to_version_info(a) >= to_version_info(b) ? 1 : 0;
}
