using MessageSender;

// TODO: actual commandline arg parsing

var mode = args.ElementAtOrDefault(0);
if (mode == null)
{
    Console.WriteLine("'wait' or 'detect'");
    return;
}

var portArg = args.ElementAtOrDefault(1);
int? port = portArg != null ? int.Parse(portArg) : null;

var appClient = new AppClient("Unknown host", port);

if (mode == "wait")
{
    await appClient.WaitAndRespond();
}
else if (mode == "detect")
{
    appClient.DetectAvailableDevices();
}
else
{
    Console.WriteLine("Incorrect command");
}
