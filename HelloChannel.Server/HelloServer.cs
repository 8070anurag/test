using System.Runtime.InteropServices;
using HelloChannel.Protocol;

namespace HelloChannel.Server;

/// <summary>
/// Server-side DVC handler. Runs inside the VM's RDP session.
/// Opens the "HELLO" dynamic virtual channel, sends Hello messages,
/// and reads Hi responses from the client plugin on the host desktop.
/// </summary>
public class HelloServer : IDisposable
{
    private const string ChannelName = "HELLO";
    private IntPtr _channelHandle = IntPtr.Zero;
    private bool _disposed;

    /// <summary>
    /// Opens the HELLO dynamic virtual channel.
    /// This will only work when running inside an RDP session.
    /// </summary>
    public bool OpenChannel()
    {
        Console.WriteLine($"[Server] Opening DVC channel '{ChannelName}'...");

        _channelHandle = NativeMethods.WTSVirtualChannelOpenEx(
            NativeMethods.WTS_CURRENT_SESSION,
            ChannelName,
            NativeMethods.WTS_CHANNEL_OPTION_DYNAMIC);

        if (_channelHandle == IntPtr.Zero || _channelHandle == NativeMethods.INVALID_HANDLE_VALUE)
        {
            int error = Marshal.GetLastWin32Error();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Server] ERROR: Failed to open channel. Win32 error={error}");
            Console.WriteLine();
            Console.WriteLine("  Common causes:");
            Console.WriteLine("  • Not running inside an RDP session");
            Console.WriteLine("  • Client plugin not registered on the host desktop");
            Console.WriteLine("  • Channel name mismatch");
            Console.ResetColor();
            _channelHandle = IntPtr.Zero;
            return false;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[Server] Channel '{ChannelName}' opened successfully!");
        Console.ResetColor();
        return true;
    }

    /// <summary>
    /// Sends a Hello message to the client plugin on the host desktop.
    /// </summary>
    public bool SendHello(string text)
    {
        if (_channelHandle == IntPtr.Zero)
            return false;

        var message = new HelloMessage
        {
            Type = MessageType.Hello,
            Text = text
        };

        byte[] fullFrame = message.Serialize();

        // DVC handles framing — send payload only (skip 4-byte length prefix)
        int payloadLen = fullFrame.Length - 4;
        byte[] payload = new byte[payloadLen];
        Array.Copy(fullFrame, 4, payload, 0, payloadLen);

        if (!NativeMethods.WTSVirtualChannelWrite(
                _channelHandle, payload, (uint)payloadLen, out uint bytesWritten))
        {
            int error = Marshal.GetLastWin32Error();
            Console.WriteLine($"[Server] Write failed. Win32 error={error}");
            return false;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[Server] >>> Sent: {message}");
        Console.ResetColor();
        return true;
    }

    /// <summary>
    /// Reads a response from the client plugin.
    /// </summary>
    public HelloMessage? ReadResponse(uint timeoutMs = 5000)
    {
        if (_channelHandle == IntPtr.Zero)
            return null;

        byte[] buffer = new byte[4096];

        if (!NativeMethods.WTSVirtualChannelRead(
                _channelHandle, timeoutMs, buffer, (uint)buffer.Length, out uint bytesRead))
        {
            int error = Marshal.GetLastWin32Error();
            if (error == 0)
                Console.WriteLine("[Server] Read timed out — no response from client.");
            else
                Console.WriteLine($"[Server] Read failed. Win32 error={error}");
            return null;
        }

        if (bytesRead == 0)
        {
            Console.WriteLine("[Server] Read returned 0 bytes.");
            return null;
        }

        try
        {
            // WTSVirtualChannelRead prepends an 8-byte CHANNEL_PDU_HEADER (Length, Flags)
            // Skip the first 8 bytes to get the actual payload
            int payloadOffset = 8;
            int payloadLength = (int)bytesRead - payloadOffset;

            var message = HelloMessage.DeserializePayload(buffer, payloadOffset, payloadLength);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[Server] <<< Received: {message}");
            Console.ResetColor();

            return message;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Failed to deserialize response: {ex.Message}");
            Console.WriteLine($"[Server] Raw bytes ({bytesRead}): {BitConverter.ToString(buffer, 0, (int)bytesRead)}");
            return null;
        }
    }

    /// <summary>
    /// Main loop: send Hello → read Hi → wait → repeat.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        int messageCount = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            messageCount++;
            string text = $"Hello from VM! 👋 (#{messageCount})";

            Console.WriteLine();
            Console.WriteLine($"─── Round {messageCount} ───");

            if (!SendHello(text))
            {
                Console.WriteLine("[Server] Send failed — stopping.");
                break;
            }

            // Read response from client
            var response = ReadResponse(5000);

            if (response != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[Server] ✅ Round-trip #{messageCount} complete!");
                Console.ResetColor();
            }

            // Wait 2 seconds before next message
            try
            {
                await Task.Delay(2000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        Console.WriteLine("[Server] Message loop ended.");
    }

    /// <summary>
    /// Closes the virtual channel.
    /// </summary>
    public void CloseChannel()
    {
        if (_channelHandle != IntPtr.Zero)
        {
            NativeMethods.WTSVirtualChannelClose(_channelHandle);
            _channelHandle = IntPtr.Zero;
            Console.WriteLine("[Server] Channel closed.");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            CloseChannel();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
