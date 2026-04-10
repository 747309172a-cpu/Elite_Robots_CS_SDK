using System.Runtime.InteropServices;
using System.Text;

namespace EliteRobots.CSharp;

public sealed class PrimaryClientInterface : IDisposable
{
    private readonly ElitePrimarySafeHandle _handle;
    private NativeMethods.ElitePrimaryRobotExceptionCallback? _nativeRobotExceptionCallback;
    private Action<PrimaryRobotException>? _managedRobotExceptionCallback;
    private GCHandle _selfHandle;
    private bool _disposed;

    public PrimaryClientInterface()
    {
        var status = NativeMethods.elite_primary_create(out var rawHandle);
        ThrowIfError(status, rawHandle);
        _handle = new ElitePrimarySafeHandle(rawHandle);
        _selfHandle = GCHandle.Alloc(this, GCHandleType.Normal);
    }

    public bool connect(string ip, int port = 30001)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ip);
        var status = NativeMethods.elite_primary_connect(_handle.DangerousGetHandle(), ip, port, out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public void disconnect()
    {
        var status = NativeMethods.elite_primary_disconnect(_handle.DangerousGetHandle());
        ThrowIfError(status, _handle.DangerousGetHandle());
    }

    public bool sendScript(string script)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(script);
        var status = NativeMethods.elite_primary_send_script(_handle.DangerousGetHandle(), script, out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return success != 0;
    }

    public string getLocalIP()
    {
        var handle = _handle.DangerousGetHandle();
        var status = NativeMethods.elite_primary_get_local_ip(handle, null, 0, out var requiredLen);
        ThrowIfError(status, handle);
        if (requiredLen <= 1)
        {
            return string.Empty;
        }

        var buffer = new byte[requiredLen];
        status = NativeMethods.elite_primary_get_local_ip(handle, buffer, buffer.Length, out _);
        ThrowIfError(status, handle);

        var contentLength = buffer.Length;
        if (contentLength > 0 && buffer[contentLength - 1] == 0)
        {
            contentLength--;
        }
        return Encoding.UTF8.GetString(buffer, 0, contentLength);
    }


    public bool getPackage(out PrimaryKinematicsInfo info, int timeoutMs = 1000)
    {
        var dhA = new double[6];
        var dhD = new double[6];
        var dhAlpha = new double[6];
        var status = NativeMethods.elite_primary_get_kinematics_info(
            _handle.DangerousGetHandle(),
            timeoutMs,
            dhA,
            dhD,
            dhAlpha,
            out var success);
        ThrowIfError(status, _handle.DangerousGetHandle());

        info = new PrimaryKinematicsInfo
        {
            DhA = dhA,
            DhD = dhD,
            DhAlpha = dhAlpha,
        };
        return success != 0;
    }

    public void registerRobotExceptionCallback(Action<PrimaryRobotException> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _managedRobotExceptionCallback = callback;
        _nativeRobotExceptionCallback ??= OnNativeRobotException;
        var status = NativeMethods.elite_primary_register_robot_exception_callback(
            _handle.DangerousGetHandle(),
            _nativeRobotExceptionCallback,
            GCHandle.ToIntPtr(_selfHandle));
        ThrowIfError(status, _handle.DangerousGetHandle());
    }

    public void clearRobotExceptionCallback()
    {
        _managedRobotExceptionCallback = null;
    }


    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;

        _managedRobotExceptionCallback = null;
        _handle.Dispose();
        if (_selfHandle.IsAllocated)
        {
            _selfHandle.Free();
        }
    }

    private static void OnNativeRobotException(ref NativeMethods.ElitePrimaryRobotExceptionNative ex, nint userData)
    {
        if (userData == nint.Zero)
        {
            return;
        }

        var gch = GCHandle.FromIntPtr(userData);
        if (gch.Target is not PrimaryClientInterface client || client._managedRobotExceptionCallback is null)
        {
            return;
        }

        var message = ex.message == nint.Zero ? string.Empty : (Marshal.PtrToStringUTF8(ex.message) ?? string.Empty);
        var managed = new PrimaryRobotException
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
            Message = message,
        };

        client._managedRobotExceptionCallback(managed);
    }

    private static void ThrowIfError(NativeMethods.EliteStatus status, nint handle)
    {
        if (status == NativeMethods.EliteStatus.Ok)
        {
            return;
        }

        var msgPtr = NativeMethods.elite_c_last_error_message();
        var message = Marshal.PtrToStringUTF8(msgPtr);
        if (string.IsNullOrWhiteSpace(message))
        {
            message = "native call failed";
        }

        throw new EliteSdkException(message, (int)status);
    }
}
