# EliteRobots.CSharp API Reference (By Module)

This document targets the **user-facing managed API** in `dotnet/EliteRobots.CSharp/src`.

Namespace: `EliteRobots.CSharp`

---

## 1. Common Types and Exception

### 1.1 `EliteSdkException`

- `EliteSdkException(string message, int statusCode)`
- Property: `StatusCode`

### 1.2 Robot State Enums

- `RobotMode`
- `SafetyMode`
- `TaskStatus`

### 1.3 RTSI Enums

- `JointMode`
- `ToolMode`
- `ToolDigitalMode`
- `ToolDigitalOutputMode`
- `RtsiVersionInfo`

---

## 2. Primary Module

### 2.1 Data Types

- `PrimaryKinematicsInfo`
  - `double[] DhA`
  - `double[] DhD`
  - `double[] DhAlpha`
- `PrimaryRobotException`

### 2.2 Client `PrimaryClientInterface`

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

## 3. Dashboard Module

### 3.1 Client `DashboardClientInterface`

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

## 4. Driver Module

### 4.1 Config Type `EliteDriverConfig`

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

### 4.2 Enums and Types (`DriverTypes.cs`)

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

`EliteSerialCommunication` methods:

- `bool connect(int timeout_ms)`
- `void disconnect()`
- `bool isConnected()`
- `int getSocatPid()`
- `int write(byte[] data)`
- `byte[] read(int size, int timeout_ms)`
- `void Dispose()`

### 4.3 Client `EliteDriver`

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

## 5. RTSI Client Module (Generic RTSI)

### 5.1 Recipe Type `EliteRtsiRecipe`

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

### 5.2 Client `RtsiClientInterface`

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

## 6. RTSI IO Module (High-Level Realtime IO)

### 6.1 Client `RtsiIOInterface`

Construction and connection:

- `RtsiIOInterface(IEnumerable<string> output_recipe, IEnumerable<string> input_recipe, double frequency = 250)`
- `bool connect(string ip)`
- `void disconnect()`
- `bool isConnected()`
- `bool isStarted()`
- `RtsiVersionInfo getControllerVersion()`

Control writes:

- `bool setSpeedScaling(double scaling)`
- `bool setStandardDigital(int index, bool level)`
- `bool setConfigureDigital(int index, bool level)`
- `bool setAnalogOutputVoltage(int index, double value)`
- `bool setAnalogOutputCurrent(int index, double value)`
- `bool setExternalForceTorque(double[] value6)`
- `bool setToolDigitalOutput(int index, bool level)`

Basic states:

- `double getTimestamp()`
- `double getPayloadMass()`
- `double[] getPayloadCog()`
- `uint getScriptControlLine()`

Joint/TCP data:

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

Robot states:

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

I/O and tool:

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

Registers:

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

Recipe value access:

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

## 7. Elite Utility Module (Log/Upgrade/Version)

### 7.1 Enum `LogLevel`

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

Fields:

- `uint major`
- `uint minor`
- `uint bugfix`
- `uint build`

Constructors and methods:

- `VersionInfo(uint major, uint minor, uint bugfix, uint build)`
- `VersionInfo(string version)`
- `override string ToString()`
- `static VersionInfo fromString(string version)`
- Comparison operators: `== != < <= > >=`

---

## 8. Usage Notes

- Prefer `Elite*Client` and `*Types` in business logic instead of direct `NativeMethods*`.
- Use `using` for all `IDisposable` clients/resources.
- For `EliteSdkException`, check:
  - robot connectivity
  - external control script state
  - network/SSH reachability (especially serial and upgrade features)
