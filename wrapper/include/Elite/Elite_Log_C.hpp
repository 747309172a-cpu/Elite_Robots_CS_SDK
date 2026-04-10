// SPDX-License-Identifier: MIT
// Copyright (c) 2026, Elite Robots.
//
// C ABI wrapper for Elite log APIs.
#ifndef __ELITE_LOG_C_HPP__
#define __ELITE_LOG_C_HPP__

#include <Elite_C_Types.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef enum elite_log_level_t {
    ELITE_LOG_LEVEL_DEBUG = 0,
    ELITE_LOG_LEVEL_INFO = 1,
    ELITE_LOG_LEVEL_WARN = 2,
    ELITE_LOG_LEVEL_ERROR = 3,
    ELITE_LOG_LEVEL_FATAL = 4,
    ELITE_LOG_LEVEL_NONE = 5
} elite_log_level_t;

typedef void (*elite_log_handler_cb_t)(const char* file, int32_t line, int32_t loglevel, const char* log, void* user_data);

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

#ifdef __cplusplus
}
#endif

#endif
