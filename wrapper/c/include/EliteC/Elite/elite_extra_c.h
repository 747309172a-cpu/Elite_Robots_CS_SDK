// SPDX-License-Identifier: MIT
// Copyright (c) 2026, Elite Robots.
//
// C ABI wrapper for Elite utility APIs (ControllerLog/Log/Upgrade/VersionInfo).
#ifndef __ELITE_EXTRA_C_H__
#define __ELITE_EXTRA_C_H__

#include <EliteC/elite_c_types.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef struct elite_version_info_t2 {
    uint32_t major;
    uint32_t minor;
    uint32_t bugfix;
    uint32_t build;
} elite_version_info_t2;

typedef void (*elite_controller_log_progress_cb_t)(int32_t file_size, int32_t recv_size, const char* err, void* user_data);

typedef enum elite_log_level_t {
    ELITE_LOG_LEVEL_DEBUG = 0,
    ELITE_LOG_LEVEL_INFO = 1,
    ELITE_LOG_LEVEL_WARN = 2,
    ELITE_LOG_LEVEL_ERROR = 3,
    ELITE_LOG_LEVEL_FATAL = 4,
    ELITE_LOG_LEVEL_NONE = 5
} elite_log_level_t;

typedef void (*elite_log_handler_cb_t)(const char* file, int32_t line, int32_t loglevel, const char* log, void* user_data);

ELITE_C_EXPORT elite_c_status_t elite_controller_log_download_system_log(const char* robot_ip, const char* password,
                                                                         const char* path, elite_controller_log_progress_cb_t cb,
                                                                         void* user_data, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_upgrade_control_software(const char* ip, const char* file, const char* password,
                                                               int32_t* out_success);

ELITE_C_EXPORT elite_c_status_t elite_register_log_handler(elite_log_handler_cb_t cb, void* user_data);
ELITE_C_EXPORT void elite_unregister_log_handler();
ELITE_C_EXPORT void elite_set_log_level(elite_log_level_t level);
ELITE_C_EXPORT void elite_log_message(const char* file, int32_t line, elite_log_level_t level, const char* message);
ELITE_C_EXPORT void elite_log_debug_message(const char* file, int32_t line, const char* message);
ELITE_C_EXPORT void elite_log_info_message(const char* file, int32_t line, const char* message);
ELITE_C_EXPORT void elite_log_warn_message(const char* file, int32_t line, const char* message);
ELITE_C_EXPORT void elite_log_error_message(const char* file, int32_t line, const char* message);
ELITE_C_EXPORT void elite_log_fatal_message(const char* file, int32_t line, const char* message);
ELITE_C_EXPORT void elite_log_none_message(const char* file, int32_t line, const char* message);

ELITE_C_EXPORT elite_c_status_t elite_version_info_from_string(const char* version, elite_version_info_t2* out_version);
ELITE_C_EXPORT elite_c_status_t elite_version_info_to_string(const elite_version_info_t2* version, char* out_buffer,
                                                             int32_t buffer_len, int32_t* out_required_len);
ELITE_C_EXPORT int32_t elite_version_info_eq(const elite_version_info_t2* a, const elite_version_info_t2* b);
ELITE_C_EXPORT int32_t elite_version_info_ne(const elite_version_info_t2* a, const elite_version_info_t2* b);
ELITE_C_EXPORT int32_t elite_version_info_lt(const elite_version_info_t2* a, const elite_version_info_t2* b);
ELITE_C_EXPORT int32_t elite_version_info_le(const elite_version_info_t2* a, const elite_version_info_t2* b);
ELITE_C_EXPORT int32_t elite_version_info_gt(const elite_version_info_t2* a, const elite_version_info_t2* b);
ELITE_C_EXPORT int32_t elite_version_info_ge(const elite_version_info_t2* a, const elite_version_info_t2* b);

#ifdef __cplusplus
}
#endif

#endif
