using EliteRobots.CSharp;

internal static class EliteUtilsExample
{
    internal static void Run(string[] args)
    {
        if (args.Length < 2)
        {
            PrintUsage();
            return;
        }

        var ip = args[1];
        var password = string.Empty;
        var logPath = "/tmp/elite_system_log.csv";
        var withUpgrade = false;
        var upgradeFile = string.Empty;

        for (var i = 2; i < args.Length; i++)
        {
            if (args[i] == "--password" && i + 1 < args.Length)
            {
                password = args[++i];
            }
            else if (args[i] == "--log-path" && i + 1 < args.Length)
            {
                logPath = args[++i];
            }
            else if (args[i] == "--with-upgrade" && i + 1 < args.Length)
            {
                withUpgrade = true;
                upgradeFile = args[++i];
            }
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("Missing --password");
            PrintUsage();
            return;
        }

        RunStep("VersionInfo(major,minor,bugfix,build).ToString", () =>
        {
            var v = new VersionInfo(2, 14, 5, 1178);
            return v.ToString();
        });

        RunStep("VersionInfo.fromString", () =>
        {
            var a = VersionInfo.fromString("2.14.5.1178");
            var b = new VersionInfo(2, 14, 5, 1178);
            return $"eq={a == b}, ge={a >= b}, le={a <= b}";
        });

        RunStep("registerLogHandler", () =>
        {
            EliteLog.registerLogHandler((file, line, level, log) =>
            {
                Console.WriteLine($"[LOG-HANDLER] {level} {file}:{line} {log}");
            });
            return "ok";
        });

        RunStep("setLogLevel(ELI_DEBUG)", () =>
        {
            EliteLog.setLogLevel(LogLevel.ELI_DEBUG);
            return "ok";
        });

        RunStep("logDebugMessage", () =>
        {
            EliteLog.logDebugMessage("example_elite_utils.cs", 1, "debug test from C#");
            return "ok";
        });
        RunStep("logInfoMessage", () =>
        {
            EliteLog.logInfoMessage("example_elite_utils.cs", 2, "info test from C#");
            return "ok";
        });
        RunStep("logWarnMessage", () =>
        {
            EliteLog.logWarnMessage("example_elite_utils.cs", 3, "warn test from C#");
            return "ok";
        });
        RunStep("logErrorMessage", () =>
        {
            EliteLog.logErrorMessage("example_elite_utils.cs", 4, "error test from C#");
            return "ok";
        });
        RunStep("logFatalMessage", () =>
        {
            EliteLog.logFatalMessage("example_elite_utils.cs", 5, "fatal test from C#");
            return "ok";
        });
        RunStep("logNoneMessage", () =>
        {
            EliteLog.logNoneMessage("example_elite_utils.cs", 6, "none test from C#");
            return "ok";
        });

        RunStep("downloadSystemLog", () =>
            EliteControllerLog.downloadSystemLog(ip, password, logPath, (f, r, err) =>
            {
                if (!string.IsNullOrWhiteSpace(err))
                {
                    Console.WriteLine($"download progress {r}/{f}, err={err}");
                }
            }));

        if (withUpgrade)
        {
            RunStep("upgradeControlSoftware", () => EliteUpgrade.upgradeControlSoftware(ip, upgradeFile, password));
        }
        else
        {
            Console.WriteLine("upgradeControlSoftware: skipped (add --with-upgrade <upgrade-file>)");
        }

        RunStep("unregisterLogHandler", () =>
        {
            EliteLog.unregisterLogHandler();
            return "ok";
        });
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- elite_utils <robot-ip> --password <ssh-password> [--log-path <save-path>] [--with-upgrade <upgrade-file>]");
    }

    private static void RunStep<T>(string name, Func<T> call)
    {
        try
        {
            var value = call();
            Console.WriteLine($"{name}: {value}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{name}: FAILED - {ex.Message}");
        }
    }
}
