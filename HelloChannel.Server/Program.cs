namespace HelloChannel.Server;

/// <summary>
/// Server application entry point.
/// Run this inside the VM's RDP session to send messages
/// to the client plugin on the host desktop.
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("╔══════════════════════════════════════════════════╗");
        Console.WriteLine("║   HelloChannel Server (runs inside VM / RDP)     ║");
        Console.WriteLine("╚══════════════════════════════════════════════════╝");
        Console.WriteLine();

        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            Console.WriteLine();
            Console.WriteLine("[Program] Ctrl+C — shutting down...");
        };

        using var server = new HelloServer();

        if (!server.OpenChannel())
        {
            Console.WriteLine();
            Console.WriteLine("Make sure:");
            Console.WriteLine("  1. You are running this inside an RDP session");
            Console.WriteLine("  2. The client plugin is registered on the host desktop");
            Console.WriteLine("  3. The RDP connection was started AFTER plugin registration");
            return 1;
        }

        Console.WriteLine();
        Console.WriteLine("[Program] Sending Hello messages every 2 seconds...");
        Console.WriteLine("[Program] Press Ctrl+C to stop.");
        Console.WriteLine();

        await server.RunAsync(cts.Token);

        Console.WriteLine("[Program] Done.");
        return 0;
    }
}
