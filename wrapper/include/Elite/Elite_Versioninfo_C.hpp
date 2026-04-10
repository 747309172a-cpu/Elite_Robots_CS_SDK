// SPDX-License-Identifier: MIT
// Copyright (c) 2026, Elite Robots.
//
// C ABI wrapper for Elite version info APIs.
#ifndef __ELITE_VERSIONINFO_C_HPP__
#define __ELITE_VERSIONINFO_C_HPP__

#include <Elite_C_Types.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

ELITE_C_EXPORT elite_c_status_t elite_version_info_from_string(const char* version, elite_version_info_t* out_version);
ELITE_C_EXPORT elite_c_status_t elite_version_info_to_string(const elite_version_info_t* version, char* out_buffer,
                                                             int32_t buffer_len, int32_t* out_required_len);
ELITE_C_EXPORT int32_t elite_version_info_eq(const elite_version_info_t* a, const elite_version_info_t* b);
ELITE_C_EXPORT int32_t elite_version_info_ne(const elite_version_info_t* a, const elite_version_info_t* b);
ELITE_C_EXPORT int32_t elite_version_info_lt(const elite_version_info_t* a, const elite_version_info_t* b);
ELITE_C_EXPORT int32_t elite_version_info_le(const elite_version_info_t* a, const elite_version_info_t* b);
ELITE_C_EXPORT int32_t elite_version_info_gt(const elite_version_info_t* a, const elite_version_info_t* b);
ELITE_C_EXPORT int32_t elite_version_info_ge(const elite_version_info_t* a, const elite_version_info_t* b);

#ifdef __cplusplus
}
#endif

#endif
