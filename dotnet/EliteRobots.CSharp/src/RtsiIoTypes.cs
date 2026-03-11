namespace EliteRobots.CSharp;

public enum JointMode
{
    ModeReset = 235,
    ModeShuttingDown = 236,
    ModeBackdrive = 238,
    ModePowerOff = 239,
    ModeReadyForPoweroff = 240,
    ModeNotResponding = 245,
    ModeMotorInitialisation = 246,
    ModeBooting = 247,
    ModeBootloader = 249,
    ModeViolation = 251,
    ModeFault = 252,
    ModeRunning = 253,
    ModeIdle = 255,
}

public enum ToolMode : uint
{
    ModeReset = 235,
    ModeShuttingDown = 236,
    ModePowerOff = 239,
    ModeNotResponding = 245,
    ModeBooting = 247,
    ModeBootloader = 249,
    ModeFault = 252,
    ModeRunning = 253,
    ModeIdle = 255,
}

public enum ToolDigitalMode : byte
{
    SingleNeedle = 0,
    DoubleNeedle1 = 1,
    DoubleNeedle2 = 2,
    TripleNeedle = 3,
}

public enum ToolDigitalOutputMode : byte
{
    PushPullMode = 0,
    SourcingPnpMode = 1,
    SinkingNpnMode = 2,
}
