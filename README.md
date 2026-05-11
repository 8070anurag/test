<<<<<<< HEAD
# HelloChannel — RDP Dynamic Virtual Channel

A simple "Hello/Hi" message exchange between your **host desktop** (RDP client) and your **VM** (RDP server) using RDP Dynamic Virtual Channels (DVC).

## Architecture

```
┌─────────────────────────┐                    ┌─────────────────────────┐
│   HOST DESKTOP          │     RDP Session    │   VIRTUAL MACHINE       │
│                         │◄══════════════════►│                         │
│  mstsc.exe              │   DVC: "HELLO"     │  HelloChannel.Server    │
│    └─ HelloChannel      │                    │                         │
│       .Client.exe       │  ← "Hello!" →      │  Opens channel via      │
│       (COM Plugin)      │  ← "Hi!" →         │  WTS API                │
└─────────────────────────┘                    └─────────────────────────┘
```

## Projects

| Project | Purpose | Runs On |
|---------|---------|---------|
| `HelloChannel.Protocol` | Shared message serialization | Both (library) |
| `HelloChannel.Client` | COM plugin loaded by mstsc.exe | Host Desktop |
| `HelloChannel.Server` | Console app using WTS API | VM (inside RDP) |

## Setup Instructions

### Step 1: Build the Solution (on your desktop)

```powershell
cd d:\test
dotnet build HelloChannel.slnx
```

### Step 2: Register the Client Plugin (on your desktop)

```powershell
dotnet run --project HelloChannel.Client -- /register
```

This creates registry entries under HKCU (no admin needed):
- `HKCU\Software\Microsoft\Terminal Server Client\Default\AddIns\HelloChannel`
- `HKCU\Software\Classes\CLSID\{B4F3D5A7-2E8C-4F1A-9D6B-3C7E8A5F2D1E}`

### Step 3: Copy Server App to VM

Copy the folder `HelloChannel.Server\bin\Debug\net10.0-windows\` to your VM.
Also copy `HelloChannel.Protocol\bin\Debug\net10.0-windows\HelloChannel.Protocol.dll` alongside it.

Or publish as self-contained:
```powershell
dotnet publish HelloChannel.Server -c Release -r win-x64 --self-contained -o publish\server
```
Then copy the `publish\server` folder to the VM.

### Step 4: Connect via RDP

Open a NEW mstsc.exe connection to your VM. The plugin loads on connection start.

### Step 5: Run the Server (inside the VM)

In the RDP session, run:
```powershell
HelloChannel.Server.exe
```

### Expected Output

**On the VM (server):**
```
[Server] Channel 'HELLO' opened successfully!
─── Round 1 ───
[Server] >>> Sent: [Hello] Hello from VM! 👋 (#1)
[Server] <<< Received: [Hi] Hi from Host! 🖥️
[Server] ✅ Round-trip #1 complete!
```

**On the host desktop (client plugin):**
```
[Channel] <<< Received: [Hello] Hello from VM! 👋 (#1)
[Channel] >>> Sent: [Hi] Hi from Host! 🖥️
```

## Unregister

```powershell
dotnet run --project HelloChannel.Client -- /unregister
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Server says "Failed to open channel" | Not in an RDP session, or plugin not registered |
| Plugin not loading | Close mstsc, re-register, re-connect |
| Need .NET on VM | Publish self-contained (Step 3 alternative) |
=======
# test
>>>>>>> ed365c49020f7530b66bdff01ef5a76dc2568b37
