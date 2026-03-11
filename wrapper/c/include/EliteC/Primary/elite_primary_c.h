// SPDX-License-Identifier: MIT
// Copyright (c) 2026, Elite Robots.
//
// C ABI wrapper for source/Primary module.
#ifndef __ELITE_PRIMARY_C_H__
#define __ELITE_PRIMARY_C_H__

#include <EliteC/elite_c_types.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef struct elite_primary_handle_t elite_primary_handle_t;

typedef struct elite_primary_robot_exception_t {
    int32_t type;
    uint64_t timestamp;

    int32_t error_code;
    int32_t sub_error_code;
    int32_t error_source;
    int32_t error_level;
    int32_t error_data_type;
    uint32_t data_u32;
    int32_t data_i32;
    float data_f32;

    int32_t line;
    int32_t column;
    const char* message;
} elite_primary_robot_exception_t;

typedef void (*elite_primary_robot_exception_cb_t)(const elite_primary_robot_exception_t* ex, void* user_data);

ELITE_C_EXPORT elite_c_status_t elite_primary_create(elite_primary_handle_t** out_handle);
ELITE_C_EXPORT void elite_primary_destroy(elite_primary_handle_t* handle);

ELITE_C_EXPORT elite_c_status_t elite_primary_connect(elite_primary_handle_t* handle, const char* ip, int32_t port,
                                                      int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_primary_disconnect(elite_primary_handle_t* handle);
ELITE_C_EXPORT elite_c_status_t elite_primary_send_script(elite_primary_handle_t* handle, const char* script,
                                                          int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_primary_get_local_ip(elite_primary_handle_t* handle, char* out_buffer, int32_t buffer_len,
                                                           int32_t* out_required_len);
ELITE_C_EXPORT elite_c_status_t elite_primary_get_kinematics_info(elite_primary_handle_t* handle, int32_t timeout_ms,
                                                                  double* out_dh_a6, double* out_dh_d6, double* out_dh_alpha6,
                                                                  int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_primary_register_robot_exception_callback(elite_primary_handle_t* handle,
                                                                                elite_primary_robot_exception_cb_t cb,
                                                                                void* user_data);

#ifdef __cplusplus
}
#endif

#endif
