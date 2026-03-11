using System.Runtime.InteropServices;

namespace EliteRobots.CSharp;

public sealed class EliteRtsiIOClient : IDisposable
{
    private readonly EliteRtsiIOSafeHandle _handle;

    public EliteRtsiIOClient(IEnumerable<string> output_recipe, IEnumerable<string> input_recipe, double frequency = 250)
    {
        ArgumentNullException.ThrowIfNull(output_recipe);
        ArgumentNullException.ThrowIfNull(input_recipe);
        var outCsv = string.Join(",", output_recipe);
        var inCsv = string.Join(",", input_recipe);
        var status = NativeMethods.elite_rtsi_io_create(outCsv, inCsv, frequency, out var rawHandle);
        ThrowIfError(status, rawHandle);
        _handle = new EliteRtsiIOSafeHandle(rawHandle);
    }

    public bool connect(string ip)
    {
        ArgumentNullException.ThrowIfNull(ip);
        var status = NativeMethods.elite_rtsi_io_connect(_handle.DangerousGetHandle(), ip, out var ok);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return ok != 0;
    }

    public void disconnect()
    {
        var status = NativeMethods.elite_rtsi_io_disconnect(_handle.DangerousGetHandle());
        ThrowIfError(status, _handle.DangerousGetHandle());
    }

    public bool isConnected() => GetBool(NativeMethods.elite_rtsi_io_is_connected);
    public bool isStarted() => GetBool(NativeMethods.elite_rtsi_io_is_started);

    public RtsiVersionInfo getControllerVersion()
    {
        var status = NativeMethods.elite_rtsi_io_get_controller_version(_handle.DangerousGetHandle(), out var v);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return new RtsiVersionInfo { Major = v.major, Minor = v.minor, Bugfix = v.bugfix, Build = v.build };
    }

    public bool setSpeedScaling(double scaling) => SetBool((out int ok) =>
        NativeMethods.elite_rtsi_io_set_speed_scaling(_handle.DangerousGetHandle(), scaling, out ok));
    public bool setStandardDigital(int index, bool level) => SetBool((out int ok) =>
        NativeMethods.elite_rtsi_io_set_standard_digital(_handle.DangerousGetHandle(), index, level ? 1 : 0, out ok));
    public bool setConfigureDigital(int index, bool level) => SetBool((out int ok) =>
        NativeMethods.elite_rtsi_io_set_configure_digital(_handle.DangerousGetHandle(), index, level ? 1 : 0, out ok));
    public bool setAnalogOutputVoltage(int index, double value) => SetBool((out int ok) =>
        NativeMethods.elite_rtsi_io_set_analog_output_voltage(_handle.DangerousGetHandle(), index, value, out ok));
    public bool setAnalogOutputCurrent(int index, double value) => SetBool((out int ok) =>
        NativeMethods.elite_rtsi_io_set_analog_output_current(_handle.DangerousGetHandle(), index, value, out ok));
    public bool setExternalForceTorque(double[] value6) => SetBool((out int ok) =>
    {
        ValidateVector6(value6, nameof(value6));
        return NativeMethods.elite_rtsi_io_set_external_force_torque(_handle.DangerousGetHandle(), value6, out ok);
    });
    public bool setToolDigitalOutput(int index, bool level) => SetBool((out int ok) =>
        NativeMethods.elite_rtsi_io_set_tool_digital_output(_handle.DangerousGetHandle(), index, level ? 1 : 0, out ok));

    public double getTimestamp() => GetDouble(NativeMethods.elite_rtsi_io_get_timestamp);
    public double getPayloadMass() => GetDouble(NativeMethods.elite_rtsi_io_get_payload_mass);
    public double[] getPayloadCog() => GetVector3(NativeMethods.elite_rtsi_io_get_payload_cog);
    public uint getScriptControlLine() => GetUInt32(NativeMethods.elite_rtsi_io_get_script_control_line);
    public double[] getTargetJointPositions() => GetVector6(NativeMethods.elite_rtsi_io_get_target_joint_positions);
    public double[] getTargetJointVelocity() => GetVector6(NativeMethods.elite_rtsi_io_get_target_joint_velocity);
    public double[] getActualJointPositions() => GetVector6(NativeMethods.elite_rtsi_io_get_actual_joint_positions);
    public double[] getActualJointTorques() => GetVector6(NativeMethods.elite_rtsi_io_get_actual_joint_torques);
    public double[] getActualJointVelocity() => GetVector6(NativeMethods.elite_rtsi_io_get_actual_joint_velocity);
    public double[] getActualJointCurrent() => GetVector6(NativeMethods.elite_rtsi_io_get_actual_joint_current);
    public double[] getActualJointTemperatures() => GetVector6(NativeMethods.elite_rtsi_io_get_actual_joint_temperatures);
    public double[] getActualTCPPose() => GetVector6(NativeMethods.elite_rtsi_io_get_actual_tcp_pose);
    public double[] getActualTCPVelocity() => GetVector6(NativeMethods.elite_rtsi_io_get_actual_tcp_velocity);
    public double[] getActualTCPForce() => GetVector6(NativeMethods.elite_rtsi_io_get_actual_tcp_force);
    public double[] getTargetTCPPose() => GetVector6(NativeMethods.elite_rtsi_io_get_target_tcp_pose);
    public double[] getTargetTCPVelocity() => GetVector6(NativeMethods.elite_rtsi_io_get_target_tcp_velocity);
    public uint getDigitalInputBits() => GetUInt32(NativeMethods.elite_rtsi_io_get_digital_input_bits);
    public uint getDigitalOutputBits() => GetUInt32(NativeMethods.elite_rtsi_io_get_digital_output_bits);
    public RobotMode getRobotMode() => (RobotMode)GetInt32(NativeMethods.elite_rtsi_io_get_robot_mode);
    public JointMode[] getJointMode()
    {
        var vals = new int[6];
        var status = NativeMethods.elite_rtsi_io_get_joint_mode(_handle.DangerousGetHandle(), vals);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return vals.Select(v => (JointMode)v).ToArray();
    }
    public SafetyMode getSafetyStatus() => (SafetyMode)GetInt32(NativeMethods.elite_rtsi_io_get_safety_status);
    public uint getRobotStatus() => GetUInt32(NativeMethods.elite_rtsi_io_get_robot_status);
    public TaskStatus getRuntimeState() => (TaskStatus)GetInt32(NativeMethods.elite_rtsi_io_get_runtime_state);
    public double getActualSpeedScaling() => GetDouble(NativeMethods.elite_rtsi_io_get_actual_speed_scaling);
    public double getTargetSpeedScaling() => GetDouble(NativeMethods.elite_rtsi_io_get_target_speed_scaling);
    public double getRobotVoltage() => GetDouble(NativeMethods.elite_rtsi_io_get_robot_voltage);
    public double getRobotCurrent() => GetDouble(NativeMethods.elite_rtsi_io_get_robot_current);
    public double[] getElbowPosition() => GetVector3(NativeMethods.elite_rtsi_io_get_elbow_position);
    public double[] getElbowVelocity() => GetVector3(NativeMethods.elite_rtsi_io_get_elbow_velocity);
    public uint getSafetyStatusBits() => GetUInt32(NativeMethods.elite_rtsi_io_get_safety_status_bits);
    public uint getAnalogIOTypes() => GetUInt32(NativeMethods.elite_rtsi_io_get_analog_io_types);
    public double getAnalogInput(int index) => GetDouble((nint h, out double v) => NativeMethods.elite_rtsi_io_get_analog_input(h, index, out v));
    public double getAnalogOutput(int index) => GetDouble((nint h, out double v) => NativeMethods.elite_rtsi_io_get_analog_output(h, index, out v));
    public double getIOCurrent() => GetDouble(NativeMethods.elite_rtsi_io_get_io_current);
    public ToolMode getToolMode() => (ToolMode)GetUInt32(NativeMethods.elite_rtsi_io_get_tool_mode);
    public uint getToolAnalogInputType() => GetUInt32(NativeMethods.elite_rtsi_io_get_tool_analog_input_type);
    public uint getToolAnalogOutputType() => GetUInt32(NativeMethods.elite_rtsi_io_get_tool_analog_output_type);
    public double getToolAnalogInput() => GetDouble(NativeMethods.elite_rtsi_io_get_tool_analog_input);
    public double getToolAnalogOutput() => GetDouble(NativeMethods.elite_rtsi_io_get_tool_analog_output);
    public double getToolOutputVoltage() => GetDouble(NativeMethods.elite_rtsi_io_get_tool_output_voltage);
    public double getToolOutputCurrent() => GetDouble(NativeMethods.elite_rtsi_io_get_tool_output_current);
    public double getToolOutputTemperature() => GetDouble(NativeMethods.elite_rtsi_io_get_tool_output_temperature);
    public ToolDigitalMode getToolDigitalMode() => (ToolDigitalMode)GetUInt32(NativeMethods.elite_rtsi_io_get_tool_digital_mode);
    public ToolDigitalOutputMode getToolDigitalOutputMode(int index) =>
        (ToolDigitalOutputMode)GetUInt32((nint h, out uint v) => NativeMethods.elite_rtsi_io_get_tool_digital_output_mode(h, index, out v));
    public uint getOutBoolRegisters0To31() => GetUInt32(NativeMethods.elite_rtsi_io_get_out_bool_registers0_to_31);
    public uint getOutBoolRegisters32To63() => GetUInt32(NativeMethods.elite_rtsi_io_get_out_bool_registers32_to_63);
    public uint getInBoolRegisters0To31() => GetUInt32(NativeMethods.elite_rtsi_io_get_in_bool_registers0_to_31);
    public uint getInBoolRegisters32To63() => GetUInt32(NativeMethods.elite_rtsi_io_get_in_bool_registers32_to_63);
    public bool getInBoolRegister(int index) => GetInt32((nint h, out int v) => NativeMethods.elite_rtsi_io_get_in_bool_register(h, index, out v)) != 0;
    public bool getOutBoolRegister(int index) => GetInt32((nint h, out int v) => NativeMethods.elite_rtsi_io_get_out_bool_register(h, index, out v)) != 0;
    public int getInIntRegister(int index) => GetInt32((nint h, out int v) => NativeMethods.elite_rtsi_io_get_in_int_register(h, index, out v));
    public int getOutIntRegister(int index) => GetInt32((nint h, out int v) => NativeMethods.elite_rtsi_io_get_out_int_register(h, index, out v));
    public double getInDoubleRegister(int index) => GetDouble((nint h, out double v) => NativeMethods.elite_rtsi_io_get_in_double_register(h, index, out v));
    public double getOutDoubleRegister(int index) => GetDouble((nint h, out double v) => NativeMethods.elite_rtsi_io_get_out_double_register(h, index, out v));

    public bool getRecipeValue(string name, out double value)
    {
        var status = NativeMethods.elite_rtsi_io_get_recipe_value_double(_handle.DangerousGetHandle(), name, out value, out var ok);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return ok != 0;
    }
    public bool getRecipeValue(string name, out int value)
    {
        var status = NativeMethods.elite_rtsi_io_get_recipe_value_int32(_handle.DangerousGetHandle(), name, out value, out var ok);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return ok != 0;
    }
    public bool getRecipeValue(string name, out uint value)
    {
        var status = NativeMethods.elite_rtsi_io_get_recipe_value_uint32(_handle.DangerousGetHandle(), name, out value, out var ok);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return ok != 0;
    }
    public bool getRecipeValue(string name, out bool value)
    {
        var status = NativeMethods.elite_rtsi_io_get_recipe_value_bool(_handle.DangerousGetHandle(), name, out var v, out var ok);
        ThrowIfError(status, _handle.DangerousGetHandle());
        value = v != 0;
        return ok != 0;
    }
    public bool getRecipeValue(string name, double[] value3or6)
    {
        ArgumentNullException.ThrowIfNull(value3or6);
        if (value3or6.Length == 3)
        {
            var status = NativeMethods.elite_rtsi_io_get_recipe_value_vector3d(_handle.DangerousGetHandle(), name, value3or6, out var ok);
            ThrowIfError(status, _handle.DangerousGetHandle());
            return ok != 0;
        }
        if (value3or6.Length == 6)
        {
            var status = NativeMethods.elite_rtsi_io_get_recipe_value_vector6d(_handle.DangerousGetHandle(), name, value3or6, out var ok);
            ThrowIfError(status, _handle.DangerousGetHandle());
            return ok != 0;
        }
        throw new ArgumentException("Array length must be 3 or 6", nameof(value3or6));
    }

    public bool setInputRecipeValue(string name, double value) => SetBool((out int ok) =>
        NativeMethods.elite_rtsi_io_set_input_recipe_value_double(_handle.DangerousGetHandle(), name, value, out ok));
    public bool setInputRecipeValue(string name, int value) => SetBool((out int ok) =>
        NativeMethods.elite_rtsi_io_set_input_recipe_value_int32(_handle.DangerousGetHandle(), name, value, out ok));
    public bool setInputRecipeValue(string name, uint value) => SetBool((out int ok) =>
        NativeMethods.elite_rtsi_io_set_input_recipe_value_uint32(_handle.DangerousGetHandle(), name, value, out ok));
    public bool setInputRecipeValue(string name, bool value) => SetBool((out int ok) =>
        NativeMethods.elite_rtsi_io_set_input_recipe_value_bool(_handle.DangerousGetHandle(), name, value ? 1 : 0, out ok));
    public bool setInputRecipeValue(string name, double[] value6) => SetBool((out int ok) =>
    {
        ValidateVector6(value6, nameof(value6));
        return NativeMethods.elite_rtsi_io_set_input_recipe_value_vector6d(_handle.DangerousGetHandle(), name, value6, out ok);
    });

    public void Dispose() => _handle.Dispose();

    private delegate NativeMethods.EliteStatus BoolGetter(nint handle, out int value);
    private delegate NativeMethods.EliteStatus DoubleGetter(nint handle, out double value);
    private delegate NativeMethods.EliteStatus IntGetter(nint handle, out int value);
    private delegate NativeMethods.EliteStatus UIntGetter(nint handle, out uint value);
    private delegate NativeMethods.EliteStatus BoolSetter(out int ok);

    private bool GetBool(BoolGetter fn)
    {
        var status = fn(_handle.DangerousGetHandle(), out var v);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return v != 0;
    }

    private double GetDouble(DoubleGetter fn)
    {
        var status = fn(_handle.DangerousGetHandle(), out var v);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return v;
    }

    private int GetInt32(IntGetter fn)
    {
        var status = fn(_handle.DangerousGetHandle(), out var v);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return v;
    }

    private uint GetUInt32(UIntGetter fn)
    {
        var status = fn(_handle.DangerousGetHandle(), out var v);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return v;
    }

    private bool SetBool(BoolSetter fn)
    {
        var status = fn(out var ok);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return ok != 0;
    }

    private double[] GetVector3(Func<nint, double[], NativeMethods.EliteStatus> fn)
    {
        var v = new double[3];
        var status = fn(_handle.DangerousGetHandle(), v);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return v;
    }

    private double[] GetVector6(Func<nint, double[], NativeMethods.EliteStatus> fn)
    {
        var v = new double[6];
        var status = fn(_handle.DangerousGetHandle(), v);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return v;
    }

    private static void ValidateVector6(double[] values, string name)
    {
        ArgumentNullException.ThrowIfNull(values, name);
        if (values.Length != 6)
        {
            throw new ArgumentException("Array length must be 6", name);
        }
    }

    private static void ThrowIfError(NativeMethods.EliteStatus status, nint handle)
    {
        if (status == NativeMethods.EliteStatus.Ok)
        {
            return;
        }

        var msgPtr = handle != nint.Zero
            ? NativeMethods.elite_rtsi_io_last_error_message(handle)
            : NativeMethods.elite_c_last_error_message();
        var message = Marshal.PtrToStringUTF8(msgPtr);
        if (string.IsNullOrWhiteSpace(message))
        {
            message = "native call failed";
        }
        throw new EliteSdkException(message, (int)status);
    }
}
