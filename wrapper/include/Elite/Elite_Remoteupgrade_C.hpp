// SPDX-License-Identifier: MIT
// Copyright (c) 2026, Elite Robots.
//
// C ABI wrapper for Elite remote upgrade APIs.
#ifndef __ELITE_REMOTEUPGRADE_C_HPP__
#define __ELITE_REMOTEUPGRADE_C_HPP__

#include <Elite_C_Types.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

ELITE_C_EXPORT elite_c_status_t elite_upgrade_control_software(const char* ip, const char* file, const char* password,
                                                               int32_t* out_success);

#ifdef __cplusplus
}
#endif

#endif
