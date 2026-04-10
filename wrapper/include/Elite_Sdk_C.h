// SPDX-License-Identifier: MIT
// Copyright (c) 2026, Elite Robots.
//
// C ABI wrapper for a subset of Elite Robots CS SDK.
//c语言总入口头文件，包含了c语言接口需要的所有头文件，用户在使用c接口时只需要包含这个头文件即可
#ifndef __ELITE_SDK_C_H__
#define __ELITE_SDK_C_H__

#include <Elite_C_Types.h>
#include <stdint.h>

#include <Dashboard/Elite_Dashboard_C.h>
#include <Elite/Elite_Controllerlog_C.hpp>
#include <Elite/Elite_Driver_C.h>
#include <Elite/Elite_Log_C.hpp>
#include <Elite/Elite_Remoteupgrade_C.hpp>
#include <Elite/Elite_Serialcommunication_C.hpp>
#include <Elite/Elite_Versioninfo_C.hpp>
#include <Primary/Elite_Primary_C.h>
#include <Rtsi/Elite_Rtsi_C.h>

#endif
