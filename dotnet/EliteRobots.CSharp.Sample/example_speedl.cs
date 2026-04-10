using EliteRobots.CSharp;

internal static class SpeedlExample
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
        var scriptFile = "source/resources/external_control.script";

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
            else if (args[i] == "--script-file" && i + 1 < args.Length)
            {
                scriptFile = args[++i];
            }
        }

        try
        {
            using var dashboard = new DashboardClientInterface();
            Console.WriteLine("[INFO] Connecting dashboard...");
            if (!dashboard.connect(ip))
            {
                Console.WriteLine("[FATAL] Failed to connect dashboard.");
                return;
            }
            Console.WriteLine("[INFO] Dashboard connected.");

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

            var config = new EliteDriverConfig    //c#命名风格，参数采用小驼峰
            {
                RobotIp = ip,
                LocalIp = localIp,
                ScriptFilePath = scriptFile,
                HeadlessMode = headless,
                ServojTime = 0.004f,
                ServojGain = 2000,
                ServojLookaheadTime = 0.3f,
            };

            using var driver = new EliteDriver(config);
            Thread.Sleep(1000);

            if (config.HeadlessMode)
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
                    Console.WriteLine("[FATAL] Failed to play program.");
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
            Thread.Sleep(1000);

            var speedDown = new[] { 0.0, 0.0, -0.02, 0.0, 0.0, 0.0 };
            Console.WriteLine("[INFO] writeSpeedl down...");
            if (!driver.writeSpeedl(speedDown, -1))
            {
                Console.WriteLine("[FATAL] writeSpeedl(down) failed.");
                return;
            }
            Thread.Sleep(5000);

            var speedUp = new[] { 0.0, 0.0, 0.02, 0.0, 0.0, 0.0 };
            Console.WriteLine("[INFO] writeSpeedl up...");
            if (!driver.writeSpeedl(speedUp, -1))
            {
                Console.WriteLine("[FATAL] writeSpeedl(up) failed.");
                return;
            }
            Thread.Sleep(5000);

            Console.WriteLine($"[INFO] stopControl: {driver.stopControl()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- speedl <robot-ip> [--local-ip <ip>] [--headless <true|false>] [--script-file <path>]");
    }
}
