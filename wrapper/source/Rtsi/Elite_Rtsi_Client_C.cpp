// SPDX-License-Identifier: MIT
#include <Elite/RtsiClientInterface.hpp>
#include <Rtsi/Elite_Rtsi_C.h>
#include "Elite_Rtsi_Internal.hpp"

#include <memory>
#include <mutex>
#include <new>
#include <sstream>
#include <string>
#include <vector>
#include <stdexcept>

struct elite_rtsi_client_handle_t {
    std::unique_ptr<ELITE::RtsiClientInterface> client;
    std::string last_error;
    std::mutex mutex;
};

namespace {
thread_local std::string g_last_error;

inline void set_global_error(const std::string& msg) { g_last_error = msg; }

inline void set_client_error(elite_rtsi_client_handle_t* handle, const std::string& msg) {
    if (!handle) {
        set_global_error(msg);
        return;
    }
    std::lock_guard<std::mutex> lock(handle->mutex);
    handle->last_error = msg;
    g_last_error = msg;
}

inline std::vector<std::string> split_csv(const char* recipe_csv) {
    std::vector<std::string> out;
    if (!recipe_csv) {
        return out;
    }

    std::stringstream ss(recipe_csv);
    std::string item;
    while (std::getline(ss, item, ',')) {
        size_t start = 0;
        while (start < item.size() && (item[start] == ' ' || item[start] == '\t' || item[start] == '\n' || item[start] == '\r')) {
            ++start;
        }
        size_t end = item.size();
        while (end > start &&
               (item[end - 1] == ' ' || item[end - 1] == '\t' || item[end - 1] == '\n' || item[end - 1] == '\r')) {
            --end;
        }
        if (end > start) {
            out.emplace_back(item.substr(start, end - start));
        }
    }
    return out;
}

template <typename Fn>
elite_c_status_t run_with_client(elite_rtsi_client_handle_t* handle, Fn&& fn) {
    if (!handle) {
        set_global_error("handle is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        fn();
        return ELITE_C_STATUS_OK;
    } catch (const std::exception& e) {
        set_client_error(handle, e.what());
        return ELITE_C_STATUS_EXCEPTION;
    } catch (...) {
        set_client_error(handle, "unknown exception");
        return ELITE_C_STATUS_EXCEPTION;
    }
}

}  // namespace

elite_c_status_t elite_rtsi_client_create(elite_rtsi_client_handle_t** out_handle) {
    if (!out_handle) {
        set_global_error("out_handle is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    try {
        auto handle = std::make_unique<elite_rtsi_client_handle_t>();
        handle->client = std::make_unique<ELITE::RtsiClientInterface>();
        *out_handle = handle.release();
        set_global_error("");
        return ELITE_C_STATUS_OK;
    } catch (const std::bad_alloc&) {
        set_global_error("allocation failed");
        return ELITE_C_STATUS_ALLOCATION_FAILED;
    } catch (const std::exception& e) {
        set_global_error(e.what());
        return ELITE_C_STATUS_EXCEPTION;
    } catch (...) {
        set_global_error("unknown exception");
        return ELITE_C_STATUS_EXCEPTION;
    }
}

void elite_rtsi_client_destroy(elite_rtsi_client_handle_t* handle) { delete handle; }

elite_c_status_t elite_rtsi_client_connect(elite_rtsi_client_handle_t* handle, const char* ip, int32_t port) {
    if (!ip) {
        set_global_error("ip is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_client(handle, [&]() { handle->client->connect(ip, port); });
}

elite_c_status_t elite_rtsi_client_disconnect(elite_rtsi_client_handle_t* handle) {
    return run_with_client(handle, [&]() { handle->client->disconnect(); });
}

elite_c_status_t elite_rtsi_client_negotiate_protocol_version(elite_rtsi_client_handle_t* handle, uint16_t version,
                                                              int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_client(handle, [&]() { *out_success = handle->client->negotiateProtocolVersion(version) ? 1 : 0; });
}

elite_c_status_t elite_rtsi_client_get_controller_version(elite_rtsi_client_handle_t* handle, elite_version_info_t* out_version) {
    if (!out_version) {
        set_global_error("out_version is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_client(handle, [&]() {
        const auto version = handle->client->getControllerVersion();
        out_version->major = version.major;
        out_version->minor = version.minor;
        out_version->bugfix = version.bugfix;
        out_version->build = version.build;
    });
}

elite_c_status_t elite_rtsi_client_setup_output_recipe(elite_rtsi_client_handle_t* handle, const char* recipe_csv, double frequency,
                                                       elite_rtsi_recipe_handle_t** out_recipe) {
    if (!recipe_csv || !out_recipe) {
        set_global_error("recipe_csv or out_recipe is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_client(handle, [&]() {
        const auto recipe_list = split_csv(recipe_csv);
        auto recipe = handle->client->setupOutputRecipe(recipe_list, frequency);
        auto recipe_handle = std::make_unique<elite_rtsi_recipe_handle_t>();
        recipe_handle->recipe = recipe;
        *out_recipe = recipe_handle.release();
    });
}

elite_c_status_t elite_rtsi_client_setup_input_recipe(elite_rtsi_client_handle_t* handle, const char* recipe_csv,
                                                      elite_rtsi_recipe_handle_t** out_recipe) {
    if (!recipe_csv || !out_recipe) {
        set_global_error("recipe_csv or out_recipe is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_client(handle, [&]() {
        const auto recipe_list = split_csv(recipe_csv);
        auto recipe = handle->client->setupInputRecipe(recipe_list);
        auto recipe_handle = std::make_unique<elite_rtsi_recipe_handle_t>();
        recipe_handle->recipe = recipe;
        *out_recipe = recipe_handle.release();
    });
}

elite_c_status_t elite_rtsi_client_start(elite_rtsi_client_handle_t* handle, int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_client(handle, [&]() { *out_success = handle->client->start() ? 1 : 0; });
}

elite_c_status_t elite_rtsi_client_pause(elite_rtsi_client_handle_t* handle, int32_t* out_success) {
    if (!out_success) {
        set_global_error("out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_client(handle, [&]() { *out_success = handle->client->pause() ? 1 : 0; });
}

elite_c_status_t elite_rtsi_client_send(elite_rtsi_client_handle_t* handle, elite_rtsi_recipe_handle_t* recipe) {
    if (!recipe || !recipe->recipe) {
        set_global_error("recipe is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_client(handle, [&]() { handle->client->send(recipe->recipe); });
}

elite_c_status_t elite_rtsi_client_receive_data(elite_rtsi_client_handle_t* handle, elite_rtsi_recipe_handle_t* recipe,
                                                int32_t read_newest, int32_t* out_success) {
    if (!recipe || !recipe->recipe || !out_success) {
        set_global_error("recipe or out_success is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_client(
        handle, [&]() { *out_success = handle->client->receiveData(recipe->recipe, read_newest != 0) ? 1 : 0; });
}

// This function is an optimized version of receive that can receive multiple recipes in one call. The recipes array should contain the same recipe handles that were used in setupOutputRecipe, and the received data will be written back to those recipe handles. The out_received_index will indicate which recipe index has new data (or -1 if no new data).
//输入参数为多个recipe句柄数组，函数会尝试接收数据并写回到这些句柄中。out_received_index参数会返回哪个recipe有新数据（如果没有新数据则返回-1）。这个函数是receive的优化版本，可以一次接收多个recipe的数据。
elite_c_status_t elite_rtsi_client_receive_data_multi(elite_rtsi_client_handle_t* handle, elite_rtsi_recipe_handle_t** recipes,
                                                      int32_t recipe_count, int32_t read_newest, int32_t* out_received_index) {
    if (!recipes || recipe_count <= 0 || !out_received_index) {
        set_global_error("recipes, recipe_count or out_received_index is invalid");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }

    return run_with_client(handle, [&]() {
        std::vector<ELITE::RtsiRecipeSharedPtr> native_recipes;
        native_recipes.reserve(static_cast<size_t>(recipe_count));
        for (int32_t i = 0; i < recipe_count; ++i) {
            if (!recipes[i] || !recipes[i]->recipe) {
                throw std::invalid_argument("recipe handle is null");
            }
            native_recipes.emplace_back(recipes[i]->recipe);
        }
        *out_received_index = handle->client->receiveData(native_recipes, read_newest != 0);
    });
}

elite_c_status_t elite_rtsi_client_is_connected(elite_rtsi_client_handle_t* handle, int32_t* out_connected) {
    if (!out_connected) {
        set_global_error("out_connected is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_client(handle, [&]() { *out_connected = handle->client->isConnected() ? 1 : 0; });
}

elite_c_status_t elite_rtsi_client_is_started(elite_rtsi_client_handle_t* handle, int32_t* out_started) {
    if (!out_started) {
        set_global_error("out_started is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_client(handle, [&]() { *out_started = handle->client->isStarted() ? 1 : 0; });
}

elite_c_status_t elite_rtsi_client_is_read_available(elite_rtsi_client_handle_t* handle, int32_t* out_available) {
    if (!out_available) {
        set_global_error("out_available is null");
        return ELITE_C_STATUS_INVALID_ARGUMENT;
    }
    return run_with_client(handle, [&]() { *out_available = handle->client->isReadAvailable() ? 1 : 0; });
}

const char* elite_rtsi_client_last_error_message(elite_rtsi_client_handle_t* handle) {
    if (!handle) {
        return "handle is null";
    }
    std::lock_guard<std::mutex> lock(handle->mutex);
    return handle->last_error.c_str();
}
