// SPDX-License-Identifier: MIT
// Copyright (c) 2026, Elite Robots.
//
// C ABI wrapper for Elite serial communication APIs.
#ifndef __ELITE_SERIALCOMMUNICATION_C_HPP__
#define __ELITE_SERIALCOMMUNICATION_C_HPP__

#include <Elite_C_Types.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef struct elite_serial_comm_handle_t elite_serial_comm_handle_t;

typedef struct elite_serial_config_t {
    int32_t baud_rate;
    int32_t parity;
    int32_t stop_bits;
} elite_serial_config_t;

ELITE_C_EXPORT elite_c_status_t elite_serial_comm_connect(elite_serial_comm_handle_t* comm, int32_t timeout_ms,
                                                          int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_serial_comm_disconnect(elite_serial_comm_handle_t* comm);
ELITE_C_EXPORT elite_c_status_t elite_serial_comm_is_connected(elite_serial_comm_handle_t* comm, int32_t* out_connected);
ELITE_C_EXPORT elite_c_status_t elite_serial_comm_get_socat_pid(elite_serial_comm_handle_t* comm, int32_t* out_pid);
ELITE_C_EXPORT elite_c_status_t elite_serial_comm_write(elite_serial_comm_handle_t* comm, const uint8_t* data, int32_t size,
                                                        int32_t* out_written);
ELITE_C_EXPORT elite_c_status_t elite_serial_comm_read(elite_serial_comm_handle_t* comm, uint8_t* out_data, int32_t size,
                                                       int32_t timeout_ms, int32_t* out_read);
ELITE_C_EXPORT void elite_serial_comm_destroy(elite_serial_comm_handle_t* comm);

#ifdef __cplusplus
}
#endif

#endif
