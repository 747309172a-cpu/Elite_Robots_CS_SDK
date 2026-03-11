using EliteRobots.CSharp;

internal static class PrimaryExample
{
    internal static void Run(string[] args)
    {
        var ip = args[1];
        var port = (args.Length >= 3 && int.TryParse(args[2], out var parsedPort)) ? parsedPort : 30001;

        using var primary = new ElitePrimaryClient();
        Console.WriteLine($"connect: {primary.connect(ip, port)}");
        Console.WriteLine($"getLocalIP: {primary.getLocalIP()}");

        var got = primary.getPackage(out var kin, 1000);
        Console.WriteLine($"getPackage: {got}");
        if (got)
        {
            Console.WriteLine($"DH a: [{string.Join(", ", kin.DhA)}]");
        }

        primary.registerRobotExceptionCallback(ex =>
        {
            Console.WriteLine($"RobotException: type={ex.Type}, ts={ex.Timestamp}, msg={ex.Message}");
        });

        Console.WriteLine($"sendScript: {primary.sendScript("popup(\"hello\")\n")}");
        primary.clearRobotExceptionCallback();
        primary.disconnect();
    }
}
