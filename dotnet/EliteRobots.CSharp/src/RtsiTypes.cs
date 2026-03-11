namespace EliteRobots.CSharp;

public sealed class RtsiVersionInfo
{
    public uint Major { get; init; }
    public uint Minor { get; init; }
    public uint Bugfix { get; init; }
    public uint Build { get; init; }

    public override string ToString()
    {
        return $"{Major}.{Minor}.{Bugfix}.{Build}";
    }
}
