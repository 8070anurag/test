using System.Runtime.InteropServices;

namespace HelloChannel.Client;

/// <summary>
/// Manages the COM LocalServer32 lifecycle.
/// Registers our class factory with COM so mstsc.exe can activate us.
/// </summary>
public class LocalServer : IDisposable
{
    private uint _registrationCookie;
    private SimpleClassFactory? _factory;
    private IntPtr _factoryPtr;
    private bool _disposed;

    /// <summary>
    /// Starts the COM local server — registers the HelloPlugin class factory.
    /// </summary>
    public void Start()
    {
        Console.WriteLine("[LocalServer] Initializing COM...");

        int hr = NativeMethods.CoInitializeEx(IntPtr.Zero, NativeMethods.COINIT_MULTITHREADED);
        if (hr < 0)
            Marshal.ThrowExceptionForHR(hr);

        _factory = new SimpleClassFactory();
        _factoryPtr = Marshal.GetComInterfaceForObject(_factory, typeof(IClassFactory));

        Guid clsid = HelloPlugin.CLSID;
        hr = NativeMethods.CoRegisterClassObject(
            ref clsid,
            _factoryPtr,
            NativeMethods.CLSCTX_LOCAL_SERVER,
            NativeMethods.REGCLS_MULTIPLEUSE | NativeMethods.REGCLS_SUSPENDED,
            out _registrationCookie);

        if (hr < 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }

        hr = NativeMethods.CoResumeClassObjects();
        if (hr < 0)
            Marshal.ThrowExceptionForHR(hr);

        Console.WriteLine($"[LocalServer] COM server registered. CLSID: {{{HelloPlugin.CLSID_String}}}");
        Console.WriteLine("[LocalServer] Waiting for mstsc.exe to activate plugin...");
    }

    /// <summary>
    /// Stops the COM server.
    /// </summary>
    public void Stop()
    {
        if (_registrationCookie != 0)
        {
            NativeMethods.CoRevokeClassObject(_registrationCookie);
            _registrationCookie = 0;
            Console.WriteLine("[LocalServer] COM server unregistered.");
        }

        if (_factoryPtr != IntPtr.Zero)
        {
            Marshal.Release(_factoryPtr);
            _factoryPtr = IntPtr.Zero;
        }

        NativeMethods.CoUninitialize();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
