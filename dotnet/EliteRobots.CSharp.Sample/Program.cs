if (args.Length < 2)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run -- primary <robot-ip> [port]");
    Console.WriteLine("  dotnet run -- primary_client <robot-ip> [--port <primary-port>]");
    Console.WriteLine("  dotnet run -- dashboard <robot-ip> [port] [--config <path>] [--task <path>] [--with-disruptive]");
    Console.WriteLine("  dotnet run -- dashboard_client <robot-ip> [--port <dashboard-port>]");
    Console.WriteLine("  dotnet run -- driver <robot-ip> <script-file-path> [--local-ip <ip>] [--headless] [--ssh-password <pwd>] [--with-rs485]");
    Console.WriteLine("  dotnet run -- rtsi <robot-ip> [port]");
    Console.WriteLine("  dotnet run -- rtsi_client <robot-ip> [--port <rtsi-port>]");
    Console.WriteLine("  dotnet run -- rtsi_io <robot-ip>");
    Console.WriteLine("  dotnet run -- elite_utils <robot-ip> --password <ssh-password> [--log-path <save-path>] [--with-upgrade <upgrade-file>]");
    Console.WriteLine("  dotnet run -- connect_robot_test <robot-ip> [--local-ip <ip>] [--server-port <port>] [--wait-ms <ms>]");
    return;
}

if (string.Equals(args[0], "primary", StringComparison.OrdinalIgnoreCase))
{
    PrimaryExample.Run(args);
    return;
}

if (string.Equals(args[0], "primary_client", StringComparison.OrdinalIgnoreCase))
{
    PrimaryClientFlowExample.Run(args);
    return;
}

if (string.Equals(args[0], "dashboard", StringComparison.OrdinalIgnoreCase))
{
    DashboardExample.Run(args);
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

if (string.Equals(args[0], "rtsi", StringComparison.OrdinalIgnoreCase))
{
    RtsiExample.Run(args);
    return;
}

if (string.Equals(args[0], "rtsi_client", StringComparison.OrdinalIgnoreCase))
{
    RtsiClientFlowExample.Run(args);
    return;
}

if (string.Equals(args[0], "rtsi_io", StringComparison.OrdinalIgnoreCase))
{
    RtsiIOExample.Run(args);
    return;
}

if (string.Equals(args[0], "elite_utils", StringComparison.OrdinalIgnoreCase))
{
    EliteUtilsExample.Run(args);
    return;
}

if (string.Equals(args[0], "connect_robot_test", StringComparison.OrdinalIgnoreCase))
{
    ConnectRobotTestExample.Run(args);
    return;
}

Console.WriteLine($"Unknown mode: {args[0]}");
