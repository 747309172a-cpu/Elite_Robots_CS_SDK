[English](./README.md)
# Elite Robots CS SDK

此SDK是艾利特机器人CS系列机械臂的C++库，借助该库，开发者可以基于C++实现驱动程序，从而充分利用艾利特机器人CS系列机械臂的多功能性来开发外部应用程序。


## Requirements
- ***CS Controller*** (机器人的控制软件) 需要 >= **≥ 2.15.0**。 如果机器人的控制软件版本低于此，建议升级。
- boost >= (建议)1.74
- cmake >= 3.22.1

## Build & Install
如果你的操作系统是 Ubuntu20.04、Ubuntu22.04、Ubuntu24.04，那么可以用下面的指令直接安装elite-cs-series-sdk:
```bash
sudo add-apt-repository ppa:elite-robots/cs-robot-series-sdk
sudo apt update
sudo apt install elite-cs-series-sdk
```

如果需要编译安装，参考[编译安装指南](./doc/BuildGuide/BuildGuide.cn.md)

## 使用指南
[使用指南](./doc/UserGuide/cn/UserGuide.cn.md)  

## API 说明
[API](./doc/API/cn/API.cn.md)

## C# 封装骨架
仓库已提供最小可用的 C ABI + C# 封装骨架，路径见 [`dotnet/`](./dotnet/README.md)。
通过以下 CMake 选项启用原生 C 包装层编译：
```bash
cmake -S . -B build -DELITE_COMPILE_C_WRAPPER=ON
```

## Architecture
[代码架构](./doc/Architecture/Arch.cn.md)

## Compatible Operating Systems
在以下系统平台上测试过：

 * Ubuntu 22.04 (Jammy Jellyfish)
 * Ubuntu 16.04 (Xenial Xerus)
 * Windowns 11

## Compiler
目前在以下编译器中通过编译

 * gcc 11.4.0
 * gcc 5.5.0
 * msvc 19.40.33808
