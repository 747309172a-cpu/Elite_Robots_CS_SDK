using System.Runtime.InteropServices;
using System.Text;

namespace EliteRobots.CSharp;

public sealed class EliteDashboardClient : IDisposable
{
    private readonly EliteDashboardSafeHandle _handle;

    public EliteDashboardClient()
    {
        var status = NativeMethods.elite_dashboard_create(out var rawHandle);
        ThrowIfError(status, rawHandle);
        _handle = new EliteDashboardSafeHandle(rawHandle);
    }

    public bool connect(string ip, int port = 29999)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ip);
        var status = NativeMethods.elite_dashboard_connect(_handle.DangerousGetHandle(), ip, port, out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public void disconnect()
    {
        var status = NativeMethods.elite_dashboard_disconnect(_handle.DangerousGetHandle());
        ThrowIfError(status, _handle.DangerousGetHandle());
    }

    public bool echo()
    {
        var status = NativeMethods.elite_dashboard_echo(_handle.DangerousGetHandle(), out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool powerOn()
    {
        var status = NativeMethods.elite_dashboard_power_on(_handle.DangerousGetHandle(), out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool powerOff()
    {
        var status = NativeMethods.elite_dashboard_power_off(_handle.DangerousGetHandle(), out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool brakeRelease()
    {
        var status = NativeMethods.elite_dashboard_brake_release(_handle.DangerousGetHandle(), out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool closeSafetyDialog()
    {
        var status = NativeMethods.elite_dashboard_close_safety_dialog(_handle.DangerousGetHandle(), out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool unlockProtectiveStop()
    {
        var status = NativeMethods.elite_dashboard_unlock_protective_stop(_handle.DangerousGetHandle(), out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool safetySystemRestart()
    {
        var status = NativeMethods.elite_dashboard_safety_system_restart(_handle.DangerousGetHandle(), out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool log(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        var status = NativeMethods.elite_dashboard_log(_handle.DangerousGetHandle(), message, out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool popup(string arg, string message = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(arg);
        var status = NativeMethods.elite_dashboard_popup(_handle.DangerousGetHandle(), arg, message ?? string.Empty, out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public void quit()
    {
        var status = NativeMethods.elite_dashboard_quit(_handle.DangerousGetHandle());
        ThrowIfError(status, _handle.DangerousGetHandle());
    }

    public void reboot()
    {
        var status = NativeMethods.elite_dashboard_reboot(_handle.DangerousGetHandle());
        ThrowIfError(status, _handle.DangerousGetHandle());
    }

    public void shutdown()
    {
        var status = NativeMethods.elite_dashboard_shutdown(_handle.DangerousGetHandle());
        ThrowIfError(status, _handle.DangerousGetHandle());
    }

    public RobotMode robotMode()
    {
        var status = NativeMethods.elite_dashboard_robot_mode(_handle.DangerousGetHandle(), out var mode);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return (RobotMode)mode;
    }

    public SafetyMode safetyMode()
    {
        var status = NativeMethods.elite_dashboard_safety_mode(_handle.DangerousGetHandle(), out var mode);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return (SafetyMode)mode;
    }

    public TaskStatus runningStatus()
    {
        var status = NativeMethods.elite_dashboard_running_status(_handle.DangerousGetHandle(), out var runningStatus);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return (TaskStatus)runningStatus;
    }

    public TaskStatus getTaskStatus()
    {
        var status = NativeMethods.elite_dashboard_get_task_status(_handle.DangerousGetHandle(), out var taskStatus);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return (TaskStatus)taskStatus;
    }

    public int speedScaling()
    {
        var status = NativeMethods.elite_dashboard_speed_scaling(_handle.DangerousGetHandle(), out var scaling);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return scaling;
    }

    public bool setSpeedScaling(int scaling)
    {
        var status = NativeMethods.elite_dashboard_set_speed_scaling(_handle.DangerousGetHandle(), scaling, out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool taskIsRunning()
    {
        var status = NativeMethods.elite_dashboard_task_is_running(_handle.DangerousGetHandle(), out var running);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return running != 0;
    }

    public bool isTaskSaved()
    {
        var status = NativeMethods.elite_dashboard_is_task_saved(_handle.DangerousGetHandle(), out var saved);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return saved != 0;
    }

    public bool playProgram()
    {
        var status = NativeMethods.elite_dashboard_play_program(_handle.DangerousGetHandle(), out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool pauseProgram()
    {
        var status = NativeMethods.elite_dashboard_pause_program(_handle.DangerousGetHandle(), out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool stopProgram()
    {
        var status = NativeMethods.elite_dashboard_stop_program(_handle.DangerousGetHandle(), out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool loadConfiguration(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var status = NativeMethods.elite_dashboard_load_configuration(_handle.DangerousGetHandle(), path, out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool loadTask(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var status = NativeMethods.elite_dashboard_load_task(_handle.DangerousGetHandle(), path, out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public bool isConfigurationModify()
    {
        var status = NativeMethods.elite_dashboard_is_configuration_modify(_handle.DangerousGetHandle(), out var modified);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return modified != 0;
    }

    public string help(string cmd)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmd);
        return GetString((byte[]? buf, int len, out int required) =>
            NativeMethods.elite_dashboard_help(_handle.DangerousGetHandle(), cmd, buf, len, out required));
    }

    public string usage(string cmd)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmd);
        return GetString((byte[]? buf, int len, out int required) =>
            NativeMethods.elite_dashboard_usage(_handle.DangerousGetHandle(), cmd, buf, len, out required));
    }

    public string version()
    {
        return GetString((byte[]? buf, int len, out int required) =>
            NativeMethods.elite_dashboard_version(_handle.DangerousGetHandle(), buf, len, out required));
    }

    public string robotType()
    {
        return GetString((byte[]? buf, int len, out int required) =>
            NativeMethods.elite_dashboard_robot_type(_handle.DangerousGetHandle(), buf, len, out required));
    }

    public string robotSerialNumber()
    {
        return GetString((byte[]? buf, int len, out int required) =>
            NativeMethods.elite_dashboard_robot_serial_number(_handle.DangerousGetHandle(), buf, len, out required));
    }

    public string robotID()
    {
        return GetString((byte[]? buf, int len, out int required) =>
            NativeMethods.elite_dashboard_robot_id(_handle.DangerousGetHandle(), buf, len, out required));
    }

    public string configurationPath()
    {
        return GetString((byte[]? buf, int len, out int required) =>
            NativeMethods.elite_dashboard_configuration_path(_handle.DangerousGetHandle(), buf, len, out required));
    }

    public string getTaskPath()
    {
        return GetString((byte[]? buf, int len, out int required) =>
            NativeMethods.elite_dashboard_get_task_path(_handle.DangerousGetHandle(), buf, len, out required));
    }

    public string sendAndReceive(string cmd)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmd);
        return GetString((byte[]? buf, int len, out int required) =>
            NativeMethods.elite_dashboard_send_and_receive(_handle.DangerousGetHandle(), cmd, buf, len, out required));
    }

    public void Dispose()
    {
        _handle.Dispose();
    }

    private static void ThrowIfError(NativeMethods.EliteStatus status, nint handle)
    {
        if (status == NativeMethods.EliteStatus.Ok)
        {
            return;
        }

        var msgPtr = handle != nint.Zero
            ? NativeMethods.elite_dashboard_last_error_message(handle)
            : NativeMethods.elite_c_last_error_message();
        var message = Marshal.PtrToStringUTF8(msgPtr);
        if (string.IsNullOrWhiteSpace(message))
        {
            message = "native call failed";
        }

        throw new EliteSdkException(message, (int)status);
    }

    private delegate NativeMethods.EliteStatus StringCall(byte[]? outBuffer, int bufferLen, out int outRequiredLen);

    private string GetString(StringCall call)
    {
        var handle = _handle.DangerousGetHandle();

        var status = call(null, 0, out var requiredLen);
        ThrowIfError(status, handle);
        if (requiredLen <= 1)
        {
            return string.Empty;
        }

        var buffer = new byte[requiredLen];
        status = call(buffer, buffer.Length, out _);
        ThrowIfError(status, handle);

        var contentLength = buffer.Length;
        if (contentLength > 0 && buffer[contentLength - 1] == 0)
        {
            contentLength--;
        }
        return Encoding.UTF8.GetString(buffer, 0, contentLength);
    }
}
