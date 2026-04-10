// SPDX-License-Identifier: MIT
// Copyright (c) 2026, Elite Robots.
// C ABI wrapper for source/Dashboard module.
#ifndef __ELITE_DASHBOARD_C_H__
#define __ELITE_DASHBOARD_C_H__

#include <Elite_C_Types.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

//句柄类型定义，用户使用c接口时不需要关心这个类型的具体实现细节，只需通过指针来操作这个类型即可
typedef struct elite_dashboard_handle_t elite_dashboard_handle_t;
//二级指针
ELITE_C_EXPORT elite_c_status_t elite_dashboard_create(elite_dashboard_handle_t** out_handle);  //构造函数，分配资源并初始化
ELITE_C_EXPORT void elite_dashboard_destroy(elite_dashboard_handle_t* handle);   //析构函数，释放资源

ELITE_C_EXPORT elite_c_status_t elite_dashboard_connect(elite_dashboard_handle_t* handle, const char* ip, int32_t port,
                                                        int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_disconnect(elite_dashboard_handle_t* handle);

ELITE_C_EXPORT elite_c_status_t elite_dashboard_echo(elite_dashboard_handle_t* handle, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_power_on(elite_dashboard_handle_t* handle, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_power_off(elite_dashboard_handle_t* handle, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_brake_release(elite_dashboard_handle_t* handle, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_close_safety_dialog(elite_dashboard_handle_t* handle, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_unlock_protective_stop(elite_dashboard_handle_t* handle, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_safety_system_restart(elite_dashboard_handle_t* handle, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_log(elite_dashboard_handle_t* handle, const char* message, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_popup(elite_dashboard_handle_t* handle, const char* arg, const char* message,
                                                      int32_t* out_success);

ELITE_C_EXPORT elite_c_status_t elite_dashboard_quit(elite_dashboard_handle_t* handle);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_reboot(elite_dashboard_handle_t* handle);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_shutdown(elite_dashboard_handle_t* handle);

ELITE_C_EXPORT elite_c_status_t elite_dashboard_robot_mode(elite_dashboard_handle_t* handle, int32_t* out_mode);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_safety_mode(elite_dashboard_handle_t* handle, int32_t* out_mode);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_running_status(elite_dashboard_handle_t* handle, int32_t* out_status);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_get_task_status(elite_dashboard_handle_t* handle, int32_t* out_status);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_speed_scaling(elite_dashboard_handle_t* handle, int32_t* out_scaling);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_set_speed_scaling(elite_dashboard_handle_t* handle, int32_t scaling,
                                                                  int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_task_is_running(elite_dashboard_handle_t* handle, int32_t* out_running);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_is_task_saved(elite_dashboard_handle_t* handle, int32_t* out_saved);

ELITE_C_EXPORT elite_c_status_t elite_dashboard_play_program(elite_dashboard_handle_t* handle, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_pause_program(elite_dashboard_handle_t* handle, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_stop_program(elite_dashboard_handle_t* handle, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_load_configuration(elite_dashboard_handle_t* handle, const char* path,
                                                                   int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_load_task(elite_dashboard_handle_t* handle, const char* path,
                                                          int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_is_configuration_modify(elite_dashboard_handle_t* handle,
                                                                        int32_t* out_modified);

ELITE_C_EXPORT elite_c_status_t elite_dashboard_version(elite_dashboard_handle_t* handle, char* out_buffer, int32_t buffer_len,
                                                        int32_t* out_required_len);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_help(elite_dashboard_handle_t* handle, const char* cmd, char* out_buffer,
                                                     int32_t buffer_len, int32_t* out_required_len);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_usage(elite_dashboard_handle_t* handle, const char* cmd, char* out_buffer,
                                                      int32_t buffer_len, int32_t* out_required_len);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_robot_type(elite_dashboard_handle_t* handle, char* out_buffer, int32_t buffer_len,
                                                           int32_t* out_required_len);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_robot_serial_number(elite_dashboard_handle_t* handle, char* out_buffer,
                                                                    int32_t buffer_len, int32_t* out_required_len);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_robot_id(elite_dashboard_handle_t* handle, char* out_buffer, int32_t buffer_len,
                                                         int32_t* out_required_len);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_configuration_path(elite_dashboard_handle_t* handle, char* out_buffer,
                                                                   int32_t buffer_len, int32_t* out_required_len);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_get_task_path(elite_dashboard_handle_t* handle, char* out_buffer,
                                                              int32_t buffer_len, int32_t* out_required_len);
ELITE_C_EXPORT elite_c_status_t elite_dashboard_send_and_receive(elite_dashboard_handle_t* handle, const char* cmd,
                                                                 char* out_buffer, int32_t buffer_len,
                                                                 int32_t* out_required_len);

// For error handling, the functions will return an error code and set the last error message in the handle. If the handle is null, the error message will be set in a global variable.
ELITE_C_EXPORT const char* elite_dashboard_last_error_message(elite_dashboard_handle_t* handle);
ELITE_C_EXPORT const char* elite_dashboard_global_last_error_message(void);

#ifdef __cplusplus
}
#endif

#endif
