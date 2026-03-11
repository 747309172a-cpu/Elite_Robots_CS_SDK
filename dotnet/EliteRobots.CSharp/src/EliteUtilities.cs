using System.Runtime.InteropServices;
using System.Text;

namespace EliteRobots.CSharp;

public enum LogLevel
{
    ELI_DEBUG = 0,
    ELI_INFO = 1,
    ELI_WARN = 2,
    ELI_ERROR = 3,
    ELI_FATAL = 4,
    ELI_NONE = 5,
}

public static class EliteControllerLog
{
    public static bool downloadSystemLog(string robot_ip, string password, string path,
        Action<int, int, string>? progress_cb = null)
    {
        ArgumentNullException.ThrowIfNull(robot_ip);
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(path);

        GCHandle? gch = null;
        NativeMethods.EliteControllerLogProgressCallback? cb = null;
        if (progress_cb is not null)
        {
            gch = GCHandle.Alloc(progress_cb);
            cb = OnProgress;
        }

        try
        {
            var status = NativeMethods.elite_controller_log_download_system_log(
                robot_ip,
                password,
                path,
                cb,
                gch.HasValue ? GCHandle.ToIntPtr(gch.Value) : nint.Zero,
                out var ok);
            ThrowIfError(status);
            return ok != 0;
        }
        finally
        {
            if (gch.HasValue)
            {
                gch.Value.Free();
            }
        }
    }

    private static void OnProgress(int fileSize, int recvSize, nint err, nint userData)
    {
        if (userData == nint.Zero)
        {
            return;
        }
        var gch = GCHandle.FromIntPtr(userData);
        if (gch.Target is not Action<int, int, string> cb)
        {
            return;
        }
        cb(fileSize, recvSize, Marshal.PtrToStringUTF8(err) ?? string.Empty);
    }

    private static void ThrowIfError(NativeMethods.EliteStatus status)
    {
        if (status == NativeMethods.EliteStatus.Ok)
        {
            return;
        }
        var msgPtr = NativeMethods.elite_c_last_error_message();
        var message = Marshal.PtrToStringUTF8(msgPtr);
        throw new EliteSdkException(string.IsNullOrWhiteSpace(message) ? "native call failed" : message, (int)status);
    }
}

public static class EliteUpgrade
{
    public static bool upgradeControlSoftware(string ip, string file, string password)
    {
        ArgumentNullException.ThrowIfNull(ip);
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(password);
        var status = NativeMethods.elite_upgrade_control_software(ip, file, password, out var ok);
        if (status != NativeMethods.EliteStatus.Ok)
        {
            var msgPtr = NativeMethods.elite_c_last_error_message();
            var message = Marshal.PtrToStringUTF8(msgPtr);
            throw new EliteSdkException(string.IsNullOrWhiteSpace(message) ? "native call failed" : message, (int)status);
        }
        return ok != 0;
    }
}

public static class EliteLog
{
    private static Action<string, int, LogLevel, string>? _managedHandler;
    private static NativeMethods.EliteLogHandlerCallback? _nativeHandler;

    public static void registerLogHandler(Action<string, int, LogLevel, string> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _managedHandler = handler;
        _nativeHandler ??= OnLog;
        var status = NativeMethods.elite_register_log_handler(_nativeHandler, nint.Zero);
        ThrowIfError(status);
    }

    public static void unregisterLogHandler()
    {
        _managedHandler = null;
        NativeMethods.elite_unregister_log_handler();
    }

    public static void setLogLevel(LogLevel level) => NativeMethods.elite_set_log_level((int)level);

    public static void logDebugMessage(string file, int line, string msg) =>
        NativeMethods.elite_log_debug_message(file, line, msg);

    public static void logInfoMessage(string file, int line, string msg) =>
        NativeMethods.elite_log_info_message(file, line, msg);

    public static void logWarnMessage(string file, int line, string msg) =>
        NativeMethods.elite_log_warn_message(file, line, msg);

    public static void logErrorMessage(string file, int line, string msg) =>
        NativeMethods.elite_log_error_message(file, line, msg);

    public static void logFatalMessage(string file, int line, string msg) =>
        NativeMethods.elite_log_fatal_message(file, line, msg);

    public static void logNoneMessage(string file, int line, string msg) =>
        NativeMethods.elite_log_none_message(file, line, msg);

    private static void OnLog(nint file, int line, int loglevel, nint log, nint userData)
    {
        var cb = _managedHandler;
        if (cb is null)
        {
            return;
        }

        cb(
            Marshal.PtrToStringUTF8(file) ?? string.Empty,
            line,
            (LogLevel)loglevel,
            Marshal.PtrToStringUTF8(log) ?? string.Empty);
    }

    private static void ThrowIfError(NativeMethods.EliteStatus status)
    {
        if (status == NativeMethods.EliteStatus.Ok)
        {
            return;
        }
        var msgPtr = NativeMethods.elite_c_last_error_message();
        var message = Marshal.PtrToStringUTF8(msgPtr);
        throw new EliteSdkException(string.IsNullOrWhiteSpace(message) ? "native call failed" : message, (int)status);
    }
}

public struct VersionInfo
{
    public uint major;
    public uint minor;
    public uint bugfix;
    public uint build;

    public VersionInfo(uint major, uint minor, uint bugfix, uint build)
    {
        this.major = major;
        this.minor = minor;
        this.bugfix = bugfix;
        this.build = build;
    }

    public VersionInfo(string version)
    {
        ArgumentNullException.ThrowIfNull(version);
        var status = NativeMethods.elite_version_info_from_string(version, out var v);
        if (status != NativeMethods.EliteStatus.Ok)
        {
            var msgPtr = NativeMethods.elite_c_last_error_message();
            var message = Marshal.PtrToStringUTF8(msgPtr);
            throw new EliteSdkException(string.IsNullOrWhiteSpace(message) ? "native call failed" : message, (int)status);
        }
        major = v.major;
        minor = v.minor;
        bugfix = v.bugfix;
        build = v.build;
    }

    public override string ToString()
    {
        var native = ToNative(this);
        var status = NativeMethods.elite_version_info_to_string(ref native, null, 0, out var required);
        if (status != NativeMethods.EliteStatus.Ok)
        {
            return $"{major}.{minor}.{bugfix}.{build}";
        }
        var buffer = new byte[required];
        status = NativeMethods.elite_version_info_to_string(ref native, buffer, buffer.Length, out _);
        if (status != NativeMethods.EliteStatus.Ok)
        {
            return $"{major}.{minor}.{bugfix}.{build}";
        }
        var len = Array.IndexOf(buffer, (byte)0);
        if (len < 0)
        {
            len = buffer.Length;
        }
        return Encoding.UTF8.GetString(buffer, 0, len);
    }

    public static VersionInfo fromString(string version) => new(version);

    public static bool operator ==(VersionInfo a, VersionInfo b)
    {
        var na = ToNative(a);
        var nb = ToNative(b);
        return NativeMethods.elite_version_info_eq(ref na, ref nb) != 0;
    }
    public static bool operator !=(VersionInfo a, VersionInfo b)
    {
        var na = ToNative(a);
        var nb = ToNative(b);
        return NativeMethods.elite_version_info_ne(ref na, ref nb) != 0;
    }
    public static bool operator <(VersionInfo a, VersionInfo b)
    {
        var na = ToNative(a);
        var nb = ToNative(b);
        return NativeMethods.elite_version_info_lt(ref na, ref nb) != 0;
    }
    public static bool operator <=(VersionInfo a, VersionInfo b)
    {
        var na = ToNative(a);
        var nb = ToNative(b);
        return NativeMethods.elite_version_info_le(ref na, ref nb) != 0;
    }
    public static bool operator >(VersionInfo a, VersionInfo b)
    {
        var na = ToNative(a);
        var nb = ToNative(b);
        return NativeMethods.elite_version_info_gt(ref na, ref nb) != 0;
    }
    public static bool operator >=(VersionInfo a, VersionInfo b)
    {
        var na = ToNative(a);
        var nb = ToNative(b);
        return NativeMethods.elite_version_info_ge(ref na, ref nb) != 0;
    }

    public override bool Equals(object? obj) => obj is VersionInfo other && this == other;
    public override int GetHashCode() => HashCode.Combine(major, minor, bugfix, build);

    private static NativeMethods.EliteVersionInfoNative ToNative(VersionInfo v) => new()
    {
        major = v.major,
        minor = v.minor,
        bugfix = v.bugfix,
        build = v.build,
    };
}
