// SPDX-License-Identifier: MIT
#include <Elite/EliteDriver.hpp>
#include <Elite/RobotConfPackage.hpp>
#include <Elite/RobotException.hpp>
#include <Elite/Elite_Driver_C.h>
#include "Elite_Serialcommunication_Internal.hpp"

#include <cstring>
#include <memory>
#include <mutex>
#include <new>
#include <string>

struct callback_state_t {
    std::mutex mutex;
    elite_driver_trajectory_result_cb_t trajectory_cb{nullptr};
    void* trajectory_user_data{nullptr};
    elite_driver_robot_exception_cb_t robot_exception_cb{nullptr};
    void* robot_exception_user_data{nullptr};
};

struct elite_driver_handle_t {
    std::unique_ptr<ELITE::EliteDriver> driver;
    std::shared_ptr<callback_state_t> callback_state;
    std::string last_error;
    std::mutex mutex;
};

namespace {
thread_local std::string g_last_error;

inline void set_global_error(const std::string& msg) { g_last_error = msg; }

inline void set_handle_error(elite_driver_handle_t* handle, const std::string& msg) {
    if (!handle) {
        set_global_error(msg);
        return;
    }
    std::lock_guard<std::mutex> lock(handle->mutex);
    handle->last_error = msg;
    g_last_error = msg;
}

inline ELITE::vector6d_t to_vector6d(const double* data6) {
    ELITE::vector6d_t out{};
    for (size_t i = 0; i < out.size(); ++i) {
        out[i] = data6[i];
    }
    return out;
}

inline ELITE::vector3d_t to_vector3d(const double* data3) {
    ELITE::vector3d_t out{};
    for (size_t i = 0; i < out.size(); ++i) {
        out[i] = data3[i];
    }
    return out;
}

inline ELITE::vector6int32_t to_vector6i(const int32_t* data6) {
    ELITE::vector6int32_t out{};
    for (size_t i = 0; i < out.size(); ++i) {
        out[i] = data6[i];
    }
    return out;
}

inline void copy_vector6_to_buffer(const ELITE::vector6d_t& src, double* dst6) {
    for (size_t i = 0; i < src.size(); ++i) {
        dst6[i] = src[i];
    }
}

template <typename Fn>
elite_c_status_t run_with_handle(elite_driver_handle_t* handle, Fn&& fn) {
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

inline void fill_robot_exception_payload(const ELITE::RobotExceptionSharedPtr& ex, elite_driver_robot_exception_t* out_payload,
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

void elite_driver_config_set_default(elite_driver_config_t* config) {
    if (!config) {
        return;
    }

    config->robot_ip = nullptr;
    config->script_file_path = nullptr;
    config->local_ip = "";
    config->headless_mode = 0;
    config->script_sender_port = 50002;
    config->reverse_port = 50001;
    config->trajectory_port = 50003;
    config->script_command_port = 50004;
    config->servoj_time = 0.008f;
    config->servoj_lookahead_time = 0.1f;
    config->servoj_gain = 300;
    config->stopj_acc = 8.0f;
}

elite_c_status_t elite_driver_create(const elite_driver_config_t* config, elite_driver_handle_t** out_handle) {
    if (!config || !out_handle) {
        set_global_error("config or out_handle is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    if (!config->robot_ip || !config->script_file_path) {
        set_global_error("robot_ip and script_file_path must not be null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        auto handle = std::make_unique<elite_driver_handle_t>();

        ELITE::EliteDriverConfig driver_config;
        driver_config.robot_ip = config->robot_ip;
        driver_config.script_file_path = config->script_file_path;
        driver_config.local_ip = config->local_ip ? config->local_ip : "";
        driver_config.headless_mode = (config->headless_mode != 0);
        driver_config.script_sender_port = config->script_sender_port;
        driver_config.reverse_port = config->reverse_port;
        driver_config.trajectory_port = config->trajectory_port;
        driver_config.script_command_port = config->script_command_port;
        driver_config.servoj_time = config->servoj_time;
        driver_config.servoj_lookahead_time = config->servoj_lookahead_time;
        driver_config.servoj_gain = config->servoj_gain;
        driver_config.stopj_acc = config->stopj_acc;

        handle->driver = std::make_unique<ELITE::EliteDriver>(driver_config);
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

void elite_driver_destroy(elite_driver_handle_t* handle) {
    if (!handle) {
        return;
    }
    if (handle->driver) {
        handle->driver->setTrajectoryResultCallback(nullptr);
        handle->driver->registerRobotExceptionCallback(nullptr);
    }
    if (handle->callback_state) {
        std::lock_guard<std::mutex> lock(handle->callback_state->mutex);
        handle->callback_state->trajectory_cb = nullptr;
        handle->callback_state->trajectory_user_data = nullptr;
        handle->callback_state->robot_exception_cb = nullptr;
        handle->callback_state->robot_exception_user_data = nullptr;
    }
    delete handle;
}

elite_c_status_t elite_driver_is_robot_connected(elite_driver_handle_t* handle, int32_t* out_connected) {
    if (!out_connected) {
        set_global_error("out_connected is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() { *out_connected = handle->driver->isRobotConnected() ? 1 : 0; });
}

elite_c_status_t elite_driver_send_external_control_script(elite_driver_handle_t* handle, int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() { *out_success = handle->driver->sendExternalControlScript() ? 1 : 0; });
}

elite_c_status_t elite_driver_stop_control(elite_driver_handle_t* handle, int32_t wait_ms, int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() { *out_success = handle->driver->stopControl(wait_ms) ? 1 : 0; });
}

elite_c_status_t elite_driver_write_servoj(elite_driver_handle_t* handle, const double* pos6, int32_t timeout_ms,
                                           int32_t cartesian, int32_t* out_success) {
    if (!pos6 || !out_success) {
        set_global_error("pos6 or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() {
        const ELITE::vector6d_t pos = to_vector6d(pos6);
        *out_success = handle->driver->writeServoj(pos, timeout_ms, cartesian != 0) ? 1 : 0;
    });
}

elite_c_status_t elite_driver_write_speedj(elite_driver_handle_t* handle, const double* vel6, int32_t timeout_ms,
                                           int32_t* out_success) {
    if (!vel6 || !out_success) {
        set_global_error("vel6 or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() {
        const ELITE::vector6d_t vel = to_vector6d(vel6);
        *out_success = handle->driver->writeSpeedj(vel, timeout_ms) ? 1 : 0;
    });
}

elite_c_status_t elite_driver_write_speedl(elite_driver_handle_t* handle, const double* vel6, int32_t timeout_ms,
                                           int32_t* out_success) {
    if (!vel6 || !out_success) {
        set_global_error("vel6 or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() {
        const ELITE::vector6d_t vel = to_vector6d(vel6);
        *out_success = handle->driver->writeSpeedl(vel, timeout_ms) ? 1 : 0;
    });
}

elite_c_status_t elite_driver_write_idle(elite_driver_handle_t* handle, int32_t timeout_ms, int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() { *out_success = handle->driver->writeIdle(timeout_ms) ? 1 : 0; });
}

elite_c_status_t elite_driver_set_trajectory_result_callback(elite_driver_handle_t* handle,
                                                             elite_driver_trajectory_result_cb_t cb, void* user_data) {
    return run_with_handle(handle, [&]() {
        {
            std::lock_guard<std::mutex> lock(handle->callback_state->mutex);
            handle->callback_state->trajectory_cb = cb;
            handle->callback_state->trajectory_user_data = user_data;
        }

        if (!cb) {
            handle->driver->setTrajectoryResultCallback(nullptr);
            return;
        }

        auto state = handle->callback_state;
        handle->driver->setTrajectoryResultCallback([state](ELITE::TrajectoryMotionResult result) {
            elite_driver_trajectory_result_cb_t local_cb = nullptr;
            void* local_user_data = nullptr;
            {
                std::lock_guard<std::mutex> lock(state->mutex);
                local_cb = state->trajectory_cb;
                local_user_data = state->trajectory_user_data;
            }
            if (!local_cb) {
                return;
            }
            local_cb(static_cast<int32_t>(result), local_user_data);
        });
    });
}

elite_c_status_t elite_driver_write_trajectory_point(elite_driver_handle_t* handle, const double* positions6, float time,
                                                     float blend_radius, int32_t cartesian, int32_t* out_success) {
    if (!positions6 || !out_success) {
        set_global_error("positions6 or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() {
        const ELITE::vector6d_t positions = to_vector6d(positions6);
        *out_success = handle->driver->writeTrajectoryPoint(positions, time, blend_radius, cartesian != 0) ? 1 : 0;
    });
}

elite_c_status_t elite_driver_write_trajectory_control_action(elite_driver_handle_t* handle, int32_t action,
                                                              int32_t point_number, int32_t timeout_ms,
                                                              int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() {
        *out_success =
            handle->driver
                ->writeTrajectoryControlAction(static_cast<ELITE::TrajectoryControlAction>(action), point_number, timeout_ms)
            ? 1
            : 0;
    });
}

elite_c_status_t elite_driver_write_freedrive(elite_driver_handle_t* handle, int32_t action, int32_t timeout_ms,
                                              int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() {
        *out_success = handle->driver->writeFreedrive(static_cast<ELITE::FreedriveAction>(action), timeout_ms) ? 1 : 0;
    });
}

elite_c_status_t elite_driver_zero_ft_sensor(elite_driver_handle_t* handle, int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() { *out_success = handle->driver->zeroFTSensor() ? 1 : 0; });
}

elite_c_status_t elite_driver_set_payload(elite_driver_handle_t* handle, double mass, const double* cog3, int32_t* out_success) {
    if (!cog3 || !out_success) {
        set_global_error("cog3 or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() {
        const ELITE::vector3d_t cog = to_vector3d(cog3);
        *out_success = handle->driver->setPayload(mass, cog) ? 1 : 0;
    });
}

elite_c_status_t elite_driver_set_tool_voltage(elite_driver_handle_t* handle, int32_t voltage, int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() {
        *out_success = handle->driver->setToolVoltage(static_cast<ELITE::ToolVoltage>(voltage)) ? 1 : 0;
    });
}

elite_c_status_t elite_driver_start_force_mode(elite_driver_handle_t* handle, const double* reference_frame6,
                                               const int32_t* selection_vector6, const double* wrench6, int32_t mode,
                                               const double* limits6, int32_t* out_success) {
    if (!reference_frame6 || !selection_vector6 || !wrench6 || !limits6 || !out_success) {
        set_global_error("input buffers or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() {
        const ELITE::vector6d_t reference_frame = to_vector6d(reference_frame6);
        const ELITE::vector6int32_t selection_vector = to_vector6i(selection_vector6);
        const ELITE::vector6d_t wrench = to_vector6d(wrench6);
        const ELITE::vector6d_t limits = to_vector6d(limits6);
        *out_success = handle->driver->startForceMode(reference_frame, selection_vector, wrench,
                                                      static_cast<ELITE::ForceMode>(mode), limits)
                           ? 1
                           : 0;
    });
}

elite_c_status_t elite_driver_end_force_mode(elite_driver_handle_t* handle, int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() { *out_success = handle->driver->endForceMode() ? 1 : 0; });
}

elite_c_status_t elite_driver_send_script(elite_driver_handle_t* handle, const char* script, int32_t* out_success) {
    if (!script || !out_success) {
        set_global_error("script or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() { *out_success = handle->driver->sendScript(script) ? 1 : 0; });
}

elite_c_status_t elite_driver_get_primary_package(elite_driver_handle_t* handle, int32_t timeout_ms, double* out_dh_a6,
                                                  double* out_dh_d6, double* out_dh_alpha6, int32_t* out_success) {
    if (!out_dh_a6 || !out_dh_d6 || !out_dh_alpha6 || !out_success) {
        set_global_error("output buffers or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() {
        auto pkg = std::make_shared<ELITE::KinematicsInfo>();
        const bool ok = handle->driver->getPrimaryPackage(pkg, timeout_ms);
        *out_success = ok ? 1 : 0;
        if (ok) {
            copy_vector6_to_buffer(pkg->dh_a_, out_dh_a6);
            copy_vector6_to_buffer(pkg->dh_d_, out_dh_d6);
            copy_vector6_to_buffer(pkg->dh_alpha_, out_dh_alpha6);
        }
    });
}

elite_c_status_t elite_driver_primary_reconnect(elite_driver_handle_t* handle, int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() { *out_success = handle->driver->primaryReconnect() ? 1 : 0; });
}

elite_c_status_t elite_driver_register_robot_exception_callback(elite_driver_handle_t* handle,
                                                                elite_driver_robot_exception_cb_t cb, void* user_data) {
    return run_with_handle(handle, [&]() {
        {
            std::lock_guard<std::mutex> lock(handle->callback_state->mutex);
            handle->callback_state->robot_exception_cb = cb;
            handle->callback_state->robot_exception_user_data = user_data;
        }

        if (!cb) {
            handle->driver->registerRobotExceptionCallback(nullptr);
            return;
        }

        auto state = handle->callback_state;
        handle->driver->registerRobotExceptionCallback([state](ELITE::RobotExceptionSharedPtr ex) {
            if (!ex) {
                return;
            }

            elite_driver_robot_exception_cb_t local_cb = nullptr;
            void* local_user_data = nullptr;
            {
                std::lock_guard<std::mutex> lock(state->mutex);
                local_cb = state->robot_exception_cb;
                local_user_data = state->robot_exception_user_data;
            }
            if (!local_cb) {
                return;
            }

            elite_driver_robot_exception_t payload{};
            std::string message_buf;
            fill_robot_exception_payload(ex, &payload, &message_buf);
            local_cb(&payload, local_user_data);
        });
    });
}

elite_c_status_t elite_driver_start_tool_rs485(elite_driver_handle_t* handle, const elite_serial_config_t* config,
                                               const char* ssh_password, int32_t tcp_port, elite_serial_comm_handle_t** out_comm) {
    if (!config || !out_comm) {
        set_global_error("config or out_comm is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() {
        ELITE::SerialConfig serial_config;
        serial_config.baud_rate = static_cast<ELITE::SerialConfig::BaudRate>(config->baud_rate);
        serial_config.parity = static_cast<ELITE::SerialConfig::Parity>(config->parity);
        serial_config.stop_bits = static_cast<ELITE::SerialConfig::StopBits>(config->stop_bits);

        auto comm = handle->driver->startToolRs485(serial_config, ssh_password ? ssh_password : "", tcp_port);
        if (!comm) {
            *out_comm = nullptr;
            return;
        }

        auto comm_handle = std::make_unique<elite_serial_comm_handle_t>();
        comm_handle->comm = comm;
        *out_comm = comm_handle.release();
    });
}

elite_c_status_t elite_driver_end_tool_rs485(elite_driver_handle_t* handle, elite_serial_comm_handle_t* comm,
                                             const char* ssh_password, int32_t* out_success) {
    if (!comm || !out_success) {
        set_global_error("comm or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() {
        *out_success = handle->driver->endToolRs485(comm->comm, ssh_password ? ssh_password : "") ? 1 : 0;
    });
}

elite_c_status_t elite_driver_start_board_rs485(elite_driver_handle_t* handle, const elite_serial_config_t* config,
                                                const char* ssh_password, int32_t tcp_port,
                                                elite_serial_comm_handle_t** out_comm) {
    if (!config || !out_comm) {
        set_global_error("config or out_comm is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() {
        ELITE::SerialConfig serial_config;
        serial_config.baud_rate = static_cast<ELITE::SerialConfig::BaudRate>(config->baud_rate);
        serial_config.parity = static_cast<ELITE::SerialConfig::Parity>(config->parity);
        serial_config.stop_bits = static_cast<ELITE::SerialConfig::StopBits>(config->stop_bits);

        auto comm = handle->driver->startBoardRs485(serial_config, ssh_password ? ssh_password : "", tcp_port);
        if (!comm) {
            *out_comm = nullptr;
            return;
        }

        auto comm_handle = std::make_unique<elite_serial_comm_handle_t>();
        comm_handle->comm = comm;
        *out_comm = comm_handle.release();
    });
}

elite_c_status_t elite_driver_end_board_rs485(elite_driver_handle_t* handle, elite_serial_comm_handle_t* comm,
                                              const char* ssh_password, int32_t* out_success) {
    if (!comm || !out_success) {
        set_global_error("comm or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_handle(handle, [&]() {
        *out_success = handle->driver->endBoardRs485(comm->comm, ssh_password ? ssh_password : "") ? 1 : 0;
    });
}

const char* elite_driver_last_error_message(elite_driver_handle_t* handle) {
    if (!handle) {
        return "handle is null";
    }

    std::lock_guard<std::mutex> lock(handle->mutex);
    return handle->last_error.c_str();
}

const char* elite_c_last_error_message(void) { return g_last_error.c_str(); }
