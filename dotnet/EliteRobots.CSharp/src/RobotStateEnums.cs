namespace EliteRobots.CSharp;

public enum RobotMode
{
    Unknown = -2,
    NoController = -1,
    Disconnected = 0,
    ConfirmSafety = 1,
    Booting = 2,
    PowerOff = 3,
    PowerOn = 4,
    Idle = 5,
    Backdrive = 6,
    Running = 7,
    UpdatingFirmware = 8,
    WaitingCalibration = 9,
}

public enum SafetyMode
{
    Unknown = -2,
    Normal = 1,
    Reduced = 2,
    ProtectiveStop = 3,
    Recovery = 4,
    SafeguardStop = 5,
    SystemEmergencyStop = 6,
    RobotEmergencyStop = 7,
    Violation = 8,
    Fault = 9,
    ValidateJointId = 10,
    UndefinedSafetyMode = 11,
    AutomaticModeSafeguardStop = 12,
    SystemThreePositionEnablingStop = 13,
    TpThreePositionEnablingStop = 14,
}

public enum TaskStatus
{
    Unknown = 0,
    Playing = 1,
    Paused = 2,
    Stopped = 3,
}
