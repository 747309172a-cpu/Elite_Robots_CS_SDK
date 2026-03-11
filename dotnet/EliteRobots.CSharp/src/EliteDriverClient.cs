using System.Runtime.InteropServices;

namespace EliteRobots.CSharp;

public sealed class EliteDriverClient : IDisposable
{
    private readonly EliteDriverSafeHandle _handle;
    private readonly GCHandle _selfHandle;
    private NativeMethods.EliteDriverTrajectoryResultCallback? _nativeTrajectoryResultCallback;
    private Action<TrajectoryMotionResult>? _managedTrajectoryResultCallback;
    private NativeMethods.EliteDriverRobotExceptionCallback? _nativeRobotExceptionCallback;
    private Action<EliteDriverRobotException>? _managedRobotExceptionCallback;

    public EliteDriverClient(EliteDriverConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (string.IsNullOrWhiteSpace(config.RobotIp))
        {
            throw new ArgumentException("RobotIp is required", nameof(config));
        }
        if (string.IsNullOrWhiteSpace(config.ScriptFilePath))
        {
            throw new ArgumentException("ScriptFilePath is required", nameof(config));
        }

        NativeMethods.EliteDriverConfigNative native = default;
        NativeMethods.elite_driver_config_set_default(ref native);

        using var utf8 = new Utf8Scope();
        native.robot_ip = utf8.Alloc(config.RobotIp);
        native.script_file_path = utf8.Alloc(config.ScriptFilePath);
        native.local_ip = utf8.Alloc(config.LocalIp ?? string.Empty);
        native.headless_mode = config.HeadlessMode ? 1 : 0;
        native.script_sender_port = config.ScriptSenderPort;
        native.reverse_port = config.ReversePort;
        native.trajectory_port = config.TrajectoryPort;
        native.script_command_port = config.ScriptCommandPort;
        native.servoj_time = config.ServojTime;
        native.servoj_lookahead_time = config.ServojLookaheadTime;
        native.servoj_gain = config.ServojGain;
        native.stopj_acc = config.StopjAcc;

        var status = NativeMethods.elite_driver_create(ref native, out var rawHandle);
        ThrowIfError(status, rawHandle);
        _handle = new EliteDriverSafeHandle(rawHandle);
        _selfHandle = GCHandle.Alloc(this);
    }

    public bool isRobotConnected()
    {
        var status = NativeMethods.elite_driver_is_robot_connected(_handle.DangerousGetHandle(), out var connected);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return connected != 0;
    }

    public bool sendExternalControlScript()
    {
        var status = NativeMethods.elite_driver_send_external_control_script(_handle.DangerousGetHandle(), out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool stopControl(int wait_ms = 10000)
    {
        var status = NativeMethods.elite_driver_stop_control(_handle.DangerousGetHandle(), wait_ms, out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool writeServoj(double[] pos, int timeout_ms, bool cartesian = false)
    {
        ValidateVector6(pos, nameof(pos));
        var status = NativeMethods.elite_driver_write_servoj(
            _handle.DangerousGetHandle(),
            pos,
            timeout_ms,
            cartesian ? 1 : 0,
            out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool writeSpeedj(double[] vel, int timeout_ms)
    {
        ValidateVector6(vel, nameof(vel));
        var status = NativeMethods.elite_driver_write_speedj(_handle.DangerousGetHandle(), vel, timeout_ms, out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool writeSpeedl(double[] vel, int timeout_ms)
    {
        ValidateVector6(vel, nameof(vel));
        var status = NativeMethods.elite_driver_write_speedl(_handle.DangerousGetHandle(), vel, timeout_ms, out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public void setTrajectoryResultCallback(Action<TrajectoryMotionResult> cb)
    {
        ArgumentNullException.ThrowIfNull(cb);
        _managedTrajectoryResultCallback = cb;
        _nativeTrajectoryResultCallback ??= OnNativeTrajectoryResult;
        var status = NativeMethods.elite_driver_set_trajectory_result_callback(
            _handle.DangerousGetHandle(),
            _nativeTrajectoryResultCallback,
            GCHandle.ToIntPtr(_selfHandle));
        ThrowIfError(status, _handle.DangerousGetHandle());
    }

    public bool writeTrajectoryPoint(double[] positions, float time, float blend_radius, bool cartesian)
    {
        ValidateVector6(positions, nameof(positions));
        var status = NativeMethods.elite_driver_write_trajectory_point(
            _handle.DangerousGetHandle(),
            positions,
            time,
            blend_radius,
            cartesian ? 1 : 0,
            out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool writeTrajectoryControlAction(TrajectoryControlAction action, int point_number, int timeout_ms)
    {
        var status = NativeMethods.elite_driver_write_trajectory_control_action(
            _handle.DangerousGetHandle(),
            (int)action,
            point_number,
            timeout_ms,
            out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool writeIdle(int timeout_ms)
    {
        var status = NativeMethods.elite_driver_write_idle(_handle.DangerousGetHandle(), timeout_ms, out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool writeFreedrive(FreedriveAction action, int timeout_ms)
    {
        var status = NativeMethods.elite_driver_write_freedrive(
            _handle.DangerousGetHandle(),
            (int)action,
            timeout_ms,
            out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool zeroFTSensor()
    {
        var status = NativeMethods.elite_driver_zero_ft_sensor(_handle.DangerousGetHandle(), out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool setPayload(double mass, double[] cog)
    {
        ValidateVector3(cog, nameof(cog));
        var status = NativeMethods.elite_driver_set_payload(_handle.DangerousGetHandle(), mass, cog, out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool setToolVoltage(ToolVoltage vol)
    {
        var status = NativeMethods.elite_driver_set_tool_voltage(_handle.DangerousGetHandle(), (int)vol, out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool startForceMode(double[] reference_frame, int[] selection_vector, double[] wrench, ForceMode mode, double[] limits)
    {
        ValidateVector6(reference_frame, nameof(reference_frame));
        ValidateIntVector6(selection_vector, nameof(selection_vector));
        ValidateVector6(wrench, nameof(wrench));
        ValidateVector6(limits, nameof(limits));

        var status = NativeMethods.elite_driver_start_force_mode(
            _handle.DangerousGetHandle(),
            reference_frame,
            selection_vector,
            wrench,
            (int)mode,
            limits,
            out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool endForceMode()
    {
        var status = NativeMethods.elite_driver_end_force_mode(_handle.DangerousGetHandle(), out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool sendScript(string script)
    {
        ArgumentNullException.ThrowIfNull(script);
        var status = NativeMethods.elite_driver_send_script(_handle.DangerousGetHandle(), script, out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool getPrimaryPackage(PrimaryKinematicsInfo pkg, int timeout_ms)
    {
        ArgumentNullException.ThrowIfNull(pkg);
        ValidateVector6(pkg.DhA, nameof(pkg.DhA));
        ValidateVector6(pkg.DhD, nameof(pkg.DhD));
        ValidateVector6(pkg.DhAlpha, nameof(pkg.DhAlpha));

        var dhA = new double[6];
        var dhD = new double[6];
        var dhAlpha = new double[6];
        var status = NativeMethods.elite_driver_get_primary_package(
            _handle.DangerousGetHandle(),
            timeout_ms,
            dhA,
            dhD,
            dhAlpha,
            out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        if (success != 0)
        {
            Array.Copy(dhA, pkg.DhA, 6);
            Array.Copy(dhD, pkg.DhD, 6);
            Array.Copy(dhAlpha, pkg.DhAlpha, 6);
            return true;
        }
        return false;
    }

    public bool primaryReconnect()
    {
        var status = NativeMethods.elite_driver_primary_reconnect(_handle.DangerousGetHandle(), out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public void registerRobotExceptionCallback(Action<EliteDriverRobotException> cb)
    {
        ArgumentNullException.ThrowIfNull(cb);
        _managedRobotExceptionCallback = cb;
        _nativeRobotExceptionCallback ??= OnNativeRobotException;
        var status = NativeMethods.elite_driver_register_robot_exception_callback(
            _handle.DangerousGetHandle(),
            _nativeRobotExceptionCallback,
            GCHandle.ToIntPtr(_selfHandle));
        ThrowIfError(status, _handle.DangerousGetHandle());
    }

    public EliteSerialCommunication? startToolRs485(EliteSerialConfig config, string ssh_password, int tcp_port = 54321)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(ssh_password);
        NativeMethods.EliteSerialConfigNative nativeConfig = new()
        {
            baud_rate = (int)config.baud_rate,
            parity = (int)config.parity,
            stop_bits = (int)config.stop_bits,
        };
        var status = NativeMethods.elite_driver_start_tool_rs485(
            _handle.DangerousGetHandle(),
            ref nativeConfig,
            ssh_password,
            tcp_port,
            out var rawComm);
        ThrowIfError(status, _handle.DangerousGetHandle());
        if (rawComm == nint.Zero)
        {
            return null;
        }
        return new EliteSerialCommunication(new EliteSerialCommSafeHandle(rawComm));
    }

    public bool endToolRs485(EliteSerialCommunication comm, string ssh_password)
    {
        ArgumentNullException.ThrowIfNull(comm);
        ArgumentNullException.ThrowIfNull(ssh_password);
        var status = NativeMethods.elite_driver_end_tool_rs485(
            _handle.DangerousGetHandle(),
            comm.Handle.DangerousGetHandle(),
            ssh_password,
            out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public EliteSerialCommunication? startBoardRs485(EliteSerialConfig config, string ssh_password, int tcp_port = 54322)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(ssh_password);
        NativeMethods.EliteSerialConfigNative nativeConfig = new()
        {
            baud_rate = (int)config.baud_rate,
            parity = (int)config.parity,
            stop_bits = (int)config.stop_bits,
        };
        var status = NativeMethods.elite_driver_start_board_rs485(
            _handle.DangerousGetHandle(),
            ref nativeConfig,
            ssh_password,
            tcp_port,
            out var rawComm);
        ThrowIfError(status, _handle.DangerousGetHandle());
        if (rawComm == nint.Zero)
        {
            return null;
        }
        return new EliteSerialCommunication(new EliteSerialCommSafeHandle(rawComm));
    }

    public bool endBoardRs485(EliteSerialCommunication comm, string ssh_password)
    {
        ArgumentNullException.ThrowIfNull(comm);
        ArgumentNullException.ThrowIfNull(ssh_password);
        var status = NativeMethods.elite_driver_end_board_rs485(
            _handle.DangerousGetHandle(),
            comm.Handle.DangerousGetHandle(),
            ssh_password,
            out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public void Dispose()
    {
        _managedTrajectoryResultCallback = null;
        _managedRobotExceptionCallback = null;
        _handle.Dispose();
        if (_selfHandle.IsAllocated)
        {
            _selfHandle.Free();
        }
    }

    private static void ValidateVector6(double[] values, string name)
    {
        ArgumentNullException.ThrowIfNull(values, name);
        if (values.Length != 6)
        {
            throw new ArgumentException("Array length must be 6", name);
        }
    }

    private static void ValidateIntVector6(int[] values, string name)
    {
        ArgumentNullException.ThrowIfNull(values, name);
        if (values.Length != 6)
        {
            throw new ArgumentException("Array length must be 6", name);
        }
    }

    private static void ValidateVector3(double[] values, string name)
    {
        ArgumentNullException.ThrowIfNull(values, name);
        if (values.Length != 3)
        {
            throw new ArgumentException("Array length must be 3", name);
        }
    }

    private static void ThrowIfError(NativeMethods.EliteStatus status, nint handle)
    {
        if (status == NativeMethods.EliteStatus.Ok)
        {
            return;
        }

        var msgPtr = handle != nint.Zero
            ? NativeMethods.elite_driver_last_error_message(handle)
            : NativeMethods.elite_c_last_error_message();
        var message = Marshal.PtrToStringUTF8(msgPtr);
        if (string.IsNullOrWhiteSpace(message))
        {
            message = "native call failed";
        }

        throw new EliteSdkException(message, (int)status);
    }

    private static void OnNativeTrajectoryResult(int result, nint userData)
    {
        if (userData == nint.Zero)
        {
            return;
        }

        var gch = GCHandle.FromIntPtr(userData);
        if (gch.Target is not EliteDriverClient client || client._managedTrajectoryResultCallback is null)
        {
            return;
        }
        client._managedTrajectoryResultCallback((TrajectoryMotionResult)result);
    }

    private static void OnNativeRobotException(ref NativeMethods.EliteDriverRobotExceptionNative ex, nint userData)
    {
        if (userData == nint.Zero)
        {
            return;
        }

        var gch = GCHandle.FromIntPtr(userData);
        if (gch.Target is not EliteDriverClient client || client._managedRobotExceptionCallback is null)
        {
            return;
        }

        var managed = new EliteDriverRobotException
        {
            Type = ex.type,
            Timestamp = ex.timestamp,
            ErrorCode = ex.error_code,
            SubErrorCode = ex.sub_error_code,
            ErrorSource = ex.error_source,
            ErrorLevel = ex.error_level,
            ErrorDataType = ex.error_data_type,
            DataU32 = ex.data_u32,
            DataI32 = ex.data_i32,
            DataF32 = ex.data_f32,
            Line = ex.line,
            Column = ex.column,
            Message = Marshal.PtrToStringUTF8(ex.message) ?? string.Empty,
        };
        client._managedRobotExceptionCallback(managed);
    }

    private sealed class Utf8Scope : IDisposable
    {
        private readonly List<nint> _ptrs = new();

        public nint Alloc(string value)
        {
            var ptr = Marshal.StringToCoTaskMemUTF8(value);
            _ptrs.Add(ptr);
            return ptr;
        }

        public void Dispose()
        {
            foreach (var ptr in _ptrs)
            {
                Marshal.FreeCoTaskMem(ptr);
            }
            _ptrs.Clear();
        }
    }
}
