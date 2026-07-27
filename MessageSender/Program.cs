using System.Net;
using System.Net.Sockets;
using System.Text;

const int defaultPort = 6969;
var port = int.Parse(args.ElementAtOrDefault(1) ?? defaultPort.ToString());

var localIpAddress = GetLocalIpAddress();

if (localIpAddress == null)
{
    Console.WriteLine("Couldn't get local IP address, aborting");
    return;
}

Console.WriteLine($"Current IP address: {localIpAddress}");
Console.WriteLine($"Endpoint: {new IPEndPoint(localIpAddress, port)}");

if (args.Length == 0)
{
    Console.WriteLine("'wait' or 'detect'");
    return;
}

var mode = args[0];

if (mode == "wait")
{
    await WaitAndRespond();
}
else if (mode == "send")
{
    SendOne();
}
else if (mode == "detect")
{
    await DetectAvailableDevices();
}
else
{
    Console.WriteLine("Incorrect command");
}

return;

IPAddress? GetLocalIpAddress()
{
    using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
    socket.Connect("8.8.8.8", 65530);
    var endPoint = socket.LocalEndPoint as IPEndPoint;
    var localIpAddress = endPoint?.Address;

    return localIpAddress;
}

void SendOne()
{
    var udpClient = new UdpClient();
    udpClient.Client.Bind(new IPEndPoint(localIpAddress, port));

    const string message = "ident";
    var messageBytes = Encoding.UTF8.GetBytes(message);

    var broadcastAddress = new IPEndPoint(IPAddress.Broadcast, port);
    udpClient.Send(messageBytes, messageBytes.Length, broadcastAddress);
}

Task DetectAvailableDevices()
{
    var udpClient = new UdpClient();
    udpClient.Client.Bind(new IPEndPoint(localIpAddress, port));

    const string message = "ident";
    var messageBytes = Encoding.UTF8.GetBytes(message);

    var receiveResponseTask = Task.Run(() =>
    {
        while (true)
        {
            // send broadcast
            var broadcastAddress = new IPEndPoint(IPAddress.Broadcast, port);
            udpClient.Send(messageBytes, messageBytes.Length, broadcastAddress);

            var from = new IPEndPoint(0, 0);

            // receive response
            var receivedBytes = udpClient.Receive(ref from);
            var receivedString = Encoding.UTF8.GetString(receivedBytes);

            if (!from.Equals(new IPEndPoint(localIpAddress, port)))
            {
                Console.WriteLine($"received: '{receivedString}' from {from}");
            }

            Thread.Sleep(500);
        }
    });

    return receiveResponseTask;
}

Task WaitAndRespond()
{
    var udpClient = new UdpClient();
    udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));

    var waitAndRespondTask = Task.Run(() =>
    {
        while (true)
        {
            var from = new IPEndPoint(0, 0);
            var receivedBytes = udpClient.Receive(ref from);
            var receivedString = Encoding.UTF8.GetString(receivedBytes);

            Console.WriteLine(
                receivedString == "ident" ? $"received ident from: {from}" : $"received other message from: {from}");

            const string response = "IP";
            var responseBytes = Encoding.UTF8.GetBytes(response);
            udpClient.Send(responseBytes, responseBytes.Length, from);
        }
    });

    return waitAndRespondTask;
}
