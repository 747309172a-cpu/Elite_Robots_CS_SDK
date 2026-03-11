// SPDX-License-Identifier: MIT
// Copyright (c) 2026, Elite Robots.
//
// Shared C ABI types for Elite C wrapper modules.
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

#ifdef __cplusplus
}
#endif

#endif
