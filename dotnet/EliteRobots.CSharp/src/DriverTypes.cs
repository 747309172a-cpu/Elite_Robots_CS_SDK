namespace EliteRobots.CSharp;

public enum TrajectoryMotionResult
{
    SUCCESS = 0,
    CANCELED = 1,
    FAILURE = 2,
}

public enum TrajectoryControlAction
{
    CANCEL = -1,
    NOOP = 0,
    START = 1,
}

public enum FreedriveAction
{
    FREEDRIVE_END = -1,
    FREEDRIVE_NOOP = 0,
    FREEDRIVE_START = 1,
}

public enum ToolVoltage
{
    OFF = 0,
    V_12 = 12,
    V_24 = 24,
}

public enum ForceMode
{
    FIX = 0,
    POINT = 1,
    MOTION = 2,
    TCP = 3,
}

public sealed class EliteSerialConfig
{
    public SerialConfigBaudRate baud_rate { get; set; } = SerialConfigBaudRate.BR_115200;
    public SerialConfigParity parity { get; set; } = SerialConfigParity.NONE;
    public SerialConfigStopBits stop_bits { get; set; } = SerialConfigStopBits.ONE;
}

public enum SerialConfigBaudRate
{
    BR_2400 = 2400,
    BR_4800 = 4800,
    BR_9600 = 9600,
    BR_19200 = 19200,
    BR_38400 = 38400,
    BR_57600 = 57600,
    BR_115200 = 115200,
    BR_460800 = 460800,
    BR_1000000 = 1000000,
    BR_2000000 = 2000000,
}

public enum SerialConfigParity
{
    NONE = 0,
    ODD = 1,
    EVEN = 2,
}

public enum SerialConfigStopBits
{
    ONE = 1,
    TWO = 2,
}

public sealed class EliteDriverRobotException
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

public sealed class EliteSerialCommunication : IDisposable
{
    internal EliteSerialCommunication(EliteSerialCommSafeHandle handle)
    {
        Handle = handle;
    }

    internal EliteSerialCommSafeHandle Handle { get; }

    public bool connect(int timeout_ms)
    {
        var status = NativeMethods.elite_serial_comm_connect(Handle.DangerousGetHandle(), timeout_ms, out var ok);
        ThrowIfError(status);
        return ok != 0;
    }

    public void disconnect()
    {
        var status = NativeMethods.elite_serial_comm_disconnect(Handle.DangerousGetHandle());
        ThrowIfError(status);
    }

    public bool isConnected()
    {
        var status = NativeMethods.elite_serial_comm_is_connected(Handle.DangerousGetHandle(), out var ok);
        ThrowIfError(status);
        return ok != 0;
    }

    public int getSocatPid()
    {
        var status = NativeMethods.elite_serial_comm_get_socat_pid(Handle.DangerousGetHandle(), out var pid);
        ThrowIfError(status);
        return pid;
    }

    public int write(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var status = NativeMethods.elite_serial_comm_write(Handle.DangerousGetHandle(), data, data.Length, out var written);
        ThrowIfError(status);
        return written;
    }

    public byte[] read(int size, int timeout_ms)
    {
        if (size < 0)
        {
            throw new ArgumentException("size must be >= 0", nameof(size));
        }

        var buffer = new byte[size];
        var status = NativeMethods.elite_serial_comm_read(Handle.DangerousGetHandle(), buffer, size, timeout_ms, out var read);
        ThrowIfError(status);
        if (read <= 0)
        {
            return Array.Empty<byte>();
        }
        if (read == size)
        {
            return buffer;
        }
        var result = new byte[read];
        Array.Copy(buffer, result, read);
        return result;
    }

    public void Dispose()
    {
        Handle.Dispose();
    }

    private static void ThrowIfError(NativeMethods.EliteStatus status)
    {
        if (status == NativeMethods.EliteStatus.Ok)
        {
            return;
        }

        var msgPtr = NativeMethods.elite_c_last_error_message();
        var message = System.Runtime.InteropServices.Marshal.PtrToStringUTF8(msgPtr);
        if (string.IsNullOrWhiteSpace(message))
        {
            message = "native call failed";
        }
        throw new EliteSdkException(message, (int)status);
    }
}
