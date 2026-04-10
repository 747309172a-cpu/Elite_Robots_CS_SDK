// SPDX-License-Identifier: MIT
#include <Elite/Elite_Serialcommunication_C.hpp>
#include "Elite_Serialcommunication_Internal.hpp"

#include <exception>
#include <string>

namespace {
thread_local std::string g_last_error;

inline void set_global_error(const std::string& msg) { g_last_error = msg; }

template <typename Fn>
elite_c_status_t run_with_serial(elite_serial_comm_handle_t* comm, Fn&& fn) {
    if (!comm || !comm->comm) {
        set_global_error("comm is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        fn();
        return ELITE_C_STATUS_OK;
    } catch (const std::exception& e) {
        set_global_error(e.what());
        return ELITE_C_STATUS_EXCEPTION;
    } catch (...) {
        set_global_error("unknown exception");
        return ELITE_C_STATUS_EXCEPTION;
    }
}
}  // namespace

elite_c_status_t elite_serial_comm_connect(elite_serial_comm_handle_t* comm, int32_t timeout_ms, int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_serial(comm, [&]() { *out_success = comm->comm->connect(timeout_ms) ? 1 : 0; });
}

elite_c_status_t elite_serial_comm_disconnect(elite_serial_comm_handle_t* comm) {
    return run_with_serial(comm, [&]() { comm->comm->disconnect(); });
}

elite_c_status_t elite_serial_comm_is_connected(elite_serial_comm_handle_t* comm, int32_t* out_connected) {
    if (!out_connected) {
        set_global_error("out_connected is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_serial(comm, [&]() { *out_connected = comm->comm->isConnected() ? 1 : 0; });
}

elite_c_status_t elite_serial_comm_get_socat_pid(elite_serial_comm_handle_t* comm, int32_t* out_pid) {
    if (!out_pid) {
        set_global_error("out_pid is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_serial(comm, [&]() { *out_pid = comm->comm->getSocatPid(); });
}

elite_c_status_t elite_serial_comm_write(elite_serial_comm_handle_t* comm, const uint8_t* data, int32_t size,
                                         int32_t* out_written) {
    if (!data || size < 0 || !out_written) {
        set_global_error("data, size or out_written is invalid");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_serial(comm, [&]() { *out_written = comm->comm->write(data, static_cast<size_t>(size)); });
}

elite_c_status_t elite_serial_comm_read(elite_serial_comm_handle_t* comm, uint8_t* out_data, int32_t size, int32_t timeout_ms,
                                        int32_t* out_read) {
    if (!out_data || size < 0 || !out_read) {
        set_global_error("out_data, size or out_read is invalid");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_serial(
        comm, [&]() { *out_read = comm->comm->read(out_data, static_cast<size_t>(size), timeout_ms); });
}

void elite_serial_comm_destroy(elite_serial_comm_handle_t* comm) { delete comm; }
