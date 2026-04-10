// SPDX-License-Identifier: MIT
#include <Dashboard/Elite_Dashboard_C.h>
#include <Elite/DashboardClient.hpp>

#include <cstring>
#include <memory>
#include <mutex>
#include <new>
#include <string>

struct elite_dashboard_handle_t {
    std::unique_ptr<ELITE::DashboardClient> dashboard;
    std::string last_error;
    std::mutex mutex;
};

namespace {
thread_local std::string g_last_error;


inline void set_global_error(const std::string& msg) { g_last_error = msg; }

inline void set_handle_error(elite_dashboard_handle_t* handle, const std::string& msg) {
    if (!handle) {
        set_global_error(msg);
        return;
    }
    std::lock_guard<std::mutex> lock(handle->mutex);
    handle->last_error = msg;
    g_last_error = msg;
}

// Helper function to run a function with error handling and handle checking
template <typename Fn>
elite_c_status_t run_with_handle(elite_dashboard_handle_t* handle, Fn&& fn) {
    if (!handle) {
        set_global_error("handle is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        fn(); //执行传入的lambda函数，lambda函数中包含了具体的操作逻辑，比如调用dashboard的方法等
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

template <typename Fn>
elite_c_status_t run_bool_out(elite_dashboard_handle_t* handle, int32_t* out_success, const char* null_msg, Fn&& fn) {
    if (!out_success) {
        set_global_error(null_msg);
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_handle(handle, [&]() { *out_success = fn() ? 1 : 0; });
}

// Similar to run_bool_out but for int32_t output
template <typename Fn>
elite_c_status_t run_int_out(elite_dashboard_handle_t* handle, int32_t* out_value, const char* null_msg, Fn&& fn) {
    if (!out_value) {
        set_global_error(null_msg);
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_handle(handle, [&]() { *out_value = fn(); });
}

//返回值为string类型的c++接口，c中需要转换保存在out_buffer中
template <typename Fn>
elite_c_status_t run_string_out(elite_dashboard_handle_t* handle, char* out_buffer, int32_t buffer_len, int32_t* out_required_len,
                                Fn&& fn) {
    if (!handle) {
        set_global_error("handle is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        const std::string value = fn();
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
}  // namespace


elite_c_status_t elite_dashboard_create(elite_dashboard_handle_t** out_handle) {
    if (!out_handle) {
        set_global_error("out_handle is null");   //错误提示写到全局错误区
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    //异常必须捕获，不能让异常抛出到c层，否则会导致程序崩溃，错误提示写到handle的错误区和全局错误区
    try {
        auto handle = std::make_unique<elite_dashboard_handle_t>();
        handle->dashboard = std::make_unique<ELITE::DashboardClient>();
        *out_handle = handle.release(); //将指针所有权还给调用者，c中无unique_ptr，只有裸指针
        set_global_error("");  //清空上次的错误内容
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

void elite_dashboard_destroy(elite_dashboard_handle_t* handle) { delete handle; }

elite_c_status_t elite_dashboard_connect(elite_dashboard_handle_t* handle, const char* ip, int32_t port, int32_t* out_success) {
    if (!ip || !out_success) {
        set_global_error("ip or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() { *out_success = handle->dashboard->connect(ip, port) ? 1 : 0; });
}

elite_c_status_t elite_dashboard_disconnect(elite_dashboard_handle_t* handle) {
    return run_with_handle(handle, [&]() { handle->dashboard->disconnect(); });
}

elite_c_status_t elite_dashboard_echo(elite_dashboard_handle_t* handle, int32_t* out_success) {
    return run_bool_out(handle, out_success, "out_success is null", [&]() { return handle->dashboard->echo(); });
}

elite_c_status_t elite_dashboard_power_on(elite_dashboard_handle_t* handle, int32_t* out_success) {
    return run_bool_out(handle, out_success, "out_success is null", [&]() { return handle->dashboard->powerOn(); });
}

elite_c_status_t elite_dashboard_power_off(elite_dashboard_handle_t* handle, int32_t* out_success) {
    return run_bool_out(handle, out_success, "out_success is null", [&]() { return handle->dashboard->powerOff(); });
}

elite_c_status_t elite_dashboard_brake_release(elite_dashboard_handle_t* handle, int32_t* out_success) {
    return run_bool_out(handle, out_success, "out_success is null", [&]() { return handle->dashboard->brakeRelease(); });
}

elite_c_status_t elite_dashboard_unlock_protective_stop(elite_dashboard_handle_t* handle, int32_t* out_success) {
    return run_bool_out(handle, out_success, "out_success is null", [&]() { return handle->dashboard->unlockProtectiveStop(); });
}

elite_c_status_t elite_dashboard_robot_mode(elite_dashboard_handle_t* handle, int32_t* out_mode) {
    return run_int_out(handle, out_mode, "out_mode is null",
                       [&]() { return static_cast<int32_t>(handle->dashboard->robotMode()); });
}

elite_c_status_t elite_dashboard_safety_mode(elite_dashboard_handle_t* handle, int32_t* out_mode) {
    return run_int_out(handle, out_mode, "out_mode is null",
                       [&]() { return static_cast<int32_t>(handle->dashboard->safetyMode()); });
}

elite_c_status_t elite_dashboard_running_status(elite_dashboard_handle_t* handle, int32_t* out_status) {
    return run_int_out(handle, out_status, "out_status is null",
                       [&]() { return static_cast<int32_t>(handle->dashboard->runningStatus()); });
}

elite_c_status_t elite_dashboard_version(elite_dashboard_handle_t* handle, char* out_buffer, int32_t buffer_len,
                                         int32_t* out_required_len) {
    return run_string_out(handle, out_buffer, buffer_len, out_required_len, [&]() { return handle->dashboard->version(); });
}

elite_c_status_t elite_dashboard_close_safety_dialog(elite_dashboard_handle_t* handle, int32_t* out_success) {
    return run_bool_out(handle, out_success, "out_success is null", [&]() { return handle->dashboard->closeSafetyDialog(); });
}

elite_c_status_t elite_dashboard_safety_system_restart(elite_dashboard_handle_t* handle, int32_t* out_success) {
    return run_bool_out(handle, out_success, "out_success is null", [&]() { return handle->dashboard->safetySystemRestart(); });
}

elite_c_status_t elite_dashboard_log(elite_dashboard_handle_t* handle, const char* message, int32_t* out_success) {
    if (!message) {
        set_global_error("message is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_bool_out(handle, out_success, "out_success is null", [&]() { return handle->dashboard->log(message); });
}

elite_c_status_t elite_dashboard_popup(elite_dashboard_handle_t* handle, const char* arg, const char* message,
                                       int32_t* out_success) {
    if (!arg) {
        set_global_error("arg is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_bool_out(handle, out_success, "out_success is null",
                        [&]() { return handle->dashboard->popup(arg, message ? message : ""); });
}

elite_c_status_t elite_dashboard_quit(elite_dashboard_handle_t* handle) {
    return run_with_handle(handle, [&]() { handle->dashboard->quit(); });
}

elite_c_status_t elite_dashboard_reboot(elite_dashboard_handle_t* handle) {
    return run_with_handle(handle, [&]() { handle->dashboard->reboot(); });
}

elite_c_status_t elite_dashboard_shutdown(elite_dashboard_handle_t* handle) {
    return run_with_handle(handle, [&]() { handle->dashboard->shutdown(); });
}

elite_c_status_t elite_dashboard_get_task_status(elite_dashboard_handle_t* handle, int32_t* out_status) {
    return run_int_out(handle, out_status, "out_status is null",
                       [&]() { return static_cast<int32_t>(handle->dashboard->getTaskStatus()); });
}

elite_c_status_t elite_dashboard_speed_scaling(elite_dashboard_handle_t* handle, int32_t* out_scaling) {
    return run_int_out(handle, out_scaling, "out_scaling is null", [&]() { return handle->dashboard->speedScaling(); });
}

elite_c_status_t elite_dashboard_set_speed_scaling(elite_dashboard_handle_t* handle, int32_t scaling, int32_t* out_success) {
    return run_bool_out(handle, out_success, "out_success is null", [&]() { return handle->dashboard->setSpeedScaling(scaling); });
}

elite_c_status_t elite_dashboard_task_is_running(elite_dashboard_handle_t* handle, int32_t* out_running) {
    return run_bool_out(handle, out_running, "out_running is null", [&]() { return handle->dashboard->taskIsRunning(); });
}

elite_c_status_t elite_dashboard_is_task_saved(elite_dashboard_handle_t* handle, int32_t* out_saved) {
    return run_bool_out(handle, out_saved, "out_saved is null", [&]() { return handle->dashboard->isTaskSaved(); });
}

elite_c_status_t elite_dashboard_play_program(elite_dashboard_handle_t* handle, int32_t* out_success) {
    return run_bool_out(handle, out_success, "out_success is null", [&]() { return handle->dashboard->playProgram(); });
}

elite_c_status_t elite_dashboard_pause_program(elite_dashboard_handle_t* handle, int32_t* out_success) {
    return run_bool_out(handle, out_success, "out_success is null", [&]() { return handle->dashboard->pauseProgram(); });
}

elite_c_status_t elite_dashboard_stop_program(elite_dashboard_handle_t* handle, int32_t* out_success) {
    return run_bool_out(handle, out_success, "out_success is null", [&]() { return handle->dashboard->stopProgram(); });
}

elite_c_status_t elite_dashboard_load_configuration(elite_dashboard_handle_t* handle, const char* path, int32_t* out_success) {
    if (!path) {
        set_global_error("path is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_bool_out(handle, out_success, "out_success is null", [&]() { return handle->dashboard->loadConfiguration(path); });
}

elite_c_status_t elite_dashboard_load_task(elite_dashboard_handle_t* handle, const char* path, int32_t* out_success) {
    if (!path) {
        set_global_error("path is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_bool_out(handle, out_success, "out_success is null", [&]() { return handle->dashboard->loadTask(path); });
}

elite_c_status_t elite_dashboard_is_configuration_modify(elite_dashboard_handle_t* handle, int32_t* out_modified) {
    return run_bool_out(handle, out_modified, "out_modified is null", [&]() { return handle->dashboard->isConfigurationModify(); });
}

elite_c_status_t elite_dashboard_help(elite_dashboard_handle_t* handle, const char* cmd, char* out_buffer, int32_t buffer_len,
                                      int32_t* out_required_len) {
    if (!cmd) {
        set_global_error("cmd is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_string_out(handle, out_buffer, buffer_len, out_required_len, [&]() { return handle->dashboard->help(cmd); });
}

elite_c_status_t elite_dashboard_usage(elite_dashboard_handle_t* handle, const char* cmd, char* out_buffer, int32_t buffer_len,
                                       int32_t* out_required_len) {
    if (!cmd) {
        set_global_error("cmd is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_string_out(handle, out_buffer, buffer_len, out_required_len, [&]() { return handle->dashboard->usage(cmd); });
}

elite_c_status_t elite_dashboard_robot_type(elite_dashboard_handle_t* handle, char* out_buffer, int32_t buffer_len,
                                            int32_t* out_required_len) {
    return run_string_out(handle, out_buffer, buffer_len, out_required_len, [&]() { return handle->dashboard->robotType(); });
}

elite_c_status_t elite_dashboard_robot_serial_number(elite_dashboard_handle_t* handle, char* out_buffer, int32_t buffer_len,
                                                     int32_t* out_required_len) {
    return run_string_out(handle, out_buffer, buffer_len, out_required_len,
                          [&]() { return handle->dashboard->robotSerialNumber(); });
}

elite_c_status_t elite_dashboard_robot_id(elite_dashboard_handle_t* handle, char* out_buffer, int32_t buffer_len,
                                          int32_t* out_required_len) {
    return run_string_out(handle, out_buffer, buffer_len, out_required_len, [&]() { return handle->dashboard->robotID(); });
}

elite_c_status_t elite_dashboard_configuration_path(elite_dashboard_handle_t* handle, char* out_buffer, int32_t buffer_len,
                                                    int32_t* out_required_len) {
    return run_string_out(handle, out_buffer, buffer_len, out_required_len,
                          [&]() { return handle->dashboard->configurationPath(); });
}

elite_c_status_t elite_dashboard_get_task_path(elite_dashboard_handle_t* handle, char* out_buffer, int32_t buffer_len,
                                               int32_t* out_required_len) {
    return run_string_out(handle, out_buffer, buffer_len, out_required_len, [&]() { return handle->dashboard->getTaskPath(); });
}

elite_c_status_t elite_dashboard_send_and_receive(elite_dashboard_handle_t* handle, const char* cmd, char* out_buffer,
                                                  int32_t buffer_len, int32_t* out_required_len) {
    if (!cmd) {
        set_global_error("cmd is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_string_out(handle, out_buffer, buffer_len, out_required_len,
                          [&]() { return handle->dashboard->sendAndReceive(cmd); });
}

const char* elite_dashboard_last_error_message(elite_dashboard_handle_t* handle) {
    if (!handle) {
        return g_last_error.c_str();
    }

    std::lock_guard<std::mutex> lock(handle->mutex);
    return handle->last_error.c_str();
}

const char* elite_dashboard_global_last_error_message(void) { return g_last_error.c_str(); }
