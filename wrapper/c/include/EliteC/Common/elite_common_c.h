// SPDX-License-Identifier: MIT
// Copyright (c) 2026, Elite Robots.
//
// C ABI wrapper for source/Common module.
#ifndef __ELITE_COMMON_C_H__
#define __ELITE_COMMON_C_H__

#include <EliteC/elite_c_types.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef struct elite_common_split_result_t elite_common_split_result_t;

ELITE_C_EXPORT elite_c_status_t elite_common_split_string(const char* input, const char* delimiter,
                                                          elite_common_split_result_t** out_result);
ELITE_C_EXPORT void elite_common_split_result_destroy(elite_common_split_result_t* result);
ELITE_C_EXPORT elite_c_status_t elite_common_split_result_size(elite_common_split_result_t* result, int32_t* out_size);
ELITE_C_EXPORT elite_c_status_t elite_common_split_result_get(elite_common_split_result_t* result, int32_t index,
                                                              char* out_buffer, int32_t buffer_len,
                                                              int32_t* out_required_len);
ELITE_C_EXPORT const char* elite_common_split_result_last_error_message(elite_common_split_result_t* result);

#ifdef __cplusplus
}
#endif

#endif
