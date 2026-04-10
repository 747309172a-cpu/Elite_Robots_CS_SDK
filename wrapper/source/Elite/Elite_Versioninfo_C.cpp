// SPDX-License-Identifier: MIT
#include <Elite/VersionInfo.hpp>
#include <Elite/Elite_Versioninfo_C.hpp>

#include <cstring>
#include <string>

namespace {
inline void fill_version_info(const ELITE::VersionInfo& in, elite_version_info_t* out) {
    out->major = in.major;
    out->minor = in.minor;
    out->bugfix = in.bugfix;
    out->build = in.build;
}

inline ELITE::VersionInfo to_version_info(const elite_version_info_t* in) {
    return ELITE::VersionInfo(static_cast<int>(in->major), static_cast<int>(in->minor), static_cast<int>(in->bugfix),
                              static_cast<int>(in->build));
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

elite_c_status_t elite_version_info_from_string(const char* version, elite_version_info_t* out_version) {
    if (!version || !out_version) {
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        const auto v = ELITE::VersionInfo::fromString(version);
        fill_version_info(v, out_version);
        return ELITE_C_STATUS_OK;
    } catch (...) {
        return ELITE_C_STATUS_EXCEPTION;
    }
}

elite_c_status_t elite_version_info_to_string(const elite_version_info_t* version, char* out_buffer, int32_t buffer_len,
                                              int32_t* out_required_len) {
    if (!version || !out_required_len) {
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        const auto v = to_version_info(version);
        return copy_string_to_buffer(v.toString(), out_buffer, buffer_len, out_required_len);
    } catch (...) {
        return ELITE_C_STATUS_EXCEPTION;
    }
}

int32_t elite_version_info_eq(const elite_version_info_t* a, const elite_version_info_t* b) {
    if (!a || !b) {
        return 0;
    }
    return to_version_info(a) == to_version_info(b) ? 1 : 0;
}
int32_t elite_version_info_ne(const elite_version_info_t* a, const elite_version_info_t* b) {
    if (!a || !b) {
        return 0;
    }
    return to_version_info(a) != to_version_info(b) ? 1 : 0;
}
int32_t elite_version_info_lt(const elite_version_info_t* a, const elite_version_info_t* b) {
    if (!a || !b) {
        return 0;
    }
    return to_version_info(a) < to_version_info(b) ? 1 : 0;
}
int32_t elite_version_info_le(const elite_version_info_t* a, const elite_version_info_t* b) {
    if (!a || !b) {
        return 0;
    }
    return to_version_info(a) <= to_version_info(b) ? 1 : 0;
}
int32_t elite_version_info_gt(const elite_version_info_t* a, const elite_version_info_t* b) {
    if (!a || !b) {
        return 0;
    }
    return to_version_info(a) > to_version_info(b) ? 1 : 0;
}
int32_t elite_version_info_ge(const elite_version_info_t* a, const elite_version_info_t* b) {
    if (!a || !b) {
        return 0;
    }
    return to_version_info(a) >= to_version_info(b) ? 1 : 0;
}
