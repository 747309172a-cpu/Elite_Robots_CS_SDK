using System.Runtime.InteropServices;

namespace EliteRobots.CSharp;

public sealed class EliteRtsiRecipe : IDisposable
{
    private readonly EliteRtsiRecipeSafeHandle _handle;
    private readonly nint _clientHandle;

    internal EliteRtsiRecipe(nint clientHandle, nint rawHandle)
    {
        _clientHandle = clientHandle;
        _handle = new EliteRtsiRecipeSafeHandle(rawHandle);
    }

    public int getID()
    {
        var status = NativeMethods.elite_rtsi_recipe_get_id(_handle.DangerousGetHandle(), out var id);
        ThrowIfError(status, _clientHandle);
        return id;
    }

    public string[] getRecipe()
    {
        var first = NativeMethods.elite_rtsi_recipe_get_recipe_csv(_handle.DangerousGetHandle(), null, 0, out var required);
        ThrowIfError(first, _clientHandle);
        if (required <= 1)
        {
            return Array.Empty<string>();
        }

        var buffer = new byte[required];
        var second =
            NativeMethods.elite_rtsi_recipe_get_recipe_csv(_handle.DangerousGetHandle(), buffer, buffer.Length, out required);
        ThrowIfError(second, _clientHandle);
        var nul = Array.IndexOf(buffer, (byte)0);
        var len = nul >= 0 ? nul : buffer.Length;
        var csv = System.Text.Encoding.UTF8.GetString(buffer, 0, len);
        if (string.IsNullOrWhiteSpace(csv))
        {
            return Array.Empty<string>();
        }
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public bool getValue(string name, out double out_value)
    {
        ArgumentNullException.ThrowIfNull(name);
        var status = NativeMethods.elite_rtsi_recipe_get_value_double(_handle.DangerousGetHandle(), name, out out_value, out var ok);
        ThrowIfError(status, _clientHandle);
        return ok != 0;
    }

    public bool getValue(string name, out int out_value)
    {
        ArgumentNullException.ThrowIfNull(name);
        var status = NativeMethods.elite_rtsi_recipe_get_value_int32(_handle.DangerousGetHandle(), name, out out_value, out var ok);
        ThrowIfError(status, _clientHandle);
        return ok != 0;
    }

    public bool getValue(string name, out uint out_value)
    {
        ArgumentNullException.ThrowIfNull(name);
        var status = NativeMethods.elite_rtsi_recipe_get_value_uint32(_handle.DangerousGetHandle(), name, out out_value, out var ok);
        ThrowIfError(status, _clientHandle);
        return ok != 0;
    }

    public bool getValue(string name, out bool out_value)
    {
        ArgumentNullException.ThrowIfNull(name);
        var status = NativeMethods.elite_rtsi_recipe_get_value_bool(_handle.DangerousGetHandle(), name, out var value, out var ok);
        ThrowIfError(status, _clientHandle);
        out_value = value != 0;
        return ok != 0;
    }

    public bool getValue(string name, double[] out_value6)
    {
        ArgumentNullException.ThrowIfNull(name);
        ValidateVector6(out_value6, nameof(out_value6));
        var status =
            NativeMethods.elite_rtsi_recipe_get_value_vector6d(_handle.DangerousGetHandle(), name, out_value6, out var ok);
        ThrowIfError(status, _clientHandle);
        return ok != 0;
    }

    public bool setValue(string name, double value)
    {
        ArgumentNullException.ThrowIfNull(name);
        var status = NativeMethods.elite_rtsi_recipe_set_value_double(_handle.DangerousGetHandle(), name, value, out var ok);
        ThrowIfError(status, _clientHandle);
        return ok != 0;
    }

    public bool setValue(string name, int value)
    {
        ArgumentNullException.ThrowIfNull(name);
        var status = NativeMethods.elite_rtsi_recipe_set_value_int32(_handle.DangerousGetHandle(), name, value, out var ok);
        ThrowIfError(status, _clientHandle);
        return ok != 0;
    }

    public bool setValue(string name, uint value)
    {
        ArgumentNullException.ThrowIfNull(name);
        var status = NativeMethods.elite_rtsi_recipe_set_value_uint32(_handle.DangerousGetHandle(), name, value, out var ok);
        ThrowIfError(status, _clientHandle);
        return ok != 0;
    }

    public bool setValue(string name, bool value)
    {
        ArgumentNullException.ThrowIfNull(name);
        var status =
            NativeMethods.elite_rtsi_recipe_set_value_bool(_handle.DangerousGetHandle(), name, value ? 1 : 0, out var ok);
        ThrowIfError(status, _clientHandle);
        return ok != 0;
    }

    public bool setValue(string name, double[] value6)
    {
        ArgumentNullException.ThrowIfNull(name);
        ValidateVector6(value6, nameof(value6));
        var status = NativeMethods.elite_rtsi_recipe_set_value_vector6d(_handle.DangerousGetHandle(), name, value6, out var ok);
        ThrowIfError(status, _clientHandle);
        return ok != 0;
    }

    public void Dispose()
    {
        _handle.Dispose();
    }

    internal nint DangerousGetHandle() => _handle.DangerousGetHandle();

    private static void ValidateVector6(double[] values, string name)
    {
        ArgumentNullException.ThrowIfNull(values, name);
        if (values.Length != 6)
        {
            throw new ArgumentException("Array length must be 6", name);
        }
    }

    private static void ThrowIfError(NativeMethods.EliteStatus status, nint clientHandle)
    {
        if (status == NativeMethods.EliteStatus.Ok)
        {
            return;
        }

        var msgPtr = clientHandle != nint.Zero
            ? NativeMethods.elite_rtsi_client_last_error_message(clientHandle)
            : NativeMethods.elite_c_last_error_message();
        var message = Marshal.PtrToStringUTF8(msgPtr);
        if (string.IsNullOrWhiteSpace(message))
        {
            message = "native call failed";
        }

        throw new EliteSdkException(message, (int)status);
    }
}

public sealed class EliteRtsiClient : IDisposable
{
    private readonly EliteRtsiClientSafeHandle _handle;

    public EliteRtsiClient()
    {
        var status = NativeMethods.elite_rtsi_client_create(out var rawHandle);
        ThrowIfError(status, rawHandle);
        _handle = new EliteRtsiClientSafeHandle(rawHandle);
    }

    public void connect(string ip, int port = 30004)
    {
        ArgumentNullException.ThrowIfNull(ip);
        var status = NativeMethods.elite_rtsi_client_connect(_handle.DangerousGetHandle(), ip, port);
        ThrowIfError(status, _handle.DangerousGetHandle());
    }

    public void disconnect()
    {
        var status = NativeMethods.elite_rtsi_client_disconnect(_handle.DangerousGetHandle());
        ThrowIfError(status, _handle.DangerousGetHandle());
    }

    public bool negotiateProtocolVersion(ushort version = 1)
    {
        var status = NativeMethods.elite_rtsi_client_negotiate_protocol_version(_handle.DangerousGetHandle(), version, out var ok);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return ok != 0;
    }

    public RtsiVersionInfo getControllerVersion()
    {
        var status = NativeMethods.elite_rtsi_client_get_controller_version(_handle.DangerousGetHandle(), out var ver);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return new RtsiVersionInfo
        {
            Major = ver.major,
            Minor = ver.minor,
            Bugfix = ver.bugfix,
            Build = ver.build,
        };
    }

    public EliteRtsiRecipe setupOutputRecipe(IEnumerable<string> recipe_list, double frequency = 250)
    {
        ArgumentNullException.ThrowIfNull(recipe_list);
        var csv = string.Join(",", recipe_list);
        var status =
            NativeMethods.elite_rtsi_client_setup_output_recipe(_handle.DangerousGetHandle(), csv, frequency, out var recipe);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return new EliteRtsiRecipe(_handle.DangerousGetHandle(), recipe);
    }

    public EliteRtsiRecipe setupInputRecipe(IEnumerable<string> recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        var csv = string.Join(",", recipe);
        var status = NativeMethods.elite_rtsi_client_setup_input_recipe(_handle.DangerousGetHandle(), csv, out var outRecipe);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return new EliteRtsiRecipe(_handle.DangerousGetHandle(), outRecipe);
    }

    public bool start()
    {
        var status = NativeMethods.elite_rtsi_client_start(_handle.DangerousGetHandle(), out var ok);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return ok != 0;
    }

    public bool pause()
    {
        var status = NativeMethods.elite_rtsi_client_pause(_handle.DangerousGetHandle(), out var ok);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return ok != 0;
    }

    public void send(EliteRtsiRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        var status = NativeMethods.elite_rtsi_client_send(_handle.DangerousGetHandle(), recipe.DangerousGetHandle());
        ThrowIfError(status, _handle.DangerousGetHandle());
    }

    public bool receiveData(EliteRtsiRecipe recipe, bool read_newest = false)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        var status = NativeMethods.elite_rtsi_client_receive_data(
            _handle.DangerousGetHandle(),
            recipe.DangerousGetHandle(),
            read_newest ? 1 : 0,
            out var ok);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return ok != 0;
    }

    public int receiveData(IReadOnlyList<EliteRtsiRecipe> recipes, bool read_newest = false)
    {
        ArgumentNullException.ThrowIfNull(recipes);
        if (recipes.Count == 0)
        {
            throw new ArgumentException("recipes must not be empty", nameof(recipes));
        }

        var ptrs = new nint[recipes.Count];
        for (var i = 0; i < recipes.Count; i++)
        {
            ArgumentNullException.ThrowIfNull(recipes[i]);
            ptrs[i] = recipes[i].DangerousGetHandle();
        }

        var status = NativeMethods.elite_rtsi_client_receive_data_multi(
            _handle.DangerousGetHandle(),
            ptrs,
            ptrs.Length,
            read_newest ? 1 : 0,
            out var receivedIndex);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return receivedIndex;
    }

    public bool isConnected()
    {
        var status = NativeMethods.elite_rtsi_client_is_connected(_handle.DangerousGetHandle(), out var ok);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return ok != 0;
    }

    public bool isStarted()
    {
        var status = NativeMethods.elite_rtsi_client_is_started(_handle.DangerousGetHandle(), out var ok);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return ok != 0;
    }

    public bool isReadAvailable()
    {
        var status = NativeMethods.elite_rtsi_client_is_read_available(_handle.DangerousGetHandle(), out var ok);
        ThrowIfError(status, _handle.DangerousGetHandle());
        return ok != 0;
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
            ? NativeMethods.elite_rtsi_client_last_error_message(handle)
            : NativeMethods.elite_c_last_error_message();
        var message = Marshal.PtrToStringUTF8(msgPtr);
        if (string.IsNullOrWhiteSpace(message))
        {
            message = "native call failed";
        }

        throw new EliteSdkException(message, (int)status);
    }
}
