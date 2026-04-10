using System.Net;
using System.Net.Sockets;
using EliteRobots.CSharp;

internal static class ConnectRobotTestExample
{
    internal static void Run(string[] args)
    {
        if (args.Length < 2)
        {
            PrintUsage();
            return;
        }

        var ip = args[1];
        var localIp = string.Empty;
        var serverPort = 50002;
        var waitMs = 5000;

        for (var i = 2; i < args.Length; i++)
        {
            if (args[i] == "--local-ip" && i + 1 < args.Length)
            {
                localIp = args[++i];
            }
            else if (args[i] == "--server-port" && i + 1 < args.Length && int.TryParse(args[++i], out var parsedPort))
            {
                serverPort = parsedPort;
            }
            else if (args[i] == "--wait-ms" && i + 1 < args.Length && int.TryParse(args[++i], out var parsedWait))
            {
                waitMs = parsedWait;
            }
        }

        using var tcpServer = new TcpServer(serverPort);
        LogInfo($"TCP Server is started on port {serverPort}");

        using var primary = new PrimaryClientInterface();
        if (!primary.connect(ip))
        {
            LogError($"Failed to connect to primary server at {ip}:30001");
            return;
        }
        LogInfo($"Connected to primary server at {ip}:30001");

        if (string.IsNullOrWhiteSpace(localIp))
        {
            localIp = primary.getLocalIP();
        }

        const string robotSocketSendString = "hello";
        var robotScript = "def socket_test():\n";
        robotScript += $"\tsocket_open(\"{localIp}\", {serverPort})\n";
        robotScript += $"\tsocket_send_string(\"{robotSocketSendString}\\n\")\n";
        robotScript += "end\n";

        if (!primary.sendScript(robotScript))
        {
            LogError("Failed to send robot script");
            return;
        }
        LogInfo("Robot script sent successfully");

        Thread.Sleep(waitMs);

        var receivedData = tcpServer.GetReceivedData();
        if (receivedData is not null)
        {
            LogInfo($"Received data from robot: {receivedData}");
            if (receivedData.Trim() == robotSocketSendString)
            {
                LogInfo("Success, robot connected to PC");
            }
            else
            {
                LogError("Fail, robot connected to PC but not send right string");
            }
        }
        else
        {
            LogError("No data received from robot");
        }

        primary.disconnect();
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- connect_robot_test <robot-ip> [--local-ip <ip>] [--server-port <port>] [--wait-ms <ms>]");
    }

    private static void LogInfo(string msg)
    {
        Console.WriteLine(msg);
        EliteLog.logInfoMessage("connect_robot_test.cs", 0, msg);
    }

    private static void LogError(string msg)
    {
        Console.WriteLine(msg);
        EliteLog.logErrorMessage("connect_robot_test.cs", 0, msg);
    }

    private sealed class TcpServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private readonly Thread _serverThread;
        private readonly object _lock = new();
        private string? _receivedData;

        public TcpServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _serverThread = new Thread(ServerLoop) { IsBackground = true };
            _serverThread.Start();
        }

        public string? GetReceivedData()
        {
            lock (_lock)
            {
                if (_receivedData is null)
                {
                    return null;
                }

                var value = _receivedData;
                _receivedData = null;
                return value;
            }
        }

        private void ServerLoop()
        {
            try
            {
                using var client = _listener.AcceptTcpClient();
                using var stream = client.GetStream();
                var buffer = new byte[1024];

                while (!_cts.IsCancellationRequested)
                {
                    if (!stream.DataAvailable)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    var n = stream.Read(buffer, 0, buffer.Length);
                    if (n <= 0)
                    {
                        break;
                    }

                    var text = System.Text.Encoding.UTF8.GetString(buffer, 0, n);
                    lock (_lock)
                    {
                        _receivedData = text;
                    }
                }
            }
            catch (SocketException)
            {
                // Listener stopped.
            }
            catch (ObjectDisposedException)
            {
                // Listener disposed.
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _listener.Stop();
        }
    }
}
