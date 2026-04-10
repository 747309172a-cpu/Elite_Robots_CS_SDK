// SPDX-License-Identifier: MIT
#include <Elite/ControllerLog.hpp>
#include <Elite/Elite_Controllerlog_C.hpp>

elite_c_status_t elite_controller_log_download_system_log(const char* robot_ip, const char* password, const char* path,
                                                          elite_controller_log_progress_cb_t cb, void* user_data,
                                                          int32_t* out_success) {
    if (!robot_ip || !password || !path || !out_success) {
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        auto progress_cb = [cb, user_data](int f_z, int r_z, const char* err) {
            if (cb) {
                cb(f_z, r_z, err, user_data);
            }
        };
        *out_success = ELITE::ControllerLog::downloadSystemLog(robot_ip, password, path, std::move(progress_cb)) ? 1 : 0;
        return ELITE_C_STATUS_OK;
    } catch (...) {
        return ELITE_C_STATUS_EXCEPTION;
    }
}
