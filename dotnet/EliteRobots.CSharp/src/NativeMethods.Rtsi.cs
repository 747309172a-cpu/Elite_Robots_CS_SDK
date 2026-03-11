using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace EliteRobots.CSharp;

internal static partial class NativeMethods
{
    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_client_receive_data_multi(
        nint handle,
        [In] nint[] recipes,
        int recipeCount,
        int readNewest,
        out int outReceivedIndex);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_recipe_get_recipe_csv(
        nint recipe,
        [Out] byte[]? outBuffer,
        int bufferLen,
        out int outRequiredLen);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_create(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string outputRecipeCsv,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string inputRecipeCsv,
        double frequency,
        out nint outHandle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void elite_rtsi_io_destroy(nint handle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_connect(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string ip,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_disconnect(nint handle);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_is_connected(nint handle, out int outConnected);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_is_started(nint handle, out int outStarted);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_controller_version(nint handle, out EliteVersionInfoNative outVersion);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_set_speed_scaling(nint handle, double scaling, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_set_standard_digital(nint handle, int index, int level, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_set_configure_digital(nint handle, int index, int level, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_set_analog_output_voltage(nint handle, int index, double value, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_set_analog_output_current(nint handle, int index, double value, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_set_external_force_torque(nint handle, [In] double[] value6, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_set_tool_digital_output(nint handle, int index, int level, out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_timestamp(nint handle, out double outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_payload_mass(nint handle, out double outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_payload_cog(nint handle, [Out] double[] outValue3);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_script_control_line(nint handle, out uint outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_target_joint_positions(nint handle, [Out] double[] outValue6);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_target_joint_velocity(nint handle, [Out] double[] outValue6);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_actual_joint_positions(nint handle, [Out] double[] outValue6);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_actual_joint_torques(nint handle, [Out] double[] outValue6);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_actual_joint_velocity(nint handle, [Out] double[] outValue6);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_actual_joint_current(nint handle, [Out] double[] outValue6);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_actual_joint_temperatures(nint handle, [Out] double[] outValue6);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_actual_tcp_pose(nint handle, [Out] double[] outValue6);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_actual_tcp_velocity(nint handle, [Out] double[] outValue6);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_actual_tcp_force(nint handle, [Out] double[] outValue6);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_target_tcp_pose(nint handle, [Out] double[] outValue6);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_target_tcp_velocity(nint handle, [Out] double[] outValue6);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_digital_input_bits(nint handle, out uint outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_digital_output_bits(nint handle, out uint outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_robot_mode(nint handle, out int outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_joint_mode(nint handle, [Out] int[] outValue6);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_safety_status(nint handle, out int outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_robot_status(nint handle, out uint outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_runtime_state(nint handle, out int outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_actual_speed_scaling(nint handle, out double outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_target_speed_scaling(nint handle, out double outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_robot_voltage(nint handle, out double outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_robot_current(nint handle, out double outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_elbow_position(nint handle, [Out] double[] outValue3);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_elbow_velocity(nint handle, [Out] double[] outValue3);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_safety_status_bits(nint handle, out uint outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_analog_io_types(nint handle, out uint outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_analog_input(nint handle, int index, out double outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_analog_output(nint handle, int index, out double outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_io_current(nint handle, out double outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_tool_mode(nint handle, out uint outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_tool_analog_input_type(nint handle, out uint outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_tool_analog_output_type(nint handle, out uint outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_tool_analog_input(nint handle, out double outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_tool_analog_output(nint handle, out double outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_tool_output_voltage(nint handle, out double outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_tool_output_current(nint handle, out double outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_tool_output_temperature(nint handle, out double outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_tool_digital_mode(nint handle, out uint outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_tool_digital_output_mode(nint handle, int index, out uint outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_out_bool_registers0_to_31(nint handle, out uint outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_out_bool_registers32_to_63(nint handle, out uint outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_in_bool_registers0_to_31(nint handle, out uint outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_in_bool_registers32_to_63(nint handle, out uint outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_in_bool_register(nint handle, int index, out int outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_out_bool_register(nint handle, int index, out int outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_in_int_register(nint handle, int index, out int outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_out_int_register(nint handle, int index, out int outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_in_double_register(nint handle, int index, out double outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_out_double_register(nint handle, int index, out double outValue);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_recipe_value_double(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        out double outValue,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_recipe_value_int32(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        out int outValue,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_recipe_value_uint32(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        out uint outValue,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_recipe_value_bool(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        out int outValue,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_recipe_value_vector3d(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        [Out] double[] outValue3,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_get_recipe_value_vector6d(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        [Out] double[] outValue6,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_set_input_recipe_value_double(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        double value,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_set_input_recipe_value_int32(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        int value,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_set_input_recipe_value_uint32(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        uint value,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_set_input_recipe_value_bool(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        int value,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EliteStatus elite_rtsi_io_set_input_recipe_value_vector6d(
        nint handle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        [In] double[] value6,
        out int outSuccess);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern nint elite_rtsi_io_last_error_message(nint handle);
}

internal sealed class EliteRtsiIOSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal EliteRtsiIOSafeHandle()
        : base(ownsHandle: true)
    {
    }

    internal EliteRtsiIOSafeHandle(nint preexistingHandle)
        : base(ownsHandle: true)
    {
        SetHandle(preexistingHandle);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.elite_rtsi_io_destroy(handle);
        return true;
    }
}
