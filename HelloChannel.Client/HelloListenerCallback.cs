using System.Runtime.InteropServices;

namespace HelloChannel.Client;

/// <summary>
/// Called by mstsc.exe when the server side opens the "HELLO" channel.
/// Creates a HelloChannelCallback for each new channel connection.
/// </summary>
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
public class HelloListenerCallback : IWTSListenerCallback
{
    // prevent GC of channel callbacks
    private readonly List<HelloChannelCallback> _callbacks = new();

    public int OnNewChannelConnection(
        IWTSVirtualChannel pChannel,
        string data,
        out bool pAccept,
        out IWTSVirtualChannelCallback pCallback)
    {
        Console.WriteLine("[Listener] New channel connection incoming!");

        var channelCallback = new HelloChannelCallback(pChannel);
        _callbacks.Add(channelCallback);

        pAccept = true;
        pCallback = channelCallback;

        Console.WriteLine("[Listener] Channel connection accepted.");
        return NativeMethods.S_OK;
    }
}
