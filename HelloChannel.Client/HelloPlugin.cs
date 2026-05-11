using System.Runtime.InteropServices;

namespace HelloChannel.Client;

/// <summary>
/// Main DVC plugin. Implements IWTSPlugin which mstsc.exe calls
/// to initialize, connect, and terminate the plugin.
/// </summary>
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[Guid(CLSID_String)]
public class HelloPlugin : IWTSPlugin
{
    public const string CLSID_String = "B4F3D5A7-2E8C-4F1A-9D6B-3C7E8A5F2D1E";
    public static readonly Guid CLSID = new Guid(CLSID_String);

    public const string ChannelName = "HELLO";

    private IWTSVirtualChannelManager? _channelManager;
    private IWTSListener? _listener;
    private HelloListenerCallback? _listenerCallback;

    public int Initialize(IWTSVirtualChannelManager pChannelMgr)
    {
        Console.WriteLine($"[Plugin] Initialize — creating listener for channel '{ChannelName}'...");

        _channelManager = pChannelMgr;
        _listenerCallback = new HelloListenerCallback();

        int hr = _channelManager.CreateListener(ChannelName, 0, _listenerCallback, out _listener);

        if (hr != 0)
        {
            Console.WriteLine($"[Plugin] ERROR: CreateListener failed. HRESULT=0x{hr:X8}");
            return hr;
        }

        Console.WriteLine($"[Plugin] Listener ready on channel '{ChannelName}'.");
        return NativeMethods.S_OK;
    }

    public int Connected()
    {
        Console.WriteLine("[Plugin] Connected — RDP session is active.");
        return NativeMethods.S_OK;
    }

    public int Disconnected(uint dwDisconnectCode)
    {
        Console.WriteLine($"[Plugin] Disconnected (code={dwDisconnectCode}).");
        return NativeMethods.S_OK;
    }

    public int Terminated()
    {
        Console.WriteLine("[Plugin] Terminated — cleaning up.");
        _listener = null;
        _listenerCallback = null;
        _channelManager = null;
        return NativeMethods.S_OK;
    }
}
