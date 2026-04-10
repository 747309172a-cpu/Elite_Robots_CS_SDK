// SPDX-License-Identifier: MIT
// Copyright (c) 2026, Elite Robots.
//
// C ABI wrapper for source/Elite module (EliteDriver subset).
#ifndef __ELITE_DRIVER_C_H__
#define __ELITE_DRIVER_C_H__

#include <Elite/Elite_Serialcommunication_C.hpp>
#include <Elite_C_Types.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef struct elite_driver_handle_t elite_driver_handle_t;

typedef struct elite_driver_config_t {
    const char* robot_ip;
    const char* script_file_path;
    const char* local_ip;

    int32_t headless_mode;
    int32_t script_sender_port;
    int32_t reverse_port;
    int32_t trajectory_port;
    int32_t script_command_port;

    float servoj_time;
    float servoj_lookahead_time;
    int32_t servoj_gain;
    float stopj_acc;
} elite_driver_config_t;

typedef struct elite_driver_robot_exception_t {
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
} elite_driver_robot_exception_t;

typedef void (*elite_driver_trajectory_result_cb_t)(int32_t result, void* user_data);
typedef void (*elite_driver_robot_exception_cb_t)(const elite_driver_robot_exception_t* ex, void* user_data);

ELITE_C_EXPORT void elite_driver_config_set_default(elite_driver_config_t* config);

ELITE_C_EXPORT elite_c_status_t elite_driver_create(const elite_driver_config_t* config, elite_driver_handle_t** out_handle);
ELITE_C_EXPORT void elite_driver_destroy(elite_driver_handle_t* handle);

ELITE_C_EXPORT elite_c_status_t elite_driver_is_robot_connected(elite_driver_handle_t* handle, int32_t* out_connected);
ELITE_C_EXPORT elite_c_status_t elite_driver_send_external_control_script(elite_driver_handle_t* handle, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_stop_control(elite_driver_handle_t* handle, int32_t wait_ms, int32_t* out_success);

ELITE_C_EXPORT elite_c_status_t elite_driver_write_servoj(elite_driver_handle_t* handle, const double* pos6, int32_t timeout_ms,
                                                          int32_t cartesian, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_write_speedj(elite_driver_handle_t* handle, const double* vel6, int32_t timeout_ms,
                                                          int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_write_speedl(elite_driver_handle_t* handle, const double* vel6, int32_t timeout_ms,
                                                          int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_write_idle(elite_driver_handle_t* handle, int32_t timeout_ms, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_set_trajectory_result_callback(elite_driver_handle_t* handle,
                                                                            elite_driver_trajectory_result_cb_t cb,
                                                                            void* user_data);
ELITE_C_EXPORT elite_c_status_t elite_driver_write_trajectory_point(elite_driver_handle_t* handle, const double* positions6,
                                                                    float time, float blend_radius, int32_t cartesian,
                                                                    int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_write_trajectory_control_action(elite_driver_handle_t* handle, int32_t action,
                                                                             int32_t point_number, int32_t timeout_ms,
                                                                             int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_write_freedrive(elite_driver_handle_t* handle, int32_t action, int32_t timeout_ms,
                                                             int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_zero_ft_sensor(elite_driver_handle_t* handle, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_set_payload(elite_driver_handle_t* handle, double mass, const double* cog3,
                                                         int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_set_tool_voltage(elite_driver_handle_t* handle, int32_t voltage,
                                                              int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_start_force_mode(elite_driver_handle_t* handle, const double* reference_frame6,
                                                              const int32_t* selection_vector6, const double* wrench6,
                                                              int32_t mode, const double* limits6, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_end_force_mode(elite_driver_handle_t* handle, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_send_script(elite_driver_handle_t* handle, const char* script, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_get_primary_package(elite_driver_handle_t* handle, int32_t timeout_ms,
                                                                 double* out_dh_a6, double* out_dh_d6, double* out_dh_alpha6,
                                                                 int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_primary_reconnect(elite_driver_handle_t* handle, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_register_robot_exception_callback(elite_driver_handle_t* handle,
                                                                               elite_driver_robot_exception_cb_t cb,
                                                                               void* user_data);
ELITE_C_EXPORT elite_c_status_t elite_driver_start_tool_rs485(elite_driver_handle_t* handle,
                                                              const elite_serial_config_t* config,
                                                              const char* ssh_password, int32_t tcp_port,
                                                              elite_serial_comm_handle_t** out_comm);
ELITE_C_EXPORT elite_c_status_t elite_driver_end_tool_rs485(elite_driver_handle_t* handle, elite_serial_comm_handle_t* comm,
                                                            const char* ssh_password, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_driver_start_board_rs485(elite_driver_handle_t* handle,
                                                               const elite_serial_config_t* config,
                                                               const char* ssh_password, int32_t tcp_port,
                                                               elite_serial_comm_handle_t** out_comm);
ELITE_C_EXPORT elite_c_status_t elite_driver_end_board_rs485(elite_driver_handle_t* handle, elite_serial_comm_handle_t* comm,
                                                             const char* ssh_password, int32_t* out_success);

ELITE_C_EXPORT const char* elite_driver_last_error_message(elite_driver_handle_t* handle);
ELITE_C_EXPORT const char* elite_c_last_error_message(void);

#ifdef __cplusplus
}
#endif

#endif
