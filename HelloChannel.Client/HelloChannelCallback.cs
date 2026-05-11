using System.Runtime.InteropServices;
using HelloChannel.Protocol;

namespace HelloChannel.Client;

/// <summary>
/// Handles data on a single DVC channel connection.
/// When the server (VM) sends a Hello message, this echoes back a Hi response.
/// </summary>
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
public class HelloChannelCallback : IWTSVirtualChannelCallback
{
    private readonly IWTSVirtualChannel _channel;

    public HelloChannelCallback(IWTSVirtualChannel channel)
    {
        _channel = channel;
    }

    /// <summary>
    /// Called when the server sends data through the DVC channel.
    /// </summary>
    public int OnDataReceived(uint cbSize, IntPtr pBuffer)
    {
        try
        {
            // Copy raw bytes from unmanaged buffer
            byte[] rawData = new byte[cbSize];
            Marshal.Copy(pBuffer, rawData, 0, (int)cbSize);

            // DVC delivers just the payload (no length prefix)
            var message = HelloMessage.DeserializePayload(rawData, 0, (int)cbSize);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[Channel] <<< Received: {message}");
            Console.ResetColor();

            // Send response back to VM
            var response = new HelloMessage
            {
                Type = MessageType.Hi,
                Text = "Hi from Host! 🖥️"
            };

            SendMessage(response);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Channel] ERROR processing data: {ex.Message}");
            Console.ResetColor();
        }

        return NativeMethods.S_OK;
    }

    /// <summary>
    /// Called when the channel is closed.
    /// </summary>
    public int OnClose()
    {
        Console.WriteLine("[Channel] Channel closed.");
        return NativeMethods.S_OK;
    }

    /// <summary>
    /// Serializes and sends a message through the DVC channel.
    /// We send only the payload (without the 4-byte length prefix) since DVC handles framing.
    /// </summary>
    private void SendMessage(HelloMessage message)
    {
        byte[] fullFrame = message.Serialize();

        // DVC handles framing, so send payload only (skip 4-byte length prefix)
        int payloadLen = fullFrame.Length - 4;
        IntPtr pBuffer = Marshal.AllocHGlobal(payloadLen);
        try
        {
            Marshal.Copy(fullFrame, 4, pBuffer, payloadLen);
            int hr = _channel.Write((uint)payloadLen, pBuffer, IntPtr.Zero);

            if (hr == NativeMethods.S_OK)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[Channel] >>> Sent: {message}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"[Channel] Write failed. HRESULT=0x{hr:X8}");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(pBuffer);
        }
    }
}
