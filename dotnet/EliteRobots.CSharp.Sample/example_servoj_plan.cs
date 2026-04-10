using EliteRobots.CSharp;

internal static class ServojPlanExample
{
    private sealed class TrapezoidalPoint
    {
        public TrapezoidalPoint(double t, double pos, double vel, double acc)
        {
            this.t = t;
            this.pos = pos;
            this.vel = vel;
            this.acc = acc;
        }

        public double t { get; }
        public double pos { get; }
        public double vel { get; }
        public double acc { get; }
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
        var maxSpeed = 2.0;
        var maxAcc = 2.0;
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
            else if (args[i] == "--max-speed" && i + 1 < args.Length)
            {
                if (!double.TryParse(args[++i], out maxSpeed))
                {
                    Console.WriteLine("Invalid --max-speed value.");
                    return;
                }
            }
            else if (args[i] == "--max-acc" && i + 1 < args.Length)
            {
                if (!double.TryParse(args[++i], out maxAcc))
                {
                    Console.WriteLine("Invalid --max-acc value.");
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
                Console.WriteLine($"[FATAL] Cannot connect RTSI server: {ip}");
                return;
            }
            Console.WriteLine("[INFO] RTSI connected.");

            using var dashboard = new DashboardClientInterface();
            Console.WriteLine("[INFO] Connecting dashboard...");
            if (!dashboard.connect(ip))
            {
                Console.WriteLine("[FATAL] Failed to connect dashboard.");
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

            Console.WriteLine("[INFO] Waiting for robot connecting...");
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

            var positiveRotation = false;
            var negativeRotation = false;
            var targetJoint = rtsi.getActualJointPositions();
            const double JointFinalTarget = 3.0;

            while (!(positiveRotation && negativeRotation))
            {
                List<TrapezoidalPoint> plan;
                if (!positiveRotation)
                {
                    plan = TrapezoidalSpeedPlan(targetJoint[5], JointFinalTarget, maxSpeed, maxAcc, config.ServojTime);
                    positiveRotation = true;
                }
                else
                {
                    plan = TrapezoidalSpeedPlan(targetJoint[5], -JointFinalTarget, maxSpeed, maxAcc, config.ServojTime);
                    negativeRotation = true;
                }

                foreach (var point in plan)
                {
                    targetJoint[5] = point.pos;
                    if (!driver.writeServoj(targetJoint, 100, false))
                    {
                        Console.WriteLine("[FATAL] writeServoj failed.");
                        return;
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(config.ServojTime));
                }
            }

            Console.WriteLine($"[INFO] stopControl: {driver.stopControl()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
        }
    }

    private static List<TrapezoidalPoint> TrapezoidalSpeedPlan(double start, double end, double vmax, double amax, double dt)
    {
        var trajectory = new List<TrapezoidalPoint>();
        if (dt <= 0 || vmax <= 0 || amax <= 0)
        {
            return trajectory;
        }

        var dq = end - start;
        var dir = dq >= 0 ? 1.0 : -1.0;
        dq = Math.Abs(dq);

        var tAcc = vmax / amax;
        var dAcc = 0.5 * amax * tAcc * tAcc;

        double tFlat;
        if (dq < 2 * dAcc)
        {
            tAcc = Math.Sqrt(dq / amax);
            dAcc = 0.5 * amax * tAcc * tAcc;
            tFlat = 0.0;
        }
        else
        {
            tFlat = (dq - 2 * dAcc) / vmax;
        }

        var totalTime = 2 * tAcc + tFlat;
        for (var t = 0.0; t < totalTime; t += dt)
        {
            double acc;
            double vel;
            double pos;

            if (t < tAcc)
            {
                acc = dir * amax;
                vel = acc * t;
                pos = start + dir * (0.5 * amax * t * t);
            }
            else if (t < tAcc + tFlat)
            {
                acc = 0.0;
                vel = dir * vmax;
                pos = start + dir * (dAcc + vmax * (t - tAcc));
            }
            else
            {
                var td = t - (tAcc + tFlat);
                acc = -dir * amax;
                vel = dir * vmax - amax * td * dir;
                pos = end - dir * (0.5 * amax * (totalTime - t) * (totalTime - t));
            }

            trajectory.Add(new TrapezoidalPoint(t, pos, vel, acc));
        }

        return trajectory;
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
        Console.WriteLine("  dotnet run -- servoj_plan <robot-ip> [--local-ip <ip>] [--headless <true|false>] [--max-speed <rad/s>] [--max-acc <rad/s^2>] [--script-file <path>]");
    }
}
