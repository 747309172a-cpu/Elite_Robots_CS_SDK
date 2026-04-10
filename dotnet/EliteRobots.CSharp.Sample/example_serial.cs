using System.Text;
using EliteRobots.CSharp;

internal static class SerialExample
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
        var headless = false;
        var sshPassword = string.Empty;
        var scriptFile = "source/resources/external_control.script";
        var tcpPort = 54321;

        for (var i = 2; i < args.Length; i++)
        {
            if (args[i] == "--local-ip" && i + 1 < args.Length)
            {
                localIp = args[++i];
            }
            else if (args[i] == "--headless" && i + 1 < args.Length)
            {
                if (!bool.TryParse(args[++i], out headless))
                {
                    Console.WriteLine("Invalid --headless value, use true or false.");
                    return;
                }
            }
            else if (args[i] == "--ssh-password" && i + 1 < args.Length)
            {
                sshPassword = args[++i];
            }
            else if (args[i] == "--script-file" && i + 1 < args.Length)
            {
                scriptFile = args[++i];
            }
            else if (args[i] == "--tcp-port" && i + 1 < args.Length)
            {
                if (!int.TryParse(args[++i], out tcpPort))
                {
                    Console.WriteLine("Invalid --tcp-port value.");
                    return;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(sshPassword))
        {
            Console.WriteLine("--ssh-password is required.");
            PrintUsage();
            return;
        }

        try
        {
            using var dashboard = new DashboardClientInterface();
            Console.WriteLine("[INFO] Connecting to dashboard...");
            if (!dashboard.connect(ip))
            {
                Console.WriteLine("[FATAL] Failed to connect to dashboard.");
                return;
            }

            Console.WriteLine("[INFO] Powering on...");
            if (!dashboard.powerOn())
            {
                Console.WriteLine("[FATAL] Power-on failed.");
                return;
            }

            Console.WriteLine("[INFO] Releasing brake...");
            if (!dashboard.brakeRelease())
            {
                Console.WriteLine("[FATAL] Brake release failed.");
                return;
            }

            var config = new EliteDriverConfig
            {
                RobotIp = ip,
                LocalIp = localIp,
                ScriptFilePath = scriptFile,
                HeadlessMode = headless,
            };

            using var driver = new EliteDriver(config);
            Thread.Sleep(1000);

            if (headless)
            {
                if (!driver.isRobotConnected() && !driver.sendExternalControlScript())
                {
                    Console.WriteLine("[FATAL] Failed to send external control script.");
                    return;
                }
            }
            else
            {
                if (!dashboard.playProgram())
                {
                    Console.WriteLine("[FATAL] Failed to play program from dashboard.");
                    return;
                }
            }

            Console.WriteLine("[INFO] Waiting for robot connection...");
            var deadline = DateTime.UtcNow.AddSeconds(15);
            while (!driver.isRobotConnected())
            {
                if (DateTime.UtcNow > deadline)
                {
                    Console.WriteLine("[FATAL] Robot did not connect in time.");
                    return;
                }
                Thread.Sleep(10);
            }
            Console.WriteLine("[INFO] External control script is running.");

            var serialConfig = new SerialConfig
            {
                baud_rate = SerialConfigBaudRate.BR_115200,
                parity = SerialConfigParity.NONE,
                stop_bits = SerialConfigStopBits.ONE,
            };

            using var serial = driver.startToolRs485(serialConfig, sshPassword, tcpPort);
            if (serial is null)
            {
                Console.WriteLine("[FATAL] startToolRs485 returned null.");
                return;
            }

            Console.WriteLine("[INFO] Connecting serial...");
            if (!serial.connect(1000))
            {
                Console.WriteLine("[FATAL] Failed to connect serial.");
                return;
            }

            var hello = Encoding.UTF8.GetBytes("hello world");
            Console.WriteLine("[INFO] Writing serial...");
            var written = serial.write(hello);
            if (written <= 0)
            {
                Console.WriteLine("[FATAL] Failed to write serial.");
                return;
            }
            Console.WriteLine($"[INFO] Wrote {written} bytes.");

            Console.WriteLine("[INFO] Reading serial...");
            var read = serial.read(64, 5000);
            Console.WriteLine($"[INFO] Read {read.Length} bytes: {Encoding.UTF8.GetString(read)}");

            Console.WriteLine("[INFO] Ending tool RS485...");
            var endOk = driver.endToolRs485(serial, sshPassword);
            Console.WriteLine($"[INFO] endToolRs485: {endOk}");

            var stopOk = driver.stopControl();
            Console.WriteLine($"[INFO] stopControl: {stopOk}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- serial <robot-ip> --ssh-password <pwd> [--local-ip <ip>] [--headless <true|false>] [--script-file <path>] [--tcp-port <port>]");
    }
}
