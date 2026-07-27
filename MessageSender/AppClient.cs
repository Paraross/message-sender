using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MessageSender;

public class AppClient
{
    private const int DefaultPort = 6969;
    private const string IdentMessage = "ident";

    private readonly string _userName;
    private readonly IPEndPoint _localEndpoint;

    public AppClient(string userName, int port = DefaultPort)
    {
        var localEndpoint = IpUtils.GetLocalEndpoint(port);

        _localEndpoint = localEndpoint ?? throw new ApplicationException("Couldn't get local IP address");
        _userName = userName;
    }
    
    private IPEndPoint BroadcastEndpoint => new(IPAddress.Broadcast, _localEndpoint.Port);
    
    public void SendOne()
    {
        var udpClient = new UdpClient();
        udpClient.Client.Bind(_localEndpoint);

        SendBroadcast(udpClient, IdentMessage);
    }
    
    public Task DetectAvailableDevices()
    {
        var udpClient = new UdpClient();
        udpClient.Client.Bind(_localEndpoint);

        var receiveResponseTask = Task.Run(() =>
        {
            while (true)
            {
                SendBroadcast(udpClient, IdentMessage);

                var from = new IPEndPoint(0, 0);

                var receivedBytes = udpClient.Receive(ref from);
                var receivedString = Encoding.UTF8.GetString(receivedBytes);

                var isLoopback = from.Equals(_localEndpoint) && receivedString == IdentMessage;
                if (!isLoopback)
                {
                    Console.WriteLine($"received: '{receivedString}' from {from}");
                }

                Thread.Sleep(500);
            }
        });

        return receiveResponseTask;
    }

    public Task WaitAndRespond()
    {
        var udpClient = new UdpClient();
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, _localEndpoint.Port));

        var waitAndRespondTask = Task.Run(() =>
        {
            while (true)
            {
                var from = new IPEndPoint(0, 0);
                var receivedBytes = udpClient.Receive(ref from);
                var receivedString = Encoding.UTF8.GetString(receivedBytes);

                Console.WriteLine(
                    receivedString == "ident" ? $"received ident from: {from}" : $"received other message from: {from}");

                SendMessage(udpClient, from, $"{_localEndpoint} {_userName}");
            }
        });

        return waitAndRespondTask;
    }

    private static void SendMessage(UdpClient udpClient, IPEndPoint endpoint, string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        udpClient.Send(messageBytes, messageBytes.Length, endpoint);
    }

    private void SendBroadcast(UdpClient udpClient, byte[] messageBytes)
    {
        udpClient.Send(messageBytes, messageBytes.Length, BroadcastEndpoint);
    }
    
    private void SendBroadcast(UdpClient udpClient, string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        SendBroadcast(udpClient, messageBytes);
    }
}
