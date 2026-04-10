// SPDX-License-Identifier: MIT
#include <Elite/Log.hpp>
#include <Elite/Elite_Log_C.hpp>

#include <memory>
#include <mutex>

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
}  // namespace

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
