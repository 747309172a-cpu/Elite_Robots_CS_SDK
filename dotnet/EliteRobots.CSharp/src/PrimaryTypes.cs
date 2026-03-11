namespace EliteRobots.CSharp;

public sealed class PrimaryKinematicsInfo
{
    public double[] DhA { get; init; } = new double[6];
    public double[] DhD { get; init; } = new double[6];
    public double[] DhAlpha { get; init; } = new double[6];
}

public sealed class PrimaryRobotException
{
    public int Type { get; init; }
    public ulong Timestamp { get; init; }

    public int ErrorCode { get; init; }
    public int SubErrorCode { get; init; }
    public int ErrorSource { get; init; }
    public int ErrorLevel { get; init; }
    public int ErrorDataType { get; init; }
    public uint DataU32 { get; init; }
    public int DataI32 { get; init; }
    public float DataF32 { get; init; }

    public int Line { get; init; }
    public int Column { get; init; }
    public string Message { get; init; } = string.Empty;
}
