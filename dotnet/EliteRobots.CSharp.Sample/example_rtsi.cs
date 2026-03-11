using EliteRobots.CSharp;

internal static class RtsiExample
{
    internal static void Run(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run -- rtsi <robot-ip> [port]");
            return;
        }

        var ip = args[1];
        var port = (args.Length >= 3 && int.TryParse(args[2], out var parsedPort)) ? parsedPort : 30004;

        using var rtsi = new EliteRtsiClient();

        RunStep("connect", () =>
        {
            rtsi.connect(ip, port);
            return "ok";
        });
        RunStep("isConnected", rtsi.isConnected);
        RunStep("negotiateProtocolVersion", () => rtsi.negotiateProtocolVersion(1));
        RunStep("getControllerVersion", () => rtsi.getControllerVersion().ToString());

        using var outputRecipe = RunStep("setupOutputRecipe", () =>
            rtsi.setupOutputRecipe(new[] { "timestamp", "actual_joint_positions", "actual_TCP_pose" }, 50));

        using var inputRecipe = RunStep("setupInputRecipe", () =>
            rtsi.setupInputRecipe(new[] { "speed_slider_fraction", "speed_slider_mask" }));

        if (outputRecipe is null || inputRecipe is null)
        {
            RunStep("disconnect", () =>
            {
                rtsi.disconnect();
                return "ok";
            });
            return;
        }

        RunStep("outputRecipe.getID", outputRecipe.getID);
        RunStep("inputRecipe.getID", inputRecipe.getID);

        RunStep("inputRecipe.setValue(speed_slider_mask)", () => inputRecipe.setValue("speed_slider_mask", 1U));
        RunStep("inputRecipe.setValue(speed_slider_fraction)", () => inputRecipe.setValue("speed_slider_fraction", 0.5));
        RunStep("send(inputRecipe)", () =>
        {
            rtsi.send(inputRecipe);
            return "ok";
        });

        RunStep("start", rtsi.start);
        RunStep("isStarted", rtsi.isStarted);
        RunStep("receiveData(outputRecipe)", () => rtsi.receiveData(outputRecipe, true));
        RunStep("isReadAvailable", rtsi.isReadAvailable);

        RunStep("outputRecipe.getValue(timestamp)", () =>
        {
            if (!outputRecipe.getValue("timestamp", out double timestamp))
            {
                return "not found";
            }
            return timestamp.ToString("F6");
        });

        RunStep("outputRecipe.getValue(actual_joint_positions)", () =>
        {
            var q = new double[6];
            if (!outputRecipe.getValue("actual_joint_positions", q))
            {
                return "not found";
            }
            return $"[{string.Join(", ", q)}]";
        });

        RunStep("pause", rtsi.pause);
        RunStep("disconnect", () =>
        {
            rtsi.disconnect();
            return "ok";
        });
    }

    private static T? RunStep<T>(string name, Func<T> call)
    {
        try
        {
            var value = call();
            Console.WriteLine($"{name}: {value}");
            return value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{name}: FAILED - {ex.Message}");
            return default;
        }
    }
}
