using EliteRobots.CSharp;

internal static class TrajectoryExample
{
    private sealed class TrajectoryControl : IDisposable
    {
        private readonly EliteDriverConfig _config;
        private readonly DashboardClientInterface _dashboard;
        private readonly EliteDriver _driver;

        public TrajectoryControl(EliteDriverConfig config)
        {
            _config = config;
            _dashboard = new DashboardClientInterface();
            _driver = new EliteDriver(config);

            Console.WriteLine("[INFO] Connecting to dashboard...");
            if (!_dashboard.connect(_config.RobotIp))
            {
                throw new InvalidOperationException("Failed to connect dashboard.");
            }
            Console.WriteLine("[INFO] Dashboard connected.");
        }

        public bool startControl()
        {
            Console.WriteLine("[INFO] Powering on...");
            if (!_dashboard.powerOn())
            {
                Console.WriteLine("[FATAL] Power-on failed.");
                return false;
            }

            Console.WriteLine("[INFO] Releasing brake...");
            if (!_dashboard.brakeRelease())
            {
                Console.WriteLine("[FATAL] Brake release failed.");
                return false;
            }

            if (_config.HeadlessMode)
            {
                if (!_driver.isRobotConnected() && !_driver.sendExternalControlScript())
                {
                    Console.WriteLine("[FATAL] Failed to send external control script.");
                    return false;
                }
            }
            else
            {
                if (!_dashboard.playProgram())
                {
                    Console.WriteLine("[FATAL] Failed to play program.");
                    return false;
                }
            }

            Console.WriteLine("[INFO] Waiting external control script run...");
            while (!_driver.isRobotConnected())
            {
                Thread.Sleep(100);
            }
            Console.WriteLine("[INFO] External control script is running.");
            return true;
        }

        public bool moveTrajectory(IReadOnlyList<double[]> targetPoints, float pointTime, float blendRadius, bool isCartesian)
        {
            var done = new ManualResetEventSlim(false);
            TrajectoryMotionResult motionResult = TrajectoryMotionResult.FAILURE;

            _driver.setTrajectoryResultCallback(result =>
            {
                motionResult = result;
                done.Set();
            });

            Console.WriteLine("[INFO] Trajectory motion start.");
            if (!_driver.writeTrajectoryControlAction(TrajectoryControlAction.START, targetPoints.Count, 200))
            {
                Console.WriteLine("[FATAL] Failed to start trajectory motion.");
                return false;
            }

            foreach (var point in targetPoints)
            {
                if (!_driver.writeTrajectoryPoint(point, pointTime, blendRadius, isCartesian))
                {
                    Console.WriteLine("[FATAL] Failed to write trajectory point.");
                    return false;
                }

                if (!_driver.writeTrajectoryControlAction(TrajectoryControlAction.NOOP, 0, 200))
                {
                    Console.WriteLine("[FATAL] Failed to send NOOP.");
                    return false;
                }
            }

            while (!done.IsSet)
            {
                Thread.Sleep(10);
                if (!_driver.writeTrajectoryControlAction(TrajectoryControlAction.NOOP, 0, 200))
                {
                    Console.WriteLine("[FATAL] Failed to send NOOP.");
                    return false;
                }
            }

            Console.WriteLine($"[INFO] Trajectory motion completed: {motionResult}");
            if (!_driver.writeIdle(0))
            {
                Console.WriteLine("[FATAL] Failed to write idle.");
                return false;
            }
            return motionResult == TrajectoryMotionResult.SUCCESS;
        }

        public bool moveTo(double[] point, float pointTime, bool isCartesian)
        {
            return moveTrajectory(new[] { point }, pointTime, 0.0f, isCartesian);
        }

        public void Dispose()
        {
            try { _dashboard.disconnect(); } catch { }
            try { _driver.stopControl(); } catch { }
            _driver.Dispose();
            _dashboard.Dispose();
        }
    }

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

        var outputRecipePath = ResolveRecipePath("output_recipe.txt");
        var inputRecipePath = ResolveRecipePath("input_recipe.txt");
        if (outputRecipePath is null || inputRecipePath is null)
        {
            Console.WriteLine("[FATAL] Recipe file not found. Expected under dotnet/EliteRobots.CSharp.Sample/resource.");
            return;
        }

        var outputRecipe = LoadRecipe(outputRecipePath);
        var inputRecipe = LoadRecipe(inputRecipePath);
        if (outputRecipe.Count == 0 || inputRecipe.Count == 0)
        {
            Console.WriteLine("[FATAL] Recipe file is empty.");
            return;
        }

        try
        {
            using var rtsi = new RtsiIOInterface(outputRecipe, inputRecipe, 250.0);
            Console.WriteLine("[INFO] Connecting RTSI...");
            if (!rtsi.connect(ip))
            {
                Console.WriteLine($"[FATAL] Can't connect RTSI server: {ip}");
                return;
            }
            Console.WriteLine("[INFO] RTSI connected.");

            var actualJoints = rtsi.getActualJointPositions();
            var targetJoints = (double[])actualJoints.Clone();
            targetJoints[3] = -1.57;

            var config = new EliteDriverConfig
            {
                RobotIp = ip,
                LocalIp = localIp,
                ScriptFilePath = scriptFile,
                HeadlessMode = headless,
            };

            using var controller = new TrajectoryControl(config);
            Console.WriteLine("[INFO] Starting trajectory control...");
            if (!controller.startControl())
            {
                Console.WriteLine("[FATAL] Failed to start trajectory control.");
                return;
            }

            Console.WriteLine($"[INFO] MoveJ target: [{string.Join(", ", targetJoints)}]");
            if (!controller.moveTo(targetJoints, 3.0f, false))
            {
                Console.WriteLine("[FATAL] Failed to move joints.");
                return;
            }

            var targetPose = rtsi.getActualTCPPose();
            var trajectory = new List<double[]>();

            targetPose[2] -= 0.2;
            trajectory.Add((double[])targetPose.Clone());

            targetPose[1] -= 0.2;
            trajectory.Add((double[])targetPose.Clone());

            targetPose[1] += 0.2;
            targetPose[2] += 0.2;
            trajectory.Add((double[])targetPose.Clone());

            Console.WriteLine("[INFO] MoveL trajectory...");
            if (!controller.moveTrajectory(trajectory, 3.0f, 0.0f, true))
            {
                Console.WriteLine("[FATAL] Failed to move trajectory.");
                return;
            }
            Console.WriteLine("[INFO] Trajectory done.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
        }
    }

    private static string? ResolveRecipePath(string fileName)
    {
        var candidates = new[]
        {
            Path.Combine("dotnet", "EliteRobots.CSharp.Sample", "resource", fileName),
            Path.Combine("resource", fileName),
            Path.Combine(AppContext.BaseDirectory, "resource", fileName),
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }
        return null;
    }

    private static List<string> LoadRecipe(string path)
    {
        return File.ReadAllLines(path)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s) && !s.StartsWith("#"))
            .ToList();
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- trajectory <robot-ip> [--local-ip <ip>] [--headless <true|false>] [--script-file <path>]");
    }
}
