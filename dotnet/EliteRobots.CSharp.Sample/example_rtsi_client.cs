using EliteRobots.CSharp;

internal static class RtsiClientFlowExample
{
    internal static void Run(string[] args)
    {
        if (!TryParseArgs(args, out var ip, out var port))
        {
            PrintUsage();
            return;
        }

        using var client = new RtsiClientInterface();

        Console.WriteLine($"[INFO] Connecting to RTSI at {ip}:{port}...");
        client.connect(ip, port);

        Console.WriteLine("[INFO] Negotiating protocol version...");
        if (!client.negotiateProtocolVersion())
        {
            Console.Error.WriteLine("[ERROR] Protocol negotiation failed.");
            return;
        }

        var version = client.getControllerVersion();
        Console.WriteLine($"[INFO] Controller version: {version.Major}.{version.Minor}.{version.Bugfix} (build {version.Build})");

        var variables = new[] { "actual_joint_positions", "target_joint_positions", "target_speed_fraction" };
        const double frequency = 125.0;
        Console.WriteLine($"[INFO] Setting up output recipe [{string.Join(", ", variables)}] @ {frequency} Hz...");
        using var outRecipe = client.setupOutputRecipe(variables, frequency);

        Console.WriteLine("[INFO] Starting data stream...");
        if (!client.start())
        {
            Console.Error.WriteLine("[ERROR] Failed to start RTSI stream.");
            return;
        }

        for (var i = 0; i < 2; i++)
        {
            Console.WriteLine($"[INFO] Waiting for sample {i + 1}...");
            var recipes = new[] { outRecipe };
            var count = client.receiveData(recipes, true);
            if (count >= 0 && recipes.Length > 0)
            {
                var sample = recipes[0];
                var names = sample.getRecipe();
                var rid = sample.getID();
                Console.WriteLine($"[INFO] Sample {i + 1} (ID={rid}):");
                foreach (var name in names)
                {
                    Console.WriteLine($"    {name} = {FormatValue(sample, name)}");
                }
            }
            else
            {
                Console.WriteLine($"[WARN] No data received. {count}");
            }
            Thread.Sleep(1000);
        }

        Console.WriteLine("[INFO] Pausing data stream...");
        client.pause();

        var inputVars = new[] { "speed_slider_mask", "speed_slider_fraction" };
        Console.WriteLine($"[INFO] Setting up input recipe [{string.Join(", ", inputVars)}]...");
        using var inRecipe = client.setupInputRecipe(inputVars);
        var inId = inRecipe.getID();
        Console.WriteLine($"[INFO] Input recipe ID: {inId}");

        const double fraction = 0.3;
        Console.WriteLine($"[INFO] Sending speed_slider_fraction = {fraction}");
        inRecipe.setValue("speed_slider_mask", 1U);
        inRecipe.setValue("speed_slider_fraction", fraction);
        client.send(inRecipe);

        Console.WriteLine("[INFO] Resuming data stream for one more sample...");
        client.start();
        var confirmRecipes = new[] { outRecipe };
        client.receiveData(confirmRecipes, true);
        if (confirmRecipes.Length > 0 &&
            confirmRecipes[0].getValue("target_speed_fraction", out double speed))
        {
            Console.WriteLine($"[INFO] Confirmed target_speed_fraction = {speed}");
        }

        Console.WriteLine("[INFO] Pausing data stream...");
        client.pause();

        Console.WriteLine("[INFO] Disconnecting RTSI client...");
        client.disconnect();
    }

    private static string FormatValue(EliteRtsiRecipe recipe, string name)
    {
        var vec6 = new double[6];
        if (TryGet(() => recipe.getValue(name, vec6)))
        {
            return $"[{string.Join(", ", vec6)}]";
        }
        try
        {
            if (recipe.getValue(name, out double d))
            {
                return d.ToString();
            }
        }
        catch (EliteSdkException)
        {
        }

        try
        {
            if (recipe.getValue(name, out uint u))
            {
                return u.ToString();
            }
        }
        catch (EliteSdkException)
        {
        }

        try
        {
            if (recipe.getValue(name, out int i))
            {
                return i.ToString();
            }
        }
        catch (EliteSdkException)
        {
        }

        try
        {
            if (recipe.getValue(name, out bool b))
            {
                return b.ToString();
            }
        }
        catch (EliteSdkException)
        {
        }

        return "<unavailable>";
    }

    private static bool TryGet(Func<bool> getter)
    {
        try
        {
            return getter();
        }
        catch (EliteSdkException)
        {
            return false;
        }
    }

    private static bool TryParseArgs(string[] args, out string ip, out int port)
    {
        ip = string.Empty;
        port = 30004;

        if (args.Length < 2)
        {
            return false;
        }

        if (args[1] == "--ip")
        {
            for (var i = 1; i < args.Length; i++)
            {
                if (args[i] == "--ip" && i + 1 < args.Length)
                {
                    ip = args[++i];
                }
                else if (args[i] == "--port" && i + 1 < args.Length && int.TryParse(args[++i], out var parsedPort))
                {
                    port = parsedPort;
                }
            }
            return !string.IsNullOrWhiteSpace(ip);
        }

        ip = args[1];
        for (var i = 2; i < args.Length; i++)
        {
            if (args[i] == "--port" && i + 1 < args.Length && int.TryParse(args[++i], out var parsedPort))
            {
                port = parsedPort;
            }
            else if (int.TryParse(args[i], out parsedPort))
            {
                port = parsedPort;
            }
        }
        return true;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- rtsi_client <robot-ip> [--port <rtsi-port>]");
        Console.WriteLine("  dotnet run -- rtsi_client --ip <robot-ip> [--port <rtsi-port>]");
    }
}
