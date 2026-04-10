// SPDX-License-Identifier: MIT
#include <Elite/RtsiIOInterface.hpp>
#include <Rtsi/Elite_Rtsi_C.h>

#include <memory>
#include <mutex>
#include <new>
#include <sstream>
#include <string>
#include <vector>
#include <cstring>

struct elite_rtsi_io_handle_t {
    std::unique_ptr<ELITE::RtsiIOInterface> io;
    std::string last_error;
    std::mutex mutex;
};

namespace {
thread_local std::string g_last_error;

inline void set_global_error(const std::string& msg) { g_last_error = msg; }

inline void set_io_error(elite_rtsi_io_handle_t* handle, const std::string& msg) {
    if (!handle) {
        set_global_error(msg);
        return;
    }
    std::lock_guard<std::mutex> lock(handle->mutex);
    handle->last_error = msg;
    g_last_error = msg;
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

inline std::vector<std::string> split_csv(const char* recipe_csv) {
    std::vector<std::string> out;
    if (!recipe_csv) {
        return out;
    }

    std::stringstream ss(recipe_csv);
    std::string item;
    while (std::getline(ss, item, ',')) {
        size_t start = 0;
        while (start < item.size() && (item[start] == ' ' || item[start] == '\t' || item[start] == '\n' || item[start] == '\r')) {
            ++start;
        }
        size_t end = item.size();
        while (end > start &&
               (item[end - 1] == ' ' || item[end - 1] == '\t' || item[end - 1] == '\n' || item[end - 1] == '\r')) {
            --end;
        }
        if (end > start) {
            out.emplace_back(item.substr(start, end - start));
        }
    }
    return out;
}

inline ELITE::vector6d_t to_vector6d(const double* data6) {
    ELITE::vector6d_t out{};
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

inline void copy_vector3_to_buffer(const ELITE::vector3d_t& src, double* dst3) {
    for (size_t i = 0; i < src.size(); ++i) {
        dst3[i] = src[i];
    }
}

template <typename Fn>
elite_c_status_t run_with_io(elite_rtsi_io_handle_t* handle, Fn&& fn) {
    if (!handle) {
        set_global_error("handle is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        fn();
        return ELITE_C_STATUS_OK;
    } catch (const std::exception& e) {
        set_io_error(handle, e.what());
        return ELITE_C_STATUS_EXCEPTION;
    } catch (...) {
        set_io_error(handle, "unknown exception");
        return ELITE_C_STATUS_EXCEPTION;
    }
}

}  // namespace

elite_c_status_t elite_rtsi_io_create(const char* output_recipe_csv, const char* input_recipe_csv, double frequency,
                                     elite_rtsi_io_handle_t** out_handle) {
    if (!out_handle) {
        set_global_error("out_handle is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        auto handle = std::make_unique<elite_rtsi_io_handle_t>();
        const auto out_list = split_csv(output_recipe_csv ? output_recipe_csv : "");
        const auto in_list = split_csv(input_recipe_csv ? input_recipe_csv : "");
        handle->io = std::make_unique<ELITE::RtsiIOInterface>(out_list, in_list, frequency);
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

void elite_rtsi_io_destroy(elite_rtsi_io_handle_t* handle) { delete handle; }

elite_c_status_t elite_rtsi_io_connect(elite_rtsi_io_handle_t* handle, const char* ip, int32_t* out_success) {
    if (!ip || !out_success) {
        set_global_error("ip or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_success = handle->io->connect(ip) ? 1 : 0; });
}

elite_c_status_t elite_rtsi_io_disconnect(elite_rtsi_io_handle_t* handle) {
    return run_with_io(handle, [&]() { handle->io->disconnect(); });
}

elite_c_status_t elite_rtsi_io_is_connected(elite_rtsi_io_handle_t* handle, int32_t* out_connected) {
    if (!out_connected) {
        set_global_error("out_connected is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_connected = handle->io->isConnected() ? 1 : 0; });
}

elite_c_status_t elite_rtsi_io_is_started(elite_rtsi_io_handle_t* handle, int32_t* out_started) {
    if (!out_started) {
        set_global_error("out_started is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_started = handle->io->isStarted() ? 1 : 0; });
}

elite_c_status_t elite_rtsi_io_get_controller_version(elite_rtsi_io_handle_t* handle, elite_version_info_t* out_version) {
    if (!out_version) {
        set_global_error("out_version is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() {
        const auto version = handle->io->getControllerVersion();
        out_version->major = version.major;
        out_version->minor = version.minor;
        out_version->bugfix = version.bugfix;
        out_version->build = version.build;
    });
}

elite_c_status_t elite_rtsi_io_set_speed_scaling(elite_rtsi_io_handle_t* handle, double scaling, int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_success = handle->io->setSpeedScaling(scaling) ? 1 : 0; });
}

elite_c_status_t elite_rtsi_io_set_standard_digital(elite_rtsi_io_handle_t* handle, int32_t index, int32_t level,
                                                    int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_success = handle->io->setStandardDigital(index, level != 0) ? 1 : 0; });
}

elite_c_status_t elite_rtsi_io_set_configure_digital(elite_rtsi_io_handle_t* handle, int32_t index, int32_t level,
                                                     int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_success = handle->io->setConfigureDigital(index, level != 0) ? 1 : 0; });
}

elite_c_status_t elite_rtsi_io_set_analog_output_voltage(elite_rtsi_io_handle_t* handle, int32_t index, double value,
                                                         int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_success = handle->io->setAnalogOutputVoltage(index, value) ? 1 : 0; });
}

elite_c_status_t elite_rtsi_io_set_analog_output_current(elite_rtsi_io_handle_t* handle, int32_t index, double value,
                                                         int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_success = handle->io->setAnalogOutputCurrent(index, value) ? 1 : 0; });
}

elite_c_status_t elite_rtsi_io_set_external_force_torque(elite_rtsi_io_handle_t* handle, const double* value6,
                                                         int32_t* out_success) {
    if (!value6 || !out_success) {
        set_global_error("value6 or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() {
        const auto value = to_vector6d(value6);
        *out_success = handle->io->setExternalForceTorque(value) ? 1 : 0;
    });
}

elite_c_status_t elite_rtsi_io_set_tool_digital_output(elite_rtsi_io_handle_t* handle, int32_t index, int32_t level,
                                                       int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_success = handle->io->setToolDigitalOutput(index, level != 0) ? 1 : 0; });
}

#define RTSI_IO_GET_SCALAR(FN, OUTPTR, EXPR)                  \
    if (!(OUTPTR)) {                                          \
        set_global_error(#OUTPTR " is null");                 \
        return ELITE_C_STATUS_INVALID_ARGUMENT;               \
    }                                                          \
    return run_with_io(handle, [&]() { *(OUTPTR) = (EXPR); });

elite_c_status_t elite_rtsi_io_get_timestamp(elite_rtsi_io_handle_t* handle, double* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_timestamp, out_value, handle->io->getTimestamp())
}
elite_c_status_t elite_rtsi_io_get_payload_mass(elite_rtsi_io_handle_t* handle, double* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_payload_mass, out_value, handle->io->getPayloadMass())
}
elite_c_status_t elite_rtsi_io_get_script_control_line(elite_rtsi_io_handle_t* handle, uint32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_script_control_line, out_value, handle->io->getScriptControlLine())
}
elite_c_status_t elite_rtsi_io_get_digital_input_bits(elite_rtsi_io_handle_t* handle, uint32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_digital_input_bits, out_value, handle->io->getDigitalInputBits())
}
elite_c_status_t elite_rtsi_io_get_digital_output_bits(elite_rtsi_io_handle_t* handle, uint32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_digital_output_bits, out_value, handle->io->getDigitalOutputBits())
}
elite_c_status_t elite_rtsi_io_get_robot_mode(elite_rtsi_io_handle_t* handle, int32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_robot_mode, out_value, static_cast<int32_t>(handle->io->getRobotMode()))
}
elite_c_status_t elite_rtsi_io_get_safety_status(elite_rtsi_io_handle_t* handle, int32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_safety_status, out_value, static_cast<int32_t>(handle->io->getSafetyStatus()))
}
elite_c_status_t elite_rtsi_io_get_robot_status(elite_rtsi_io_handle_t* handle, uint32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_robot_status, out_value, handle->io->getRobotStatus())
}
elite_c_status_t elite_rtsi_io_get_runtime_state(elite_rtsi_io_handle_t* handle, int32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_runtime_state, out_value, static_cast<int32_t>(handle->io->getRuntimeState()))
}
elite_c_status_t elite_rtsi_io_get_actual_speed_scaling(elite_rtsi_io_handle_t* handle, double* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_actual_speed_scaling, out_value, handle->io->getActualSpeedScaling())
}
elite_c_status_t elite_rtsi_io_get_target_speed_scaling(elite_rtsi_io_handle_t* handle, double* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_target_speed_scaling, out_value, handle->io->getTargetSpeedScaling())
}
elite_c_status_t elite_rtsi_io_get_robot_voltage(elite_rtsi_io_handle_t* handle, double* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_robot_voltage, out_value, handle->io->getRobotVoltage())
}
elite_c_status_t elite_rtsi_io_get_robot_current(elite_rtsi_io_handle_t* handle, double* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_robot_current, out_value, handle->io->getRobotCurrent())
}
elite_c_status_t elite_rtsi_io_get_safety_status_bits(elite_rtsi_io_handle_t* handle, uint32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_safety_status_bits, out_value, handle->io->getSafetyStatusBits())
}
elite_c_status_t elite_rtsi_io_get_analog_io_types(elite_rtsi_io_handle_t* handle, uint32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_analog_io_types, out_value, handle->io->getAnalogIOTypes())
}
elite_c_status_t elite_rtsi_io_get_io_current(elite_rtsi_io_handle_t* handle, double* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_io_current, out_value, handle->io->getIOCurrent())
}
elite_c_status_t elite_rtsi_io_get_tool_mode(elite_rtsi_io_handle_t* handle, uint32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_tool_mode, out_value, static_cast<uint32_t>(handle->io->getToolMode()))
}
elite_c_status_t elite_rtsi_io_get_tool_analog_input_type(elite_rtsi_io_handle_t* handle, uint32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_tool_analog_input_type, out_value, handle->io->getToolAnalogInputType())
}
elite_c_status_t elite_rtsi_io_get_tool_analog_output_type(elite_rtsi_io_handle_t* handle, uint32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_tool_analog_output_type, out_value, handle->io->getToolAnalogOutputType())
}
elite_c_status_t elite_rtsi_io_get_tool_analog_input(elite_rtsi_io_handle_t* handle, double* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_tool_analog_input, out_value, handle->io->getToolAnalogInput())
}
elite_c_status_t elite_rtsi_io_get_tool_analog_output(elite_rtsi_io_handle_t* handle, double* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_tool_analog_output, out_value, handle->io->getToolAnalogOutput())
}
elite_c_status_t elite_rtsi_io_get_tool_output_voltage(elite_rtsi_io_handle_t* handle, double* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_tool_output_voltage, out_value, handle->io->getToolOutputVoltage())
}
elite_c_status_t elite_rtsi_io_get_tool_output_current(elite_rtsi_io_handle_t* handle, double* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_tool_output_current, out_value, handle->io->getToolOutputCurrent())
}
elite_c_status_t elite_rtsi_io_get_tool_output_temperature(elite_rtsi_io_handle_t* handle, double* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_tool_output_temperature, out_value, handle->io->getToolOutputTemperature())
}
elite_c_status_t elite_rtsi_io_get_tool_digital_mode(elite_rtsi_io_handle_t* handle, uint32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_tool_digital_mode, out_value, static_cast<uint32_t>(handle->io->getToolDigitalMode()))
}
elite_c_status_t elite_rtsi_io_get_out_bool_registers0_to_31(elite_rtsi_io_handle_t* handle, uint32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_out_bool_registers0_to_31, out_value, handle->io->getOutBoolRegisters0To31())
}
elite_c_status_t elite_rtsi_io_get_out_bool_registers32_to_63(elite_rtsi_io_handle_t* handle, uint32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_out_bool_registers32_to_63, out_value, handle->io->getOutBoolRegisters32To63())
}
elite_c_status_t elite_rtsi_io_get_in_bool_registers0_to_31(elite_rtsi_io_handle_t* handle, uint32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_in_bool_registers0_to_31, out_value, handle->io->getInBoolRegisters0To31())
}
elite_c_status_t elite_rtsi_io_get_in_bool_registers32_to_63(elite_rtsi_io_handle_t* handle, uint32_t* out_value) {
    RTSI_IO_GET_SCALAR(elite_rtsi_io_get_in_bool_registers32_to_63, out_value, handle->io->getInBoolRegisters32To63())
}

#undef RTSI_IO_GET_SCALAR

elite_c_status_t elite_rtsi_io_get_payload_cog(elite_rtsi_io_handle_t* handle, double* out_value3) {
    if (!out_value3) {
        set_global_error("out_value3 is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { copy_vector3_to_buffer(handle->io->getPayloadCog(), out_value3); });
}

elite_c_status_t elite_rtsi_io_get_target_joint_positions(elite_rtsi_io_handle_t* handle, double* out_value6) {
    if (!out_value6) {
        set_global_error("out_value6 is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { copy_vector6_to_buffer(handle->io->getTargetJointPositions(), out_value6); });
}
elite_c_status_t elite_rtsi_io_get_target_joint_velocity(elite_rtsi_io_handle_t* handle, double* out_value6) {
    if (!out_value6) {
        set_global_error("out_value6 is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { copy_vector6_to_buffer(handle->io->getTargetJointVelocity(), out_value6); });
}
elite_c_status_t elite_rtsi_io_get_actual_joint_positions(elite_rtsi_io_handle_t* handle, double* out_value6) {
    if (!out_value6) {
        set_global_error("out_value6 is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { copy_vector6_to_buffer(handle->io->getActualJointPositions(), out_value6); });
}
elite_c_status_t elite_rtsi_io_get_actual_joint_torques(elite_rtsi_io_handle_t* handle, double* out_value6) {
    if (!out_value6) {
        set_global_error("out_value6 is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { copy_vector6_to_buffer(handle->io->getActualJointTorques(), out_value6); });
}
elite_c_status_t elite_rtsi_io_get_actual_joint_velocity(elite_rtsi_io_handle_t* handle, double* out_value6) {
    if (!out_value6) {
        set_global_error("out_value6 is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { copy_vector6_to_buffer(handle->io->getActualJointVelocity(), out_value6); });
}
elite_c_status_t elite_rtsi_io_get_actual_joint_current(elite_rtsi_io_handle_t* handle, double* out_value6) {
    if (!out_value6) {
        set_global_error("out_value6 is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { copy_vector6_to_buffer(handle->io->getActualJointCurrent(), out_value6); });
}
elite_c_status_t elite_rtsi_io_get_actual_joint_temperatures(elite_rtsi_io_handle_t* handle, double* out_value6) {
    if (!out_value6) {
        set_global_error("out_value6 is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { copy_vector6_to_buffer(handle->io->getActualJointTemperatures(), out_value6); });
}
elite_c_status_t elite_rtsi_io_get_actual_tcp_pose(elite_rtsi_io_handle_t* handle, double* out_value6) {
    if (!out_value6) {
        set_global_error("out_value6 is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { copy_vector6_to_buffer(handle->io->getActualTCPPose(), out_value6); });
}
elite_c_status_t elite_rtsi_io_get_actual_tcp_velocity(elite_rtsi_io_handle_t* handle, double* out_value6) {
    if (!out_value6) {
        set_global_error("out_value6 is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { copy_vector6_to_buffer(handle->io->getActualTCPVelocity(), out_value6); });
}
elite_c_status_t elite_rtsi_io_get_actual_tcp_force(elite_rtsi_io_handle_t* handle, double* out_value6) {
    if (!out_value6) {
        set_global_error("out_value6 is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { copy_vector6_to_buffer(handle->io->getActualTCPForce(), out_value6); });
}
elite_c_status_t elite_rtsi_io_get_target_tcp_pose(elite_rtsi_io_handle_t* handle, double* out_value6) {
    if (!out_value6) {
        set_global_error("out_value6 is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { copy_vector6_to_buffer(handle->io->getTargetTCPPose(), out_value6); });
}
elite_c_status_t elite_rtsi_io_get_target_tcp_velocity(elite_rtsi_io_handle_t* handle, double* out_value6) {
    if (!out_value6) {
        set_global_error("out_value6 is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { copy_vector6_to_buffer(handle->io->getTargetTCPVelocity(), out_value6); });
}
elite_c_status_t elite_rtsi_io_get_joint_mode(elite_rtsi_io_handle_t* handle, int32_t* out_value6) {
    if (!out_value6) {
        set_global_error("out_value6 is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() {
        const auto mode = handle->io->getJointMode();
        for (size_t i = 0; i < mode.size(); ++i) {
            out_value6[i] = static_cast<int32_t>(mode[i]);
        }
    });
}
elite_c_status_t elite_rtsi_io_get_elbow_position(elite_rtsi_io_handle_t* handle, double* out_value3) {
    if (!out_value3) {
        set_global_error("out_value3 is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { copy_vector3_to_buffer(handle->io->getElbowPosition(), out_value3); });
}
elite_c_status_t elite_rtsi_io_get_elbow_velocity(elite_rtsi_io_handle_t* handle, double* out_value3) {
    if (!out_value3) {
        set_global_error("out_value3 is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { copy_vector3_to_buffer(handle->io->getElbowVelocity(), out_value3); });
}
elite_c_status_t elite_rtsi_io_get_analog_input(elite_rtsi_io_handle_t* handle, int32_t index, double* out_value) {
    if (!out_value) {
        set_global_error("out_value is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_value = handle->io->getAnalogInput(index); });
}
elite_c_status_t elite_rtsi_io_get_analog_output(elite_rtsi_io_handle_t* handle, int32_t index, double* out_value) {
    if (!out_value) {
        set_global_error("out_value is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_value = handle->io->getAnalogOutput(index); });
}
elite_c_status_t elite_rtsi_io_get_tool_digital_output_mode(elite_rtsi_io_handle_t* handle, int32_t index, uint32_t* out_value) {
    if (!out_value) {
        set_global_error("out_value is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_value = static_cast<uint32_t>(handle->io->getToolDigitalOutputMode(index)); });
}
elite_c_status_t elite_rtsi_io_get_in_bool_register(elite_rtsi_io_handle_t* handle, int32_t index, int32_t* out_value) {
    if (!out_value) {
        set_global_error("out_value is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_value = handle->io->getInBoolRegister(index) ? 1 : 0; });
}
elite_c_status_t elite_rtsi_io_get_out_bool_register(elite_rtsi_io_handle_t* handle, int32_t index, int32_t* out_value) {
    if (!out_value) {
        set_global_error("out_value is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_value = handle->io->getOutBoolRegister(index) ? 1 : 0; });
}
elite_c_status_t elite_rtsi_io_get_in_int_register(elite_rtsi_io_handle_t* handle, int32_t index, int32_t* out_value) {
    if (!out_value) {
        set_global_error("out_value is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_value = handle->io->getInIntRegister(index); });
}
elite_c_status_t elite_rtsi_io_get_out_int_register(elite_rtsi_io_handle_t* handle, int32_t index, int32_t* out_value) {
    if (!out_value) {
        set_global_error("out_value is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_value = handle->io->getOutIntRegister(index); });
}
elite_c_status_t elite_rtsi_io_get_in_double_register(elite_rtsi_io_handle_t* handle, int32_t index, double* out_value) {
    if (!out_value) {
        set_global_error("out_value is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_value = handle->io->getInDoubleRegister(index); });
}
elite_c_status_t elite_rtsi_io_get_out_double_register(elite_rtsi_io_handle_t* handle, int32_t index, double* out_value) {
    if (!out_value) {
        set_global_error("out_value is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_value = handle->io->getOutDoubleRegister(index); });
}

elite_c_status_t elite_rtsi_io_get_recipe_value_double(elite_rtsi_io_handle_t* handle, const char* name, double* out_value,
                                                       int32_t* out_success) {
    if (!name || !out_value || !out_success) {
        set_global_error("name, out_value or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_success = handle->io->getRecipeValue(name, *out_value) ? 1 : 0; });
}
elite_c_status_t elite_rtsi_io_get_recipe_value_int32(elite_rtsi_io_handle_t* handle, const char* name, int32_t* out_value,
                                                      int32_t* out_success) {
    if (!name || !out_value || !out_success) {
        set_global_error("name, out_value or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_success = handle->io->getRecipeValue(name, *out_value) ? 1 : 0; });
}
elite_c_status_t elite_rtsi_io_get_recipe_value_uint32(elite_rtsi_io_handle_t* handle, const char* name, uint32_t* out_value,
                                                       int32_t* out_success) {
    if (!name || !out_value || !out_success) {
        set_global_error("name, out_value or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_success = handle->io->getRecipeValue(name, *out_value) ? 1 : 0; });
}
elite_c_status_t elite_rtsi_io_get_recipe_value_bool(elite_rtsi_io_handle_t* handle, const char* name, int32_t* out_value,
                                                     int32_t* out_success) {
    if (!name || !out_value || !out_success) {
        set_global_error("name, out_value or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() {
        bool value = false;
        const bool ok = handle->io->getRecipeValue(name, value);
        *out_success = ok ? 1 : 0;
        *out_value = value ? 1 : 0;
    });
}
elite_c_status_t elite_rtsi_io_get_recipe_value_vector3d(elite_rtsi_io_handle_t* handle, const char* name, double* out_value3,
                                                         int32_t* out_success) {
    if (!name || !out_value3 || !out_success) {
        set_global_error("name, out_value3 or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() {
        ELITE::vector3d_t value{};
        const bool ok = handle->io->getRecipeValue(name, value);
        *out_success = ok ? 1 : 0;
        if (ok) {
            copy_vector3_to_buffer(value, out_value3);
        }
    });
}
elite_c_status_t elite_rtsi_io_get_recipe_value_vector6d(elite_rtsi_io_handle_t* handle, const char* name, double* out_value6,
                                                         int32_t* out_success) {
    if (!name || !out_value6 || !out_success) {
        set_global_error("name, out_value6 or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() {
        ELITE::vector6d_t value{};
        const bool ok = handle->io->getRecipeValue(name, value);
        *out_success = ok ? 1 : 0;
        if (ok) {
            copy_vector6_to_buffer(value, out_value6);
        }
    });
}
elite_c_status_t elite_rtsi_io_set_input_recipe_value_double(elite_rtsi_io_handle_t* handle, const char* name, double value,
                                                             int32_t* out_success) {
    if (!name || !out_success) {
        set_global_error("name or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_success = handle->io->setInputRecipeValue(name, value) ? 1 : 0; });
}
elite_c_status_t elite_rtsi_io_set_input_recipe_value_int32(elite_rtsi_io_handle_t* handle, const char* name, int32_t value,
                                                            int32_t* out_success) {
    if (!name || !out_success) {
        set_global_error("name or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_success = handle->io->setInputRecipeValue(name, value) ? 1 : 0; });
}
elite_c_status_t elite_rtsi_io_set_input_recipe_value_uint32(elite_rtsi_io_handle_t* handle, const char* name, uint32_t value,
                                                             int32_t* out_success) {
    if (!name || !out_success) {
        set_global_error("name or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_success = handle->io->setInputRecipeValue(name, value) ? 1 : 0; });
}
elite_c_status_t elite_rtsi_io_set_input_recipe_value_bool(elite_rtsi_io_handle_t* handle, const char* name, int32_t value,
                                                           int32_t* out_success) {
    if (!name || !out_success) {
        set_global_error("name or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() { *out_success = handle->io->setInputRecipeValue(name, value != 0) ? 1 : 0; });
}
elite_c_status_t elite_rtsi_io_set_input_recipe_value_vector6d(elite_rtsi_io_handle_t* handle, const char* name,
                                                               const double* value6, int32_t* out_success) {
    if (!name || !value6 || !out_success) {
        set_global_error("name, value6 or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_io(handle, [&]() {
        const auto value = to_vector6d(value6);
        *out_success = handle->io->setInputRecipeValue(name, value) ? 1 : 0;
    });
}

const char* elite_rtsi_io_last_error_message(elite_rtsi_io_handle_t* handle) {
    if (!handle) {
        return "handle is null";
    }
    std::lock_guard<std::mutex> lock(handle->mutex);
    return handle->last_error.c_str();
}
