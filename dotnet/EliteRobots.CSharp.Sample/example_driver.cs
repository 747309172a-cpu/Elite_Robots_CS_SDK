using EliteRobots.CSharp;

internal static class DriverExample
{
    internal static void Run(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run -- driver <robot-ip> <script-file-path> [--local-ip <ip>] [--headless] [--ssh-password <pwd>] [--with-rs485]");
            return;
        }

        var robotIp = args[1];
        var scriptFilePath = args[2];
        var localIp = string.Empty;
        var headless = false;
        var sshPassword = string.Empty;
        var withRs485 = false;

        for (var i = 3; i < args.Length; i++)
        {
            if (args[i] == "--local-ip" && i + 1 < args.Length)
            {
                localIp = args[++i];
            }
            else if (args[i] == "--ssh-password" && i + 1 < args.Length)
            {
                sshPassword = args[++i];
            }
            else if (args[i] == "--headless")
            {
                headless = true;
            }
            else if (args[i] == "--with-rs485")
            {
                withRs485 = true;
            }
        }

        var config = new EliteDriverConfig
        {
            RobotIp = robotIp,
            ScriptFilePath = scriptFilePath,
            LocalIp = localIp,
            HeadlessMode = headless,
        };

        using var driver = new EliteDriver(config);
        Console.WriteLine($"headlessMode: {headless}");

        RunStep("isRobotConnected", driver.isRobotConnected);
        RunStep("sendExternalControlScript", driver.sendExternalControlScript);

        driver.setTrajectoryResultCallback(result =>
        {
            Console.WriteLine($"trajectoryResultCallback: {result}");
        });
        Console.WriteLine("setTrajectoryResultCallback: registered");

        driver.registerRobotExceptionCallback(ex =>
        {
            Console.WriteLine($"robotExceptionCallback: type={ex.Type}, code={ex.ErrorCode}, msg={ex.Message}");
        });
        Console.WriteLine("registerRobotExceptionCallback: registered");

        var zeros6 = new double[6];
        var zeros3 = new double[3];
        var select0 = new int[6];

        RunStep("writeServoj", () => driver.writeServoj(zeros6, 100, false));
        RunStep("writeSpeedl", () => driver.writeSpeedl(zeros6, 100));
        RunStep("writeSpeedj", () => driver.writeSpeedj(zeros6, 100));
        RunStep("writeTrajectoryPoint", () => driver.writeTrajectoryPoint(zeros6, 0.1f, 0.0f, false));
        RunStep("writeTrajectoryControlAction", () => driver.writeTrajectoryControlAction(TrajectoryControlAction.NOOP, 0, 100));
        RunStep("writeIdle", () => driver.writeIdle(100));
        RunStep("writeFreedrive", () => driver.writeFreedrive(FreedriveAction.FREEDRIVE_NOOP, 100));
        RunStep("zeroFTSensor", driver.zeroFTSensor);
        RunStep("setPayload", () => driver.setPayload(0.5, zeros3));
        RunStep("setToolVoltage", () => driver.setToolVoltage(ToolVoltage.OFF));
        RunStep("startForceMode", () => driver.startForceMode(zeros6, select0, zeros6, ForceMode.FIX, zeros6));
        RunStep("endForceMode", driver.endForceMode);
        RunStep("sendScript", () => driver.sendScript("popup(\"hello from csharp driver\")\n"));

        RunStep("getPrimaryPackage", () =>
        {
            var pkg = new PrimaryKinematicsInfo();
            var ok = driver.getPrimaryPackage(pkg, 1000);
            if (ok)
            {
                return $"true, dhA=[{string.Join(", ", pkg.DhA)}]";
            }
            return "false";
        });

        RunStep("primaryReconnect", driver.primaryReconnect);

        if (withRs485)
        {
            var serialConfig = new SerialConfig
            {
                baud_rate = SerialConfigBaudRate.BR_115200,
                parity = SerialConfigParity.NONE,
                stop_bits = SerialConfigStopBits.ONE,
            };

            EliteSerialCommunication? toolComm = null;
            EliteSerialCommunication? boardComm = null;

            RunStep("startToolRs485", () =>
            {
                toolComm = driver.startToolRs485(serialConfig, sshPassword, 54321);
                return toolComm is not null;
            });
            if (toolComm is not null)
            {
                RunStep("endToolRs485", () => driver.endToolRs485(toolComm, sshPassword));
            }

            RunStep("startBoardRs485", () =>
            {
                boardComm = driver.startBoardRs485(serialConfig, sshPassword, 54322);
                return boardComm is not null;
            });
            if (boardComm is not null)
            {
                RunStep("endBoardRs485", () => driver.endBoardRs485(boardComm, sshPassword));
            }
        }
        else
        {
            Console.WriteLine("RS485 steps skipped (add --with-rs485 to run).");
        }

        RunStep("stopControl", () => driver.stopControl(10000));
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
