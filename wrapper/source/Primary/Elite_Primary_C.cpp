// SPDX-License-Identifier: MIT
#include <Primary/Elite_Primary_C.h>
#include <Elite/PrimaryPortInterface.hpp>
#include <Elite/RobotConfPackage.hpp>
#include <Elite/RobotException.hpp>

#include <cstring>
#include <memory>
#include <mutex>
#include <new>
#include <string>

struct elite_primary_handle_t {
    std::unique_ptr<ELITE::PrimaryPortInterface> primary;
    std::shared_ptr<struct callback_state_t> callback_state;
    std::string last_error;
    std::mutex mutex;
};

struct callback_state_t {
    std::mutex mutex;
    elite_primary_robot_exception_cb_t cb{nullptr};
    void* user_data{nullptr};
};

namespace {
thread_local std::string g_last_error;

inline void set_global_error(const std::string& msg) { g_last_error = msg; }

inline void set_handle_error(elite_primary_handle_t* handle, const std::string& msg) {
    if (!handle) {
        set_global_error(msg);
        return;
    }
    std::lock_guard<std::mutex> lock(handle->mutex);
    handle->last_error = msg;
    g_last_error = msg;
}

template <typename Fn>
elite_c_status_t run_with_handle(elite_primary_handle_t* handle, Fn&& fn) {
    if (!handle) {
        set_global_error("handle is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        fn();
        return ELITE_C_STATUS_OK;
    } catch (const std::exception& e) {
        set_handle_error(handle, e.what());
        return ELITE_C_STATUS_EXCEPTION;
    } catch (...) {
        set_handle_error(handle, "unknown exception");
        return ELITE_C_STATUS_EXCEPTION;
    }
}

inline elite_c_status_t copy_string_to_buffer(const std::string& value, char* out_buffer, int32_t buffer_len,
                                              int32_t* out_required_len) {
    if (!out_required_len) {
        set_global_error("out_required_len is null");
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

inline void copy_vector6_to_buffer(const ELITE::vector6d_t& src, double* dst6) {
    for (size_t i = 0; i < src.size(); ++i) {
        dst6[i] = src[i];
    }
}

inline void fill_robot_exception_payload(const ELITE::RobotExceptionSharedPtr& ex, elite_primary_robot_exception_t* out_payload,
                                         std::string* io_message_buf) {
    std::memset(out_payload, 0, sizeof(*out_payload));
    out_payload->type = static_cast<int32_t>(ex->getType());
    out_payload->timestamp = ex->getTimestamp();

    if (ex->getType() == ELITE::RobotException::Type::ROBOT_ERROR) {
        const auto robot_err = std::static_pointer_cast<ELITE::RobotError>(ex);
        out_payload->error_code = robot_err->getErrorCode();
        out_payload->sub_error_code = robot_err->getSubErrorCode();
        out_payload->error_source = static_cast<int32_t>(robot_err->getErrorSouce());
        out_payload->error_level = static_cast<int32_t>(robot_err->getErrorLevel());
        out_payload->error_data_type = static_cast<int32_t>(robot_err->getErrorDataType());
        const auto data = robot_err->getData();
        if (const auto p = std::get_if<uint32_t>(&data)) {
            out_payload->data_u32 = *p;
        } else if (const auto p = std::get_if<int32_t>(&data)) {
            out_payload->data_i32 = *p;
        } else if (const auto p = std::get_if<float>(&data)) {
            out_payload->data_f32 = *p;
        } else if (const auto p = std::get_if<std::string>(&data)) {
            *io_message_buf = *p;
            out_payload->message = io_message_buf->c_str();
        }
    } else if (ex->getType() == ELITE::RobotException::Type::SCRIPT_RUNTIME) {
        const auto runtime_err = std::static_pointer_cast<ELITE::RobotRuntimeException>(ex);
        out_payload->line = runtime_err->getLine();
        out_payload->column = runtime_err->getColumn();
        *io_message_buf = runtime_err->getMessage();
        out_payload->message = io_message_buf->c_str();
    }
}
}  // namespace

elite_c_status_t elite_primary_create(elite_primary_handle_t** out_handle) {
    if (!out_handle) {
        set_global_error("out_handle is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        auto handle = std::make_unique<elite_primary_handle_t>();
        handle->primary = std::make_unique<ELITE::PrimaryPortInterface>();
        handle->callback_state = std::make_shared<callback_state_t>();
        *out_handle = handle.release();
        set_global_error("");
        return ELITE_C_STATUS_OK;
    } catch (const std::bad_alloc&) {
        set_global_error("allocation failed");
        return ELITE_C_STATUS_ALLOCATION_FAILED;
    } catch (const std::exception& e) {
        set_global_error(e.what());
        return ELITE_C_STATUS_EXCEPTION;
    } catch (...) {
        set_global_error("unknown exception");
        return ELITE_C_STATUS_EXCEPTION;
    }
}

void elite_primary_destroy(elite_primary_handle_t* handle) {
    if (!handle) {
        return;
    }

    if (handle->primary) {
        handle->primary->registerRobotExceptionCallback(nullptr);
    }
    if (handle->callback_state) {
        std::lock_guard<std::mutex> lock(handle->callback_state->mutex);
        handle->callback_state->cb = nullptr;
        handle->callback_state->user_data = nullptr;
    }
    delete handle;
}

elite_c_status_t elite_primary_connect(elite_primary_handle_t* handle, const char* ip, int32_t port, int32_t* out_success) {
    if (!ip || !out_success) {
        set_global_error("ip or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() { *out_success = handle->primary->connect(ip, port) ? 1 : 0; });
}

elite_c_status_t elite_primary_disconnect(elite_primary_handle_t* handle) {
    return run_with_handle(handle, [&]() { handle->primary->disconnect(); });
}

elite_c_status_t elite_primary_send_script(elite_primary_handle_t* handle, const char* script, int32_t* out_success) {
    if (!script || !out_success) {
        set_global_error("script or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() { *out_success = handle->primary->sendScript(script) ? 1 : 0; });
}

elite_c_status_t elite_primary_get_local_ip(elite_primary_handle_t* handle, char* out_buffer, int32_t buffer_len,
                                            int32_t* out_required_len) {
    if (!handle) {
        set_global_error("handle is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        const std::string value = handle->primary->getLocalIP();
        const elite_c_status_t status = copy_string_to_buffer(value, out_buffer, buffer_len, out_required_len);
        if (status != ELITE_C_STATUS_OK) {
            set_handle_error(handle, g_last_error);
        }
        return status;
    } catch (const std::exception& e) {
        set_handle_error(handle, e.what());
        return ELITE_C_STATUS_EXCEPTION;
    } catch (...) {
        set_handle_error(handle, "unknown exception");
        return ELITE_C_STATUS_EXCEPTION;
    }
}

elite_c_status_t elite_primary_get_kinematics_info(elite_primary_handle_t* handle, int32_t timeout_ms, double* out_dh_a6,
                                                   double* out_dh_d6, double* out_dh_alpha6, int32_t* out_success) {
    if (!out_dh_a6 || !out_dh_d6 || !out_dh_alpha6 || !out_success) {
        set_global_error("output buffers or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() {
        auto pkg = std::make_shared<ELITE::KinematicsInfo>();
        const bool ok = handle->primary->getPackage(pkg, timeout_ms);
        *out_success = ok ? 1 : 0;
        if (ok) {
            copy_vector6_to_buffer(pkg->dh_a_, out_dh_a6);
            copy_vector6_to_buffer(pkg->dh_d_, out_dh_d6);
            copy_vector6_to_buffer(pkg->dh_alpha_, out_dh_alpha6);
        }
    });
}

elite_c_status_t elite_primary_register_robot_exception_callback(elite_primary_handle_t* handle,
                                                                 elite_primary_robot_exception_cb_t cb, void* user_data) {
    if (!cb) {
        set_global_error("callback is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() {
        {
            std::lock_guard<std::mutex> lock(handle->callback_state->mutex);
            handle->callback_state->cb = cb;
            handle->callback_state->user_data = user_data;
        }

        auto state = handle->callback_state;
        handle->primary->registerRobotExceptionCallback([state](ELITE::RobotExceptionSharedPtr ex) {
            if (!ex) {
                return;
            }

            elite_primary_robot_exception_cb_t local_cb = nullptr;
            void* local_user_data = nullptr;
            {
                std::lock_guard<std::mutex> lock(state->mutex);
                local_cb = state->cb;
                local_user_data = state->user_data;
            }
            if (!local_cb) {
                return;
            }

            elite_primary_robot_exception_t payload{};
            std::string message_buf;
            fill_robot_exception_payload(ex, &payload, &message_buf);
            local_cb(&payload, local_user_data);
        });
    });
}
