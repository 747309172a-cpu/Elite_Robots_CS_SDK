// SPDX-License-Identifier: MIT
#include <Rtsi/Elite_Rtsi_C.h>
#include "Elite_Rtsi_Internal.hpp"

#include <cstring>
#include <new>
#include <sstream>
#include <string>
#include <vector>

namespace {
thread_local std::string g_last_error;

inline void set_global_error(const std::string& msg) { g_last_error = msg; }


//c中无string类型，需要将string转换为c风格字符串输出到buffer中
inline elite_c_status_t copy_string_to_buffer(const std::string& value, char* out_buffer, int32_t buffer_len,
                                              int32_t* out_required_len) {
    if (!out_required_len) {
        set_global_error("out_required_len is null");
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

inline std::string join_csv(const std::vector<std::string>& list) {
    std::ostringstream oss;
    for (size_t i = 0; i < list.size(); ++i) {
        if (i != 0) {
            oss << ",";
        }
        oss << list[i];
    }
    return oss.str();
}


//c中无vector6d_t类型，需要将长度为6的double数组转换为ELITE::vector6d_t类型，以下几种函数都是为了输入类型转换和输出类型转换
inline ELITE::vector6d_t to_vector6d(const double* data6) {
    ELITE::vector6d_t out{};
    for (size_t i = 0; i < out.size(); ++i) {
        out[i] = data6[i];
    }
    return out;
}

// 将ELITE::vector6d_t类型的数据复制到长度为6的double数组中
inline void copy_vector6_to_buffer(const ELITE::vector6d_t& src, double* dst6) {
    for (size_t i = 0; i < src.size(); ++i) {
        dst6[i] = src[i];
    }
}

template <typename Fn>
elite_c_status_t run_with_recipe(elite_rtsi_recipe_handle_t* recipe, Fn&& fn) {
    if (!recipe || !recipe->recipe) {
        set_global_error("recipe is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        fn();
        return ELITE_C_STATUS_OK;
    } catch (const std::exception& e) {
        set_global_error(e.what());
        return ELITE_C_STATUS_EXCEPTION;
    } catch (...) {
        set_global_error("unknown exception");
        return ELITE_C_STATUS_EXCEPTION;
    }
}
}  // namespace

void elite_rtsi_recipe_destroy(elite_rtsi_recipe_handle_t* recipe) { delete recipe; }

elite_c_status_t elite_rtsi_recipe_get_id(elite_rtsi_recipe_handle_t* recipe, int32_t* out_id) {
    if (!out_id) {
        set_global_error("out_id is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_recipe(recipe, [&]() { *out_id = recipe->recipe->getID(); });
}

elite_c_status_t elite_rtsi_recipe_get_recipe_csv(elite_rtsi_recipe_handle_t* recipe, char* out_buffer, int32_t buffer_len,
                                                  int32_t* out_required_len) {
    return run_with_recipe(recipe, [&]() {
        const std::string csv = join_csv(recipe->recipe->getRecipe());
        const elite_c_status_t status = copy_string_to_buffer(csv, out_buffer, buffer_len, out_required_len);
        if (status != ELITE_C_STATUS_OK) {
            throw std::invalid_argument("copy string failed");
        }
    });
}

elite_c_status_t elite_rtsi_recipe_get_value_double(elite_rtsi_recipe_handle_t* recipe, const char* name, double* out_value,
                                                    int32_t* out_success) {
    if (!name || !out_value || !out_success) {
        set_global_error("name, out_value or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_recipe(recipe, [&]() { *out_success = recipe->recipe->getValue(name, *out_value) ? 1 : 0; });
}

elite_c_status_t elite_rtsi_recipe_get_value_int32(elite_rtsi_recipe_handle_t* recipe, const char* name, int32_t* out_value,
                                                   int32_t* out_success) {
    if (!name || !out_value || !out_success) {
        set_global_error("name, out_value or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_recipe(recipe, [&]() { *out_success = recipe->recipe->getValue(name, *out_value) ? 1 : 0; });
}

elite_c_status_t elite_rtsi_recipe_get_value_uint32(elite_rtsi_recipe_handle_t* recipe, const char* name, uint32_t* out_value,
                                                    int32_t* out_success) {
    if (!name || !out_value || !out_success) {
        set_global_error("name, out_value or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_recipe(recipe, [&]() { *out_success = recipe->recipe->getValue(name, *out_value) ? 1 : 0; });
}

elite_c_status_t elite_rtsi_recipe_get_value_bool(elite_rtsi_recipe_handle_t* recipe, const char* name, int32_t* out_value,
                                                  int32_t* out_success) {
    if (!name || !out_value || !out_success) {
        set_global_error("name, out_value or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_recipe(recipe, [&]() {
        bool value = false;
        const bool ok = recipe->recipe->getValue(name, value);
        *out_success = ok ? 1 : 0;
        *out_value = value ? 1 : 0;
    });
}

elite_c_status_t elite_rtsi_recipe_get_value_vector6d(elite_rtsi_recipe_handle_t* recipe, const char* name, double* out_value6,
                                                      int32_t* out_success) {
    if (!name || !out_value6 || !out_success) {
        set_global_error("name, out_value6 or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_recipe(recipe, [&]() {
        ELITE::vector6d_t value{};
        const bool ok = recipe->recipe->getValue(name, value);
        *out_success = ok ? 1 : 0;
        if (ok) {
            copy_vector6_to_buffer(value, out_value6);
        }
    });
}

elite_c_status_t elite_rtsi_recipe_set_value_double(elite_rtsi_recipe_handle_t* recipe, const char* name, double value,
                                                    int32_t* out_success) {
    if (!name || !out_success) {
        set_global_error("name or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_recipe(recipe, [&]() { *out_success = recipe->recipe->setValue(name, value) ? 1 : 0; });
}

elite_c_status_t elite_rtsi_recipe_set_value_int32(elite_rtsi_recipe_handle_t* recipe, const char* name, int32_t value,
                                                   int32_t* out_success) {
    if (!name || !out_success) {
        set_global_error("name or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_recipe(recipe, [&]() { *out_success = recipe->recipe->setValue(name, value) ? 1 : 0; });
}

elite_c_status_t elite_rtsi_recipe_set_value_uint32(elite_rtsi_recipe_handle_t* recipe, const char* name, uint32_t value,
                                                    int32_t* out_success) {
    if (!name || !out_success) {
        set_global_error("name or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_recipe(recipe, [&]() { *out_success = recipe->recipe->setValue(name, value) ? 1 : 0; });
}

elite_c_status_t elite_rtsi_recipe_set_value_bool(elite_rtsi_recipe_handle_t* recipe, const char* name, int32_t value,
                                                  int32_t* out_success) {
    if (!name || !out_success) {
        set_global_error("name or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_recipe(recipe, [&]() { *out_success = recipe->recipe->setValue(name, value != 0) ? 1 : 0; });
}

elite_c_status_t elite_rtsi_recipe_set_value_vector6d(elite_rtsi_recipe_handle_t* recipe, const char* name, const double* value6,
                                                      int32_t* out_success) {
    if (!name || !value6 || !out_success) {
        set_global_error("name, value6 or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_recipe(recipe, [&]() {
        const auto value = to_vector6d(value6);
        *out_success = recipe->recipe->setValue(name, value) ? 1 : 0;
    });
}
