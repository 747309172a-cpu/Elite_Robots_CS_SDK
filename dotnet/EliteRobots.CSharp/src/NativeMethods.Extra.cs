using System.Runtime.InteropServices;

namespace EliteRobots.CSharp;

internal static partial class NativeMethods
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void EliteControllerLogProgressCallback(int fileSize, int recvSize, nint err, nint userData);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void EliteLogHandlerCallback(nint file, int line, int loglevel, nint log, nint userData);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_serial_comm_connect(nint comm, int timeoutMs, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_serial_comm_disconnect(nint comm);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_serial_comm_is_connected(nint comm, out int outConnected);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_serial_comm_get_socat_pid(nint comm, out int outPid);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_serial_comm_write(
        nint comm,
        [In] byte[] data,
        int size,
        out int outWritten);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_serial_comm_read(
        nint comm,
        [Out] byte[] outData,
        int size,
        int timeoutMs,
        out int outRead);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_controller_log_download_system_log(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string robotIp,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string password,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        EliteControllerLogProgressCallback? cb,
        nint userData,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_upgrade_control_software(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string ip,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string file,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string password,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_register_log_handler(EliteLogHandlerCallback cb, nint userData);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_unregister_log_handler();

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_set_log_level(int level);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_log_message(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string file,
        int line,
        int level,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_log_debug_message(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string file,
        int line,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_log_info_message(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string file,
        int line,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_log_warn_message(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string file,
        int line,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_log_error_message(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string file,
        int line,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_log_fatal_message(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string file,
        int line,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_log_none_message(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string file,
        int line,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_version_info_from_string(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string version,
        out EliteVersionInfoNative outVersion);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_version_info_to_string(
        ref EliteVersionInfoNative version,
        [Out] byte[]? outBuffer,
        int bufferLen,
        out int outRequiredLen);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int elite_version_info_eq(ref EliteVersionInfoNative a, ref EliteVersionInfoNative b);
    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int elite_version_info_ne(ref EliteVersionInfoNative a, ref EliteVersionInfoNative b);
    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int elite_version_info_lt(ref EliteVersionInfoNative a, ref EliteVersionInfoNative b);
    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int elite_version_info_le(ref EliteVersionInfoNative a, ref EliteVersionInfoNative b);
    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int elite_version_info_gt(ref EliteVersionInfoNative a, ref EliteVersionInfoNative b);
    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int elite_version_info_ge(ref EliteVersionInfoNative a, ref EliteVersionInfoNative b);
}
