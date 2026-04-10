// SPDX-License-Identifier: MIT
// Copyright (c) 2026, Elite Robots.
//
// Shared C ABI types for Elite C wrapper modules.
//c语言接口中使用的共享类型定义，供多个c接口模块使用，避免重复定义和跨模块定义不一致问题
#ifndef __ELITE_C_TYPES_H__
#define __ELITE_C_TYPES_H__

#include <stdint.h>

#if defined(_WIN32) || defined(_WIN64)
#if defined(ELITE_C_EXPORT_LIBRARY)
#define ELITE_C_EXPORT __declspec(dllexport)
#else
#define ELITE_C_EXPORT __declspec(dllimport)
#endif
#else
#define ELITE_C_EXPORT __attribute__((visibility("default")))
#endif

#ifdef __cplusplus
extern "C" {
#endif

typedef enum elite_c_status_t {
    ELITE_C_STATUS_OK = 0,
    ELITE_C_STATUS_INVALID_ARGUMENT = 1,
    ELITE_C_STATUS_ALLOCATION_FAILED = 2,
    ELITE_C_STATUS_EXCEPTION = 3,
} elite_c_status_t;

typedef struct elite_version_info_t {
    uint32_t major;
    uint32_t minor;
    uint32_t bugfix;
    uint32_t build;
} elite_version_info_t;

#ifdef __cplusplus
}
#endif

#endif
