using System.Runtime.InteropServices;

namespace HelloChannel.Client;

/// <summary>
/// P/Invoke declarations for COM server registration (ole32.dll).
/// </summary>
internal static class NativeMethods
{
    [DllImport("ole32.dll")]
    public static extern int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

    [DllImport("ole32.dll")]
    public static extern int CoRegisterClassObject(
        ref Guid rclsid,
        IntPtr pUnk,
        uint dwClsContext,
        uint flags,
        out uint lpdwRegister);

    [DllImport("ole32.dll")]
    public static extern int CoRevokeClassObject(uint dwRegister);

    [DllImport("ole32.dll")]
    public static extern int CoResumeClassObjects();

    [DllImport("ole32.dll")]
    public static extern void CoUninitialize();

    // ── Constants ──

    // COINIT
    public const uint COINIT_MULTITHREADED = 0x0;
    public const uint COINIT_APARTMENTTHREADED = 0x2;

    // CLSCTX
    public const uint CLSCTX_LOCAL_SERVER = 0x4;

    // REGCLS
    public const uint REGCLS_MULTIPLEUSE = 1;
    public const uint REGCLS_SUSPENDED = 4;

    // HRESULT helpers
    public const int S_OK = 0;
    public const int S_FALSE = 1;
    public const int CLASS_E_NOAGGREGATION = unchecked((int)0x80040110);
    public const int E_NOINTERFACE = unchecked((int)0x80004002);
}
