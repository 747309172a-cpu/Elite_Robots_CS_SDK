using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace EliteRobots.CSharp;

internal static partial class NativeMethods
{
    internal const string NativeLib = "elite_cs_series_sdk_c";

    internal enum EliteStatus
    {
        Ok = 0,
        InvalidArgument = 1,
        AllocationFailed = 2,
        Exception = 3,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct EliteDriverConfigNative
    {
        public nint robot_ip;
        public nint script_file_path;
        public nint local_ip;

        public int headless_mode;
        public int script_sender_port;
        public int reverse_port;
        public int trajectory_port;
        public int script_command_port;

        public float servoj_time;
        public float servoj_lookahead_time;
        public int servoj_gain;
        public float stopj_acc;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct EliteSerialConfigNative
    {
        public int baud_rate;
        public int parity;
        public int stop_bits;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct EliteDriverRobotExceptionNative
    {
        public int type;
        public ulong timestamp;
        public int error_code;
        public int sub_error_code;
        public int error_source;
        public int error_level;
        public int error_data_type;
        public uint data_u32;
        public int data_i32;
        public float data_f32;
        public int line;
        public int column;
        public nint message;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void EliteDriverTrajectoryResultCallback(int result, nint userData);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void EliteDriverRobotExceptionCallback(ref EliteDriverRobotExceptionNative ex, nint userData);

    [StructLayout(LayoutKind.Sequential)]
    internal struct EliteVersionInfoNative
    {
        public uint major;
        public uint minor;
        public uint bugfix;
        public uint build;
    }

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_driver_config_set_default(ref EliteDriverConfigNative config);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_create(
        ref EliteDriverConfigNative config,
        out nint outHandle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_driver_destroy(nint handle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_is_robot_connected(nint handle, out int outConnected);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_send_external_control_script(nint handle, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_stop_control(nint handle, int waitMs, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_write_servoj(
        nint handle,
        [In] double[] pos6,
        int timeoutMs,
        int cartesian,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_write_speedj(
        nint handle,
        [In] double[] vel6,
        int timeoutMs,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_write_speedl(
        nint handle,
        [In] double[] vel6,
        int timeoutMs,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_write_idle(nint handle, int timeoutMs, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_set_trajectory_result_callback(
        nint handle,
        EliteDriverTrajectoryResultCallback? cb,
        nint userData);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_write_trajectory_point(
        nint handle,
        [In] double[] positions6,
        float time,
        float blendRadius,
        int cartesian,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_write_trajectory_control_action(
        nint handle,
        int action,
        int pointNumber,
        int timeoutMs,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_write_freedrive(
        nint handle,
        int action,
        int timeoutMs,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_zero_ft_sensor(nint handle, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_set_payload(
        nint handle,
        double mass,
        [In] double[] cog3,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_set_tool_voltage(nint handle, int voltage, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_start_force_mode(
        nint handle,
        [In] double[] referenceFrame6,
        [In] int[] selectionVector6,
        [In] double[] wrench6,
        int mode,
        [In] double[] limits6,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_end_force_mode(nint handle, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_send_script(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string script,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_get_primary_package(
        nint handle,
        int timeoutMs,
        [Out] double[] outDhA6,
        [Out] double[] outDhD6,
        [Out] double[] outDhAlpha6,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_primary_reconnect(nint handle, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_register_robot_exception_callback(
        nint handle,
        EliteDriverRobotExceptionCallback? cb,
        nint userData);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_start_tool_rs485(
        nint handle,
        ref EliteSerialConfigNative config,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string sshPassword,
        int tcpPort,
        out nint outComm);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_end_tool_rs485(
        nint handle,
        nint comm,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string sshPassword,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_start_board_rs485(
        nint handle,
        ref EliteSerialConfigNative config,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string sshPassword,
        int tcpPort,
        out nint outComm);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_driver_end_board_rs485(
        nint handle,
        nint comm,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string sshPassword,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_serial_comm_destroy(nint comm);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern nint elite_driver_last_error_message(nint handle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern nint elite_c_last_error_message();

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_create(out nint outHandle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_dashboard_destroy(nint handle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_connect(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string ip,
        int port,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_disconnect(nint handle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_echo(nint handle, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_power_on(nint handle, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_power_off(nint handle, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_brake_release(nint handle, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_close_safety_dialog(nint handle, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_unlock_protective_stop(nint handle, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_safety_system_restart(nint handle, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_log(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_popup(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string arg,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_quit(nint handle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_reboot(nint handle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_shutdown(nint handle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_robot_mode(nint handle, out int outMode);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_safety_mode(nint handle, out int outMode);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_running_status(nint handle, out int outStatus);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_get_task_status(nint handle, out int outStatus);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_speed_scaling(nint handle, out int outScaling);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_set_speed_scaling(nint handle, int scaling, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_task_is_running(nint handle, out int outRunning);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_is_task_saved(nint handle, out int outSaved);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_play_program(nint handle, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_pause_program(nint handle, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_stop_program(nint handle, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_load_configuration(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_load_task(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_is_configuration_modify(nint handle, out int outModified);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_version(
        nint handle,
        [Out] byte[]? outBuffer,
        int bufferLen,
        out int outRequiredLen);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_help(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string cmd,
        [Out] byte[]? outBuffer,
        int bufferLen,
        out int outRequiredLen);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_usage(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string cmd,
        [Out] byte[]? outBuffer,
        int bufferLen,
        out int outRequiredLen);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_robot_type(
        nint handle,
        [Out] byte[]? outBuffer,
        int bufferLen,
        out int outRequiredLen);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_robot_serial_number(
        nint handle,
        [Out] byte[]? outBuffer,
        int bufferLen,
        out int outRequiredLen);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_robot_id(
        nint handle,
        [Out] byte[]? outBuffer,
        int bufferLen,
        out int outRequiredLen);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_configuration_path(
        nint handle,
        [Out] byte[]? outBuffer,
        int bufferLen,
        out int outRequiredLen);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_get_task_path(
        nint handle,
        [Out] byte[]? outBuffer,
        int bufferLen,
        out int outRequiredLen);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_dashboard_send_and_receive(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string cmd,
        [Out] byte[]? outBuffer,
        int bufferLen,
        out int outRequiredLen);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern nint elite_dashboard_last_error_message(nint handle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_client_create(out nint outHandle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_rtsi_client_destroy(nint handle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_client_connect(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string ip,
        int port);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_client_disconnect(nint handle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_client_negotiate_protocol_version(
        nint handle,
        ushort version,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_client_get_controller_version(
        nint handle,
        out EliteVersionInfoNative outVersion);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_client_setup_output_recipe(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string recipeCsv,
        double frequency,
        out nint outRecipe);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_client_setup_input_recipe(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string recipeCsv,
        out nint outRecipe);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_rtsi_recipe_destroy(nint recipe);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_client_start(nint handle, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_client_pause(nint handle, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_client_send(nint handle, nint recipe);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_client_receive_data(
        nint handle,
        nint recipe,
        int readNewest,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_client_is_connected(nint handle, out int outConnected);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_client_is_started(nint handle, out int outStarted);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_client_is_read_available(nint handle, out int outAvailable);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_recipe_get_id(nint recipe, out int outId);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_recipe_get_value_double(
        nint recipe,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        out double outValue,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_recipe_get_value_int32(
        nint recipe,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        out int outValue,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_recipe_get_value_uint32(
        nint recipe,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        out uint outValue,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_recipe_get_value_bool(
        nint recipe,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        out int outValue,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_recipe_get_value_vector6d(
        nint recipe,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        [Out] double[] outValue6,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_recipe_set_value_double(
        nint recipe,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        double value,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_recipe_set_value_int32(
        nint recipe,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        int value,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_recipe_set_value_uint32(
        nint recipe,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        uint value,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_recipe_set_value_bool(
        nint recipe,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        int value,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_recipe_set_value_vector6d(
        nint recipe,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        [In] double[] value6,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern nint elite_rtsi_client_last_error_message(nint handle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_primary_create(out nint outHandle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_primary_destroy(nint handle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_primary_connect(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string ip,
        int port,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_primary_disconnect(nint handle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_primary_send_script(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string script,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_primary_get_local_ip(
        nint handle,
        [Out] byte[]? outBuffer,
        int bufferLen,
        out int outRequiredLen);

    [StructLayout(LayoutKind.Sequential)]
    internal struct ElitePrimaryRobotExceptionNative
    {
        public int type;
        public ulong timestamp;
        public int error_code;
        public int sub_error_code;
        public int error_source;
        public int error_level;
        public int error_data_type;
        public uint data_u32;
        public int data_i32;
        public float data_f32;
        public int line;
        public int column;
        public nint message;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ElitePrimaryRobotExceptionCallback(ref ElitePrimaryRobotExceptionNative ex, nint userData);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_primary_get_kinematics_info(
        nint handle,
        int timeoutMs,
        [Out] double[] outDhA6,
        [Out] double[] outDhD6,
        [Out] double[] outDhAlpha6,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_primary_register_robot_exception_callback(
        nint handle,
        ElitePrimaryRobotExceptionCallback cb,
        nint userData);
}

internal sealed class EliteDriverSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal EliteDriverSafeHandle()
        : base(ownsHandle: true)
    {
    }

    internal EliteDriverSafeHandle(nint preexistingHandle)
        : base(ownsHandle: true)
    {
        SetHandle(preexistingHandle);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.elite_driver_destroy(handle);
        return true;
    }
}

internal sealed class EliteDashboardSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal EliteDashboardSafeHandle()
        : base(ownsHandle: true)
    {
    }

    internal EliteDashboardSafeHandle(nint preexistingHandle)
        : base(ownsHandle: true)
    {
        SetHandle(preexistingHandle);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.elite_dashboard_destroy(handle);
        return true;
    }
}

internal sealed class EliteSerialCommSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal EliteSerialCommSafeHandle()
        : base(ownsHandle: true)
    {
    }

    internal EliteSerialCommSafeHandle(nint preexistingHandle)
        : base(ownsHandle: true)
    {
        SetHandle(preexistingHandle);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.elite_serial_comm_destroy(handle);
        return true;
    }
}

internal sealed class ElitePrimarySafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal ElitePrimarySafeHandle()
        : base(ownsHandle: true)
    {
    }

    internal ElitePrimarySafeHandle(nint preexistingHandle)
        : base(ownsHandle: true)
    {
        SetHandle(preexistingHandle);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.elite_primary_destroy(handle);
        return true;
    }
}

internal sealed class EliteRtsiClientSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal EliteRtsiClientSafeHandle()
        : base(ownsHandle: true)
    {
    }

    internal EliteRtsiClientSafeHandle(nint preexistingHandle)
        : base(ownsHandle: true)
    {
        SetHandle(preexistingHandle);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.elite_rtsi_client_destroy(handle);
        return true;
    }
}

internal sealed class EliteRtsiRecipeSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal EliteRtsiRecipeSafeHandle()
        : base(ownsHandle: true)
    {
    }

    internal EliteRtsiRecipeSafeHandle(nint preexistingHandle)
        : base(ownsHandle: true)
    {
        SetHandle(preexistingHandle);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.elite_rtsi_recipe_destroy(handle);
        return true;
    }
}
