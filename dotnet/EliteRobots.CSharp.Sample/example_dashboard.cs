using EliteRobots.CSharp;

internal static class DashboardExample
{
    internal static void Run(string[] args)
    {
        var ip = args[1];
        var port = (args.Length >= 3 && int.TryParse(args[2], out var parsedPort)) ? parsedPort : 29999;

        string? configPath = null;
        string? taskPath = null;
        var withDisruptive = false;

        for (var i = 2; i < args.Length; i++)
        {
            if (args[i] == "--config" && i + 1 < args.Length)
            {
                configPath = args[++i];
            }
            else if (args[i] == "--task" && i + 1 < args.Length)
            {
                taskPath = args[++i];
            }
            else if (args[i] == "--with-disruptive")
            {
                withDisruptive = true;
            }
        }

        using var dashboard = new EliteDashboardClient();

        RunStep("connect", () => dashboard.connect(ip, port));
        RunStep("echo", dashboard.echo);
        RunStep("version", dashboard.version);
        RunStep("powerOn", dashboard.powerOn);
        RunStep("robotType", dashboard.robotType);
        RunStep("robotSerialNumber", dashboard.robotSerialNumber);
        RunStep("robotID", dashboard.robotID);
        RunStep("robotMode", () => dashboard.robotMode().ToString());
        RunStep("safetyMode", () => dashboard.safetyMode().ToString());
        RunStep("runningStatus", () => dashboard.runningStatus().ToString());
        RunStep("getTaskStatus", () => dashboard.getTaskStatus().ToString());
        RunStep("taskIsRunning", dashboard.taskIsRunning);
        RunStep("isTaskSaved", dashboard.isTaskSaved);
        RunStep("speedScaling", dashboard.speedScaling);
        RunStep("setSpeedScaling", () =>
        {
            var current = dashboard.speedScaling();
            return dashboard.setSpeedScaling(current);
        });
        RunStep("configurationPath", dashboard.configurationPath);
        RunStep("isConfigurationModify", dashboard.isConfigurationModify);
        RunStep("getTaskPath", dashboard.getTaskPath);
        RunStep("help(powerOn)", () => dashboard.help("powerOn"));
        RunStep("usage(powerOn)", () => dashboard.usage("powerOn"));
        RunStep("sendAndReceive(echo)", () => dashboard.sendAndReceive("echo"));
        RunStep("log", () => dashboard.log("C# dashboard wrapper test"));
        RunStep("popup(show)", () => dashboard.popup("-s", "C# dashboard popup test"));
        RunStep("popup(close)", () => dashboard.popup("-c"));
        RunStep("closeSafetyDialog", dashboard.closeSafetyDialog);
        RunStep("brakeRelease", dashboard.brakeRelease);
        RunStep("unlockProtectiveStop", dashboard.unlockProtectiveStop);
        RunStep("safetySystemRestart", dashboard.safetySystemRestart);
        RunStep("playProgram", dashboard.playProgram);
        RunStep("pauseProgram", dashboard.pauseProgram);
        RunStep("stopProgram", dashboard.stopProgram);
        RunStep("powerOff", dashboard.powerOff);

        if (!string.IsNullOrWhiteSpace(configPath))
        {
            RunStep("loadConfiguration", () => dashboard.loadConfiguration(configPath));
        }
        if (!string.IsNullOrWhiteSpace(taskPath))
        {
            RunStep("loadTask", () => dashboard.loadTask(taskPath));
        }

        if (withDisruptive)
        {
            RunStep("reboot", () =>
            {
                dashboard.reboot();
                return "called";
            });
            RunStep("shutdown", () =>
            {
                dashboard.shutdown();
                return "called";
            });
            RunStep("quit", () =>
            {
                dashboard.quit();
                return "called";
            });
        }

        RunStep("disconnect", () =>
        {
            dashboard.disconnect();
            return "ok";
        });
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
