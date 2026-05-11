using System.Runtime.InteropServices;

namespace HelloChannel.Client;

// ─────────────────────────────────────────────────────────────
//  RDP Dynamic Virtual Channel COM Interfaces
//  These GUIDs come from the Windows SDK header: tsvirtualchannels.h
// ─────────────────────────────────────────────────────────────

/// <summary>
/// Main plugin lifecycle interface. mstsc.exe calls these methods.
/// </summary>
[ComImport]
[Guid("A1230201-1439-4e62-a414-190d0ac3d40e")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IWTSPlugin
{
    [PreserveSig]
    int Initialize(IWTSVirtualChannelManager pChannelMgr);

    [PreserveSig]
    int Connected();

    [PreserveSig]
    int Disconnected(uint dwDisconnectCode);

    [PreserveSig]
    int Terminated();
}

/// <summary>
/// Provided by mstsc.exe — used to create listeners on named channels.
/// </summary>
[ComImport]
[Guid("A1230205-d6a7-11d8-b9fd-000bdbd1f198")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IWTSVirtualChannelManager
{
    [PreserveSig]
    int CreateListener(
        [MarshalAs(UnmanagedType.LPStr)] string pszChannelName,
        uint uFlags,
        IWTSListenerCallback pListenerCallback,
        out IWTSListener ppListener);
}

/// <summary>
/// The listener object returned by CreateListener.
/// </summary>
[ComImport]
[Guid("A1230206-9a39-4d58-8559-BE595D2D7D33")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IWTSListener
{
    [PreserveSig]
    int GetConfiguration(out IntPtr ppPropertyBag);
}

/// <summary>
/// Notified when a new channel connection arrives from the server side.
/// </summary>
[ComImport]
[Guid("A1230203-d6a7-11d8-b9fd-000bdbd1f198")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IWTSListenerCallback
{
    [PreserveSig]
    int OnNewChannelConnection(
        IWTSVirtualChannel pChannel,
        [MarshalAs(UnmanagedType.BStr)] string data,
        [MarshalAs(UnmanagedType.Bool)] out bool pAccept,
        out IWTSVirtualChannelCallback pCallback);
}

/// <summary>
/// Represents a virtual channel — used to write data back to the server.
/// </summary>
[ComImport]
[Guid("A1230207-d6a7-11d8-b9fd-000bdbd1f198")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IWTSVirtualChannel
{
    [PreserveSig]
    int Write(uint cbSize, IntPtr pBuffer, IntPtr pReserved);

    [PreserveSig]
    int Close();
}

/// <summary>
/// Handles per-channel data events (receive and close).
/// </summary>
[ComImport]
[Guid("A1230204-d6a7-11d8-b9fd-000bdbd1f198")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IWTSVirtualChannelCallback
{
    [PreserveSig]
    int OnDataReceived(uint cbSize, IntPtr pBuffer);

    [PreserveSig]
    int OnClose();
}

// ─────────────────────────────────────────────────────────────
//  Standard COM IClassFactory
// ─────────────────────────────────────────────────────────────

[ComImport]
[Guid("00000001-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IClassFactory
{
    [PreserveSig]
    int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject);

    [PreserveSig]
    int LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock);
}
