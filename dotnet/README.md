# Elite Robots C# SDK Guide

## 1. Overview

This `dotnet` directory contains:

- `EliteRobots.CSharp`: C# wrapper library (`P/Invoke` + `SafeHandle`)
- `EliteRobots.CSharp.Sample`: runnable C# examples

C# call chain:

`C# -> C wrapper (libelite_cs_series_sdk_c) -> C++ SDK`

---

## 2. Build Prerequisites

- .NET SDK 8.0+
- CMake + C++ compiler
- Built native SDK and C wrapper

---

## 3. Build Steps

### 3.1 Build native libraries

Run at repository root:

```bash
cmake -S . -B build -DELITE_COMPILE_C_WRAPPER=ON
cmake --build build -j4
```

Expected native outputs:

- `build/libelite-cs-series-sdk.so`
- `build/wrapper/libelite_cs_series_sdk_c.so`

### 3.2 Build C# projects

```bash
dotnet build dotnet/EliteRobots.CSharp/EliteRobots.CSharp.csproj
dotnet build dotnet/EliteRobots.CSharp.Sample/EliteRobots.CSharp.Sample.csproj
```

---

## 4. How to Run Samples

General format:

```bash
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- <mode> <args...>
```

Current modes (`Program.cs`):

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

## 5. Example Commands

```bash
# Primary
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- primary_client 172.16.102.156

# Dashboard
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- dashboard_client 172.16.102.156

# Driver (general)
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- driver 172.16.102.156 source/resources/external_control.script --headless

# SpeedL flow
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- speedl 172.16.102.156 --headless true --script-file source/resources/external_control.script

# Trajectory flow
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- trajectory 172.16.102.156 --headless true --script-file source/resources/external_control.script

# Servoj plan flow
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- servoj_plan 172.16.102.156 --headless true --script-file source/resources/external_control.script

# RTSI client
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- rtsi_client 172.16.102.156 --port 30004

# Serial RS485 flow
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- serial 172.16.102.156 --ssh-password 123456 --headless true --script-file source/resources/external_control.script

# Robot -> PC socket connectivity test
dotnet run --project dotnet/EliteRobots.CSharp.Sample -- connect_robot_test 172.16.102.156 --server-port 50002
```

---

## 6. Sample Purpose and Flow

### 6.1 `primary_client`

- Purpose: connect to primary port, read kinematics package, send scripts, receive robot exception callback.
- Flow: `connect -> getPackage(KinematicsInfo) -> register callback -> send scripts -> disconnect`

### 6.2 `dashboard_client`

- Purpose: dashboard-level robot operations.
- Flow: `connect -> version/speed/mode read-write -> popup -> power on/off -> disconnect`

### 6.3 `driver`

- Purpose: test most `EliteDriver` control APIs.
- Flow: create config and driver, then test control methods (`writeServoj/speedl/speedj`, trajectory, force mode, primary package, callbacks, optional RS485).

### 6.4 `speedl`

- Purpose: simple speed command example with external control startup.
- Flow: `dashboard power on + brake release -> driver external control -> writeSpeedl down 5s -> writeSpeedl up 5s -> stopControl`

### 6.5 `trajectory`

- Purpose: trajectory motion example including callback-based completion.
- Flow:
  - read current joints/TCP from RTSI IO
  - start control
  - `moveTo` one joint target
  - send cartesian trajectory points using `writeTrajectoryPoint`
  - maintain NOOP and wait callback result

### 6.6 `servoj_plan`

- Purpose: trapezoidal speed planned servoj example.
- Flow: compute trapezoidal profile for joint-6 and stream points with `writeServoj`.

### 6.7 `rtsi_client`

- Purpose: low-level RTSI client flow.
- Flow: `connect -> negotiate protocol -> setup recipes -> start/receive -> send input recipe -> pause/disconnect`

### 6.8 `serial`

- Purpose: tool RS485 communication through driver.
- Flow: `dashboard ready -> external control ready -> startToolRs485 -> serial connect/write/read -> endToolRs485`

### 6.9 `connect_robot_test`

- Purpose: verify robot can connect back to PC via script socket.
- Flow: start local TCP server, send primary script to robot, verify returned string.

---

## 7. Recipe Files

Sample recipe files are placed at:

- `dotnet/EliteRobots.CSharp.Sample/resource/input_recipe.txt`
- `dotnet/EliteRobots.CSharp.Sample/resource/output_recipe.txt`

---

## 8. Troubleshooting

### 8.1 `DllNotFoundException: elite_cs_series_sdk_c`

- Ensure native wrapper is built:
  - `build/wrapper/libelite_cs_series_sdk_c.so`
- Rebuild sample project after native build.

### 8.2 `SSH connection failed: Connection refused` in serial example

- `startToolRs485` requires SSH connectivity to controller.
- Check robot SSH service, port 22 accessibility, firewall/network route.

### 8.3 FIFO scheduling warning (`RtUtils`)

- This is a performance warning, not always a hard failure.

---

## 9. Safety Notice

Trajectory, speed, and servo examples can move the robot physically.

- Validate workspace safety before running.
- Keep emergency stop available.
- Start with low speed/acceleration.
