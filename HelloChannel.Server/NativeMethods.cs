using System.Runtime.InteropServices;

namespace HelloChannel.Server;

/// <summary>
/// P/Invoke declarations for WTS Virtual Channel APIs (wtsapi32.dll).
/// These are the Windows Terminal Services APIs used to open and
/// communicate over dynamic virtual channels from the server (VM) side.
/// </summary>
internal static class NativeMethods
{
    /// <summary>
    /// Opens a dynamic virtual channel in the current RDP session.
    /// </summary>
    [DllImport("wtsapi32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern IntPtr WTSVirtualChannelOpenEx(
        uint SessionId,
        string pVirtualName,
        uint flags);

    /// <summary>
    /// Writes data to an open virtual channel.
    /// </summary>
    [DllImport("wtsapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WTSVirtualChannelWrite(
        IntPtr hChannelHandle,
        byte[] Buffer,
        uint Length,
        out uint pBytesWritten);

    /// <summary>
    /// Reads data from a virtual channel (blocking).
    /// </summary>
    [DllImport("wtsapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WTSVirtualChannelRead(
        IntPtr hChannelHandle,
        uint TimeOut,
        byte[] Buffer,
        uint BufferSize,
        out uint pBytesRead);

    /// <summary>
    /// Closes a virtual channel handle.
    /// </summary>
    [DllImport("wtsapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WTSVirtualChannelClose(IntPtr hChannelHandle);

    // ── Constants ──

    /// <summary>Use the current RDP session.</summary>
    public const uint WTS_CURRENT_SESSION = 0xFFFFFFFF;

    /// <summary>Open a Dynamic Virtual Channel (DVC) instead of a static one.</summary>
    public const uint WTS_CHANNEL_OPTION_DYNAMIC = 0x00000001;

    /// <summary>Use PRI_HIGH priority for DVC.</summary>
    public const uint WTS_CHANNEL_OPTION_DYNAMIC_PRI_HIGH = 0x00000004;

    public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
}
