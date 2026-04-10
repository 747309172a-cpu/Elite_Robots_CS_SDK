// SPDX-License-Identifier: MIT
#ifndef __ELITE_RTSI_INTERNAL_HPP__
#define __ELITE_RTSI_INTERNAL_HPP__

#include <Elite/RtsiRecipe.hpp>


//wrapper 内部私有头，不对外暴露，所以未放置到include/rtsi目录下
//供 Elite_Rtsi_Client_C.cpp 和 Elite_Rtsi_Recipe_C.cpp 两个文件使用,新增一个内部头文件来共享 elite_rtsi_recipe_handle_t 定义，避免跨 cpp 的句柄类型不一致问题

struct elite_rtsi_recipe_handle_t {
    ELITE::RtsiRecipeSharedPtr recipe;
};

#endif
