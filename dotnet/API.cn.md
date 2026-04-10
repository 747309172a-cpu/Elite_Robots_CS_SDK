# EliteRobots.CSharp API 文档

本文档面向 `dotnet/EliteRobots.CSharp/src` 的**用户调用层 API**。

命名空间：`EliteRobots.CSharp`

---

## 1. 通用类型与异常

### 1.1 `EliteSdkException`

- `EliteSdkException(string message, int statusCode)`
- 属性：`StatusCode`

### 1.2 机器人状态枚举

- `RobotMode`
- `SafetyMode`
- `TaskStatus`

### 1.3 RTSI 相关枚举

- `JointMode`
- `ToolMode`
- `ToolDigitalMode`
- `ToolDigitalOutputMode`
- `RtsiVersionInfo`

---

## 2. Primary 模块

### 2.1 数据类型

- `PrimaryKinematicsInfo`
  - `double[] DhA`
  - `double[] DhD`
  - `double[] DhAlpha`
- `PrimaryRobotException`

### 2.2 客户端 `PrimaryClientInterface`

- `PrimaryClientInterface()`
- `bool connect(string ip, int port = 30001)`
- `void disconnect()`
- `bool sendScript(string script)`
- `string getLocalIP()`
- `bool getPackage(out PrimaryKinematicsInfo info, int timeoutMs = 1000)`
- `void registerRobotExceptionCallback(Action<PrimaryRobotException> callback)`
- `void clearRobotExceptionCallback()`
- `void Dispose()`

---

## 3. Dashboard 模块

### 3.1 客户端 `DashboardClientInterface`

- `DashboardClientInterface()`
- `bool connect(string ip, int port = 29999)`
- `void disconnect()`
- `bool echo()`
- `bool powerOn()`
- `bool powerOff()`
- `bool brakeRelease()`
- `bool closeSafetyDialog()`
- `bool unlockProtectiveStop()`
- `bool safetySystemRestart()`
- `bool log(string message)`
- `bool popup(string arg, string message = "")`
- `void quit()`
- `void reboot()`
- `void shutdown()`
- `RobotMode robotMode()`
- `SafetyMode safetyMode()`
- `TaskStatus runningStatus()`
- `TaskStatus getTaskStatus()`
- `int speedScaling()`
- `bool setSpeedScaling(int scaling)`
- `bool taskIsRunning()`
- `bool isTaskSaved()`
- `bool playProgram()`
- `bool pauseProgram()`
- `bool stopProgram()`
- `bool loadConfiguration(string path)`
- `bool loadTask(string path)`
- `bool isConfigurationModify()`
- `string help(string cmd)`
- `string usage(string cmd)`
- `string version()`
- `string robotType()`
- `string robotSerialNumber()`
- `string robotID()`
- `string configurationPath()`
- `string getTaskPath()`
- `string sendAndReceive(string cmd)`
- `void Dispose()`

---

## 4. Driver 模块

### 4.1 配置类型 `EliteDriverConfig`

- `string RobotIp`
- `string ScriptFilePath`
- `string LocalIp`
- `bool HeadlessMode`
- `int ScriptSenderPort`
- `int ReversePort`
- `int TrajectoryPort`
- `int ScriptCommandPort`
- `float ServojTime`
- `float ServojLookaheadTime`
- `int ServojGain`
- `float StopjAcc`

### 4.2 枚举与类型（`DriverTypes.cs`）

- `TrajectoryMotionResult`
- `TrajectoryControlAction`
- `FreedriveAction`
- `ToolVoltage`
- `ForceMode`
- `SerialConfig`
- `SerialConfigBaudRate`
- `SerialConfigParity`
- `SerialConfigStopBits`
- `EliteDriverRobotException`
- `EliteSerialCommunication`

`EliteSerialCommunication` 方法：

- `bool connect(int timeout_ms)`
- `void disconnect()`
- `bool isConnected()`
- `int getSocatPid()`
- `int write(byte[] data)`
- `byte[] read(int size, int timeout_ms)`
- `void Dispose()`

### 4.3 客户端 `EliteDriver`

- `EliteDriver(EliteDriverConfig config)`
- `bool isRobotConnected()`
- `bool sendExternalControlScript()`
- `bool stopControl(int wait_ms = 10000)`
- `bool writeServoj(double[] pos, int timeout_ms, bool cartesian = false)`
- `bool writeSpeedj(double[] vel, int timeout_ms)`
- `bool writeSpeedl(double[] vel, int timeout_ms)`
- `void setTrajectoryResultCallback(Action<TrajectoryMotionResult> cb)`
- `bool writeTrajectoryPoint(double[] positions, float time, float blend_radius, bool cartesian)`
- `bool writeTrajectoryControlAction(TrajectoryControlAction action, int point_number, int timeout_ms)`
- `bool writeIdle(int timeout_ms)`
- `bool writeFreedrive(FreedriveAction action, int timeout_ms)`
- `bool zeroFTSensor()`
- `bool setPayload(double mass, double[] cog)`
- `bool setToolVoltage(ToolVoltage vol)`
- `bool startForceMode(double[] reference_frame, int[] selection_vector, double[] wrench, ForceMode mode, double[] limits)`
- `bool endForceMode()`
- `bool sendScript(string script)`
- `bool getPrimaryPackage(PrimaryKinematicsInfo pkg, int timeout_ms)`
- `bool primaryReconnect()`
- `void registerRobotExceptionCallback(Action<EliteDriverRobotException> cb)`
- `EliteSerialCommunication? startToolRs485(SerialConfig config, string ssh_password, int tcp_port = 54321)`
- `bool endToolRs485(EliteSerialCommunication comm, string ssh_password)`
- `EliteSerialCommunication? startBoardRs485(SerialConfig config, string ssh_password, int tcp_port = 54322)`
- `bool endBoardRs485(EliteSerialCommunication comm, string ssh_password)`
- `void Dispose()`

---

## 5. RTSI Client 模块（通用 RTSI）

### 5.1 Recipe 类型 `EliteRtsiRecipe`

- `int getID()`
- `string[] getRecipe()`
- `bool getValue(string name, out double out_value)`
- `bool getValue(string name, out int out_value)`
- `bool getValue(string name, out uint out_value)`
- `bool getValue(string name, out bool out_value)`
- `bool getValue(string name, double[] out_value6)`
- `bool setValue(string name, double value)`
- `bool setValue(string name, int value)`
- `bool setValue(string name, uint value)`
- `bool setValue(string name, bool value)`
- `bool setValue(string name, double[] value6)`
- `void Dispose()`

### 5.2 客户端 `RtsiClientInterface`

- `RtsiClientInterface()`
- `void connect(string ip, int port = 30004)`
- `void disconnect()`
- `bool negotiateProtocolVersion(ushort version = 1)`
- `RtsiVersionInfo getControllerVersion()`
- `EliteRtsiRecipe setupOutputRecipe(IEnumerable<string> recipe_list, double frequency = 250)`
- `EliteRtsiRecipe setupInputRecipe(IEnumerable<string> recipe)`
- `bool start()`
- `bool pause()`
- `void send(EliteRtsiRecipe recipe)`
- `bool receiveData(EliteRtsiRecipe recipe, bool read_newest = false)`
- `int receiveData(IReadOnlyList<EliteRtsiRecipe> recipes, bool read_newest = false)`
- `bool isConnected()`
- `bool isStarted()`
- `bool isReadAvailable()`
- `void Dispose()`

---

## 6. RTSI IO 模块（高层实时 IO）

### 6.1 客户端 `RtsiIOInterface`

构造与连接：

- `RtsiIOInterface(IEnumerable<string> output_recipe, IEnumerable<string> input_recipe, double frequency = 250)`
- `bool connect(string ip)`
- `void disconnect()`
- `bool isConnected()`
- `bool isStarted()`
- `RtsiVersionInfo getControllerVersion()`

控制写入：

- `bool setSpeedScaling(double scaling)`
- `bool setStandardDigital(int index, bool level)`
- `bool setConfigureDigital(int index, bool level)`
- `bool setAnalogOutputVoltage(int index, double value)`
- `bool setAnalogOutputCurrent(int index, double value)`
- `bool setExternalForceTorque(double[] value6)`
- `bool setToolDigitalOutput(int index, bool level)`

基础状态：

- `double getTimestamp()`
- `double getPayloadMass()`
- `double[] getPayloadCog()`
- `uint getScriptControlLine()`

关节/TCP：

- `double[] getTargetJointPositions()`
- `double[] getTargetJointVelocity()`
- `double[] getActualJointPositions()`
- `double[] getActualJointTorques()`
- `double[] getActualJointVelocity()`
- `double[] getActualJointCurrent()`
- `double[] getActualJointTemperatures()`
- `double[] getActualTCPPose()`
- `double[] getActualTCPVelocity()`
- `double[] getActualTCPForce()`
- `double[] getTargetTCPPose()`
- `double[] getTargetTCPVelocity()`

机器人状态：

- `uint getDigitalInputBits()`
- `uint getDigitalOutputBits()`
- `RobotMode getRobotMode()`
- `JointMode[] getJointMode()`
- `SafetyMode getSafetyStatus()`
- `uint getRobotStatus()`
- `TaskStatus getRuntimeState()`
- `double getActualSpeedScaling()`
- `double getTargetSpeedScaling()`
- `double getRobotVoltage()`
- `double getRobotCurrent()`
- `double[] getElbowPosition()`
- `double[] getElbowVelocity()`
- `uint getSafetyStatusBits()`

I/O 与 Tool：

- `uint getAnalogIOTypes()`
- `double getAnalogInput(int index)`
- `double getAnalogOutput(int index)`
- `double getIOCurrent()`
- `ToolMode getToolMode()`
- `uint getToolAnalogInputType()`
- `uint getToolAnalogOutputType()`
- `double getToolAnalogInput()`
- `double getToolAnalogOutput()`
- `double getToolOutputVoltage()`
- `double getToolOutputCurrent()`
- `double getToolOutputTemperature()`
- `ToolDigitalMode getToolDigitalMode()`
- `ToolDigitalOutputMode getToolDigitalOutputMode(int index)`

寄存器：

- `uint getOutBoolRegisters0To31()`
- `uint getOutBoolRegisters32To63()`
- `uint getInBoolRegisters0To31()`
- `uint getInBoolRegisters32To63()`
- `bool getInBoolRegister(int index)`
- `bool getOutBoolRegister(int index)`
- `int getInIntRegister(int index)`
- `int getOutIntRegister(int index)`
- `double getInDoubleRegister(int index)`
- `double getOutDoubleRegister(int index)`

Recipe 值访问：

- `bool getRecipeValue(string name, out double value)`
- `bool getRecipeValue(string name, out int value)`
- `bool getRecipeValue(string name, out uint value)`
- `bool getRecipeValue(string name, out bool value)`
- `bool getRecipeValue(string name, double[] value3or6)`
- `bool setInputRecipeValue(string name, double value)`
- `bool setInputRecipeValue(string name, int value)`
- `bool setInputRecipeValue(string name, uint value)`
- `bool setInputRecipeValue(string name, bool value)`
- `bool setInputRecipeValue(string name, double[] value6)`
- `void Dispose()`

---

## 7. Elite 工具模块（日志/升级/版本）

### 7.1 枚举 `LogLevel`

- `ELI_DEBUG`
- `ELI_INFO`
- `ELI_WARN`
- `ELI_ERROR`
- `ELI_FATAL`
- `ELI_NONE`

### 7.2 `EliteControllerLog`

- `bool downloadSystemLog(string robot_ip, string password, string path, Action<int, int, string>? progress_cb = null)`

### 7.3 `EliteUpgrade`

- `bool upgradeControlSoftware(string ip, string file, string password)`

### 7.4 `EliteLog`

- `void registerLogHandler(Action<string, int, LogLevel, string> handler)`
- `void unregisterLogHandler()`
- `void setLogLevel(LogLevel level)`
- `void logDebugMessage(string file, int line, string msg)`
- `void logInfoMessage(string file, int line, string msg)`
- `void logWarnMessage(string file, int line, string msg)`
- `void logErrorMessage(string file, int line, string msg)`
- `void logFatalMessage(string file, int line, string msg)`
- `void logNoneMessage(string file, int line, string msg)`

### 7.5 `VersionInfo`

字段：

- `uint major`
- `uint minor`
- `uint bugfix`
- `uint build`

构造与方法：

- `VersionInfo(uint major, uint minor, uint bugfix, uint build)`
- `VersionInfo(string version)`
- `override string ToString()`
- `static VersionInfo fromString(string version)`
- 比较运算符：`== != < <= > >=`

---

## 8. 使用建议

- 业务层优先使用 `Elite*Client` 与 `*Types`，避免直接依赖 `NativeMethods*`。
- 所有 `IDisposable` 客户端建议用 `using` 自动释放。
- 遇到 `EliteSdkException` 时，优先检查：
  - 机器人连接状态
  - 外控脚本是否启动
  - SSH/端口网络是否可达（尤其串口与升级功能）
