if (args.Length < 2)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run -- primary_client <robot-ip> [--port <primary-port>]");
    Console.WriteLine("  dotnet run -- dashboard_client <robot-ip> [--port <dashboard-port>]");
    Console.WriteLine("  dotnet run -- driver <robot-ip> <script-file-path> [--local-ip <ip>] [--headless] [--ssh-password <pwd>] [--with-rs485]");
    Console.WriteLine("  dotnet run -- speedl <robot-ip> [--local-ip <ip>] [--headless <true|false>] [--script-file <path>]");
    Console.WriteLine("  dotnet run -- trajectory <robot-ip> [--local-ip <ip>] [--headless <true|false>] [--script-file <path>]");
    Console.WriteLine("  dotnet run -- servoj_plan <robot-ip> [--local-ip <ip>] [--headless <true|false>] [--max-speed <rad/s>] [--max-acc <rad/s^2>] [--script-file <path>]");
    Console.WriteLine("  dotnet run -- rtsi_client <robot-ip> [--port <rtsi-port>]");
    Console.WriteLine("  dotnet run -- serial <robot-ip> --ssh-password <pwd> [--local-ip <ip>] [--headless <true|false>] [--script-file <path>] [--tcp-port <port>]");
    Console.WriteLine("  dotnet run -- connect_robot_test <robot-ip> [--local-ip <ip>] [--server-port <port>] [--wait-ms <ms>]");
    return;
}

if (string.Equals(args[0], "primary_client", StringComparison.OrdinalIgnoreCase))
{
    PrimaryClientFlowExample.Run(args);
    return;
}

if (string.Equals(args[0], "dashboard_client", StringComparison.OrdinalIgnoreCase))
{
    DashboardClientFlowExample.Run(args);
    return;
}

if (string.Equals(args[0], "driver", StringComparison.OrdinalIgnoreCase))
{
    DriverExample.Run(args);
    return;
}

if (string.Equals(args[0], "speedl", StringComparison.OrdinalIgnoreCase))
{
    SpeedlExample.Run(args);
    return;
}

if (string.Equals(args[0], "trajectory", StringComparison.OrdinalIgnoreCase))
{
    TrajectoryExample.Run(args);
    return;
}

if (string.Equals(args[0], "servoj_plan", StringComparison.OrdinalIgnoreCase))
{
    ServojPlanExample.Run(args);
    return;
}

if (string.Equals(args[0], "rtsi_client", StringComparison.OrdinalIgnoreCase))
{
    RtsiClientFlowExample.Run(args);
    return;
}

if (string.Equals(args[0], "serial", StringComparison.OrdinalIgnoreCase))
{
    SerialExample.Run(args);
    return;
}

if (string.Equals(args[0], "connect_robot_test", StringComparison.OrdinalIgnoreCase))
{
    ConnectRobotTestExample.Run(args);
    return;
}

Console.WriteLine($"Unknown mode: {args[0]}");
