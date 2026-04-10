# Elite Robots C# SDK 使用指南

## 1. 概览

`dotnet` 目录包含：

- `EliteRobots.CSharp`：C# 封装库（基于 `P/Invoke` 与 `SafeHandle`）
- `EliteRobots.CSharp.Sample`：可直接运行的 C# 示例程序

C# 调用链：

`C# -> C 封装层 (libelite_cs_series_sdk_c) -> C++ SDK`

---

## 2. 构建前提

- .NET SDK 8.0+
- CMake + C++ 编译器
- 已编译 C++ SDK 与 C 封装库

---

## 3. 构建步骤

### 3.1 编译本地库

在仓库根目录执行：

```bash
cmake -S . -B build -DELITE_COMPILE_C_WRAPPER=ON
cmake --build build -j4
```

期望生成：

- `build/libelite-cs-series-sdk.so`
- `build/wrapper/libelite_cs_series_sdk_c.so`
后者依赖前者编译而成

### 3.2 编译 C# 项目

```bash
dotnet build dotnet/EliteRobots.CSharp/EliteRobots.CSharp.csproj
dotnet build dotnet/EliteRobots.CSharp.Sample/EliteRobots.CSharp.Sample.csproj
```

---

## 4. 如何运行示例

通用格式：

```bash
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- <mode> <args...>
```

当前支持模式（见 `Program.cs`）：

- `primary_client`
- `dashboard_client`
- `driver`
- `speedl`
- `trajectory`
- `servoj_plan`
- `rtsi_client`
- `serial`
- `connect_robot_test`

---

## 5. 示例命令

```bash
# Primary
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- primary_client 172.16.102.156(机器人ip)

# Dashboard
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- dashboard_client 172.16.102.156

# Driver（通用接口测试）
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- driver 172.16.102.156 source/resources/external_control.script --headless

# SpeedL 示例
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- speedl 172.16.102.156 --headless true --script-file source/resources/external_control.script

# Trajectory 示例
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- trajectory 172.16.102.156 --headless true --script-file source/resources/external_control.script

# Servoj 轨迹规划示例
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- servoj_plan 172.16.102.156 --headless true --script-file source/resources/external_control.script

# RTSI Client 示例
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- rtsi_client 172.16.102.156 --port 30004

# 串口 RS485 示例
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- serial 172.16.102.156 --ssh-password 123456 --headless true --script-file source/resources/external_control.script

# 机器人反连 PC 套接字测试
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- connect_robot_test 172.16.102.156 --server-port 50002
```

---

## 6. 各示例功能与流程

### 6.1 `primary_client`

- 功能：连接 Primary 端口，读取运动学包，发送脚本，接收机器人异常回调。
- 流程：`connect -> getPackage(KinematicsInfo) -> 注册回调 -> 发送脚本 -> disconnect`

### 6.2 `dashboard_client`

- 功能：执行 Dashboard 层面的机器人控制。
- 流程：`connect -> 版本/速度/模式读写 -> popup -> 上下电 -> disconnect`

### 6.3 `driver`

- 功能：覆盖测试 `EliteDriver` 的主要控制接口。
- 流程：创建配置并初始化 Driver，依次测试控制方法（`writeServoj/speedl/speedj`、轨迹、力控、Primary 包、回调、可选 RS485）。

### 6.4 `speedl`

- 功能：`writeSpeedl` 简化示例，包含外控启动流程。
- 流程：`dashboard 上电+刹车释放 -> driver 外控就绪 -> 下行速度 5 秒 -> 上行速度 5 秒 -> stopControl`

### 6.5 `trajectory`

- 功能：轨迹运动示例，包含基于回调的完成判定。
- 流程：
  - 通过 RTSI IO 读取当前关节/TCP
  - 启动控制
  - 先执行关节 `moveTo`
  - 再用 `writeTrajectoryPoint` 下发笛卡尔轨迹点
  - 循环发送 NOOP 保活并等待回调结果

### 6.6 `servoj_plan`

- 功能：梯形速度规划 `servoj` 示例。
- 流程：计算第 6 关节梯形速度轨迹并循环 `writeServoj` 下发点位。

### 6.7 `rtsi_client`

- 功能：RTSI 通用客户端示例。
- 流程：`connect -> 协议协商 -> 配置 recipe -> start/receive -> 发送输入 recipe -> pause/disconnect`

### 6.8 `serial`

- 功能：通过 Driver 打通工具端 RS485 串口通信。
- 流程：`dashboard 就绪 -> 外控就绪 -> startToolRs485 -> 串口 connect/write/read -> endToolRs485`

### 6.9 `connect_robot_test`

- 功能：验证机器人能否通过脚本反连 PC 套接字。
- 流程：本地启动 TCP 服务，Primary 下发脚本，校验机器人返回字符串。

---

## 7. 配方文件

示例配方文件位置：

- `dotnet/EliteRobots.CSharp.Sample/resource/input_recipe.txt`
- `dotnet/EliteRobots.CSharp.Sample/resource/output_recipe.txt`

---

## 8. 常见问题

### 8.1 `DllNotFoundException: elite_cs_series_sdk_c`

- 确保已生成本地库：
  - `build/wrapper/libelite_cs_series_sdk_c.so`
- 本地库更新后重新 `dotnet build`。

### 8.2 串口示例报 `SSH connection failed: Connection refused`

- `startToolRs485` 依赖控制器 SSH。
- 检查控制器 SSH 服务是否开启、22 端口是否可达、防火墙/路由是否阻断。

### 8.3 `RtUtils` FIFO 调度警告

- 这是实时调度优化提示，通常不是致命错误。

---

## 9. 安全提示

轨迹、速度与伺服示例会驱动机器人真实运动。

- 运行前确认工作空间安全。
- 保持急停可用。
- 先使用较低速度与加速度。
