using Microsoft.Win32;

namespace HelloChannel.Client;

/// <summary>
/// Client plugin entry point.
/// Usage:
///   HelloChannel.Client.exe /register     — Register COM plugin in registry (per-user)
///   HelloChannel.Client.exe /unregister   — Remove COM plugin from registry
///   HelloChannel.Client.exe -Embedding    — Started by COM (mstsc.exe activates us)
///   HelloChannel.Client.exe               — Start COM server interactively (for testing)
/// </summary>
class Program
{
    // Registry paths (per-user, no admin needed)
    private const string AddinKeyPath =
        @"Software\Microsoft\Terminal Server Client\Default\AddIns\HelloChannel";
    private const string ClsidKeyPath =
        @"Software\Classes\CLSID\{" + HelloPlugin.CLSID_String + "}";
    private const string LocalServer32KeyPath =
        ClsidKeyPath + @"\LocalServer32";

    static int Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        string command = args.Length > 0 ? args[0].ToLowerInvariant() : "";

        switch (command)
        {
            case "/register":
                return Register();

            case "/unregister":
                return Unregister();

            case "-embedding":
                // COM launched us — run as server
                Console.WriteLine("[Program] Launched by COM (-Embedding).");
                return RunServer();

            default:
                // Interactive mode — run as server for debugging
                Console.WriteLine("╔══════════════════════════════════════════════════╗");
                Console.WriteLine("║   HelloChannel Client Plugin (Interactive Mode)  ║");
                Console.WriteLine("╚══════════════════════════════════════════════════╝");
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine("  /register     Register plugin (per-user)");
                Console.WriteLine("  /unregister   Unregister plugin");
                Console.WriteLine("  -Embedding    COM activation (mstsc.exe)");
                Console.WriteLine("  (no args)     Start server interactively");
                Console.WriteLine();
                return RunServer();
        }
    }

    /// <summary>
    /// Registers the COM plugin in the current user's registry.
    /// mstsc.exe will discover and load this plugin on next RDP connection.
    /// </summary>
    static int Register()
    {
        try
        {
            string exePath = Environment.ProcessPath
                ?? System.Reflection.Assembly.GetExecutingAssembly().Location;

            Console.WriteLine("[Register] Registering HelloChannel DVC plugin...");
            Console.WriteLine($"  EXE path: {exePath}");
            Console.WriteLine($"  CLSID:    {{{HelloPlugin.CLSID_String}}}");

            // 1. Register the CLSID → LocalServer32
            using (var key = Registry.CurrentUser.CreateSubKey(LocalServer32KeyPath))
            {
                key.SetValue("", exePath);
                Console.WriteLine($"  ✅ CLSID registered at HKCU\\{LocalServer32KeyPath}");
            }

            // 2. Register the AddIn so mstsc.exe discovers it
            using (var key = Registry.CurrentUser.CreateSubKey(AddinKeyPath))
            {
                key.SetValue("Name", $"{{{HelloPlugin.CLSID_String}}}");
                Console.WriteLine($"  ✅ AddIn registered at HKCU\\{AddinKeyPath}");
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Registration complete! The plugin will load next time you start mstsc.exe.");
            Console.ResetColor();
            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Register] ERROR: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    /// <summary>
    /// Removes the COM plugin registration from the current user's registry.
    /// </summary>
    static int Unregister()
    {
        try
        {
            Console.WriteLine("[Unregister] Removing HelloChannel DVC plugin...");

            Registry.CurrentUser.DeleteSubKeyTree(AddinKeyPath, false);
            Console.WriteLine($"  ✅ AddIn removed: HKCU\\{AddinKeyPath}");

            Registry.CurrentUser.DeleteSubKeyTree(ClsidKeyPath, false);
            Console.WriteLine($"  ✅ CLSID removed: HKCU\\{ClsidKeyPath}");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Unregistration complete!");
            Console.ResetColor();
            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Unregister] ERROR: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    /// <summary>
    /// Starts the COM local server and waits.
    /// </summary>
    static int RunServer()
    {
        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            Console.WriteLine("[Program] Ctrl+C — shutting down...");
        };

        using var server = new LocalServer();
        try
        {
            server.Start();
            Console.WriteLine("[Program] Press Ctrl+C to stop.");
            Console.WriteLine();

            // Wait until cancelled
            cts.Token.WaitHandle.WaitOne();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Program] ERROR: {ex.Message}");
            Console.ResetColor();
            return 1;
        }

        Console.WriteLine("[Program] Stopped.");
        return 0;
    }
}
