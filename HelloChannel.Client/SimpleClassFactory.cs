using System.Runtime.InteropServices;

namespace HelloChannel.Client;

/// <summary>
/// COM class factory that creates HelloPlugin instances.
/// mstsc.exe calls CreateInstance() when it activates our CLSID.
/// </summary>
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
public class SimpleClassFactory : IClassFactory
{
    // prevent GC of created plugins
    private readonly List<object> _instances = new();

    public int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject)
    {
        ppvObject = IntPtr.Zero;

        if (pUnkOuter != IntPtr.Zero)
            return NativeMethods.CLASS_E_NOAGGREGATION;

        Console.WriteLine("[ClassFactory] Creating HelloPlugin instance...");

        var plugin = new HelloPlugin();
        _instances.Add(plugin); // prevent GC

        ppvObject = Marshal.GetComInterfaceForObject(plugin, typeof(IWTSPlugin));

        Console.WriteLine("[ClassFactory] HelloPlugin created successfully.");
        return NativeMethods.S_OK;
    }

    public int LockServer(bool fLock)
    {
        return NativeMethods.S_OK;
    }
}
