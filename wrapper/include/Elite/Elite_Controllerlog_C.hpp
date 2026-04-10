// SPDX-License-Identifier: MIT
// Copyright (c) 2026, Elite Robots.
//
// C ABI wrapper for Elite controller log APIs.
#ifndef __ELITE_CONTROLLERLOG_C_HPP__
#define __ELITE_CONTROLLERLOG_C_HPP__

#include <Elite_C_Types.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef void (*elite_controller_log_progress_cb_t)(int32_t file_size, int32_t recv_size, const char* err, void* user_data);

ELITE_C_EXPORT elite_c_status_t elite_controller_log_download_system_log(const char* robot_ip, const char* password,
                                                                         const char* path, elite_controller_log_progress_cb_t cb,
                                                                         void* user_data, int32_t* out_success);

#ifdef __cplusplus
}
#endif

#endif
