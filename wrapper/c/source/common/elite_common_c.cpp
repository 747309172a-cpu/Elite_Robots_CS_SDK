// SPDX-License-Identifier: MIT
#include <Elite/StringUtils.hpp>
#include <EliteC/Common/elite_common_c.h>

#include <cstring>
#include <memory>
#include <mutex>
#include <new>
#include <string>
#include <vector>

struct elite_common_split_result_t {
    std::vector<std::string> tokens;
    std::string last_error;
    std::mutex mutex;
};

namespace {
inline void set_error(elite_common_split_result_t* result, const std::string& msg) {
    if (!result) {
        return;
    }
    std::lock_guard<std::mutex> lock(result->mutex);
    result->last_error = msg;
}

inline elite_c_status_t copy_string_to_buffer(const std::string& value, char* out_buffer, int32_t buffer_len,
                                              int32_t* out_required_len) {
    if (!out_required_len) {
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    const int32_t required_len = static_cast<int32_t>(value.size()) + 1;
    *out_required_len = required_len;

    if (!out_buffer || buffer_len <= 0) {
        return ELITE_C_STATUS_OK;
    }

    const int32_t copy_len = (buffer_len < required_len) ? (buffer_len - 1) : static_cast<int32_t>(value.size());
    if (copy_len > 0) {
        std::memcpy(out_buffer, value.data(), static_cast<size_t>(copy_len));
    }
    out_buffer[(copy_len >= 0) ? copy_len : 0] = '\0';
    return ELITE_C_STATUS_OK;
}
}  // namespace

elite_c_status_t elite_common_split_string(const char* input, const char* delimiter, elite_common_split_result_t** out_result) {
    if (!input || !delimiter || !out_result) {
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        auto result = std::make_unique<elite_common_split_result_t>();
        result->tokens = ELITE::StringUtils::splitString(input, delimiter);
        *out_result = result.release();
        return ELITE_C_STATUS_OK;
    } catch (const std::bad_alloc&) {
        return ELITE_C_STATUS_ALLOCATION_FAILED;
    } catch (...) {
        return ELITE_C_STATUS_EXCEPTION;
    }
}

void elite_common_split_result_destroy(elite_common_split_result_t* result) { delete result; }

elite_c_status_t elite_common_split_result_size(elite_common_split_result_t* result, int32_t* out_size) {
    if (!result || !out_size) {
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    *out_size = static_cast<int32_t>(result->tokens.size());
    return ELITE_C_STATUS_OK;
}

elite_c_status_t elite_common_split_result_get(elite_common_split_result_t* result, int32_t index, char* out_buffer, int32_t buffer_len,
                                               int32_t* out_required_len) {
    if (!result || index < 0 || !out_required_len) {
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    const int32_t size = static_cast<int32_t>(result->tokens.size());
    if (index >= size) {
        set_error(result, "index out of range");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    const elite_c_status_t status = copy_string_to_buffer(result->tokens[static_cast<size_t>(index)], out_buffer, buffer_len,
                                                           out_required_len);
    if (status != ELITE_C_STATUS_OK) {
        set_error(result, "invalid output buffer arguments");
    }
    return status;
}

const char* elite_common_split_result_last_error_message(elite_common_split_result_t* result) {
    if (!result) {
        return "result is null";
    }

    std::lock_guard<std::mutex> lock(result->mutex);
    return result->last_error.c_str();
}
