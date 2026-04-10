// SPDX-License-Identifier: MIT
#include <Elite/RemoteUpgrade.hpp>
#include <Elite/Elite_Remoteupgrade_C.hpp>

elite_c_status_t elite_upgrade_control_software(const char* ip, const char* file, const char* password, int32_t* out_success) {
    if (!ip || !file || !password || !out_success) {
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        *out_success = ELITE::UPGRADE::upgradeControlSoftware(ip, file, password) ? 1 : 0;
        return ELITE_C_STATUS_OK;
    } catch (...) {
        return ELITE_C_STATUS_EXCEPTION;
    }
}
