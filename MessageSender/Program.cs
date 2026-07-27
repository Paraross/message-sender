using MessageSender;

const int defaultPort = 6969;
var port = int.Parse(args.ElementAtOrDefault(1) ?? defaultPort.ToString());

var localEndpoint = IpUtils.GetLocalEndpoint(port);

if (localEndpoint == null)
{
    Console.WriteLine("Couldn't get local IP address, aborting");
    return;
}

Console.WriteLine($"Current Endpoint: {localEndpoint}");

if (args.Length == 0)
{
    Console.WriteLine("'wait' or 'detect'");
    return;
}

var mode = args[0];

var appClient = new AppClient("Unknown");

if (mode == "wait")
{
    await appClient.WaitAndRespond();
}
else if (mode == "send")
{
    appClient.SendOne();
}
else if (mode == "detect")
{
    await appClient.DetectAvailableDevices();
}
else
{
    Console.WriteLine("Incorrect command");
}
