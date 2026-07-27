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

    public AppClient(string userName, int? port = DefaultPort)
    {
        var localEndpoint = IpUtils.GetLocalEndpoint(port ?? DefaultPort);

        _localEndpoint = localEndpoint ?? throw new ApplicationException("Couldn't get local IP address");
        _userName = userName;
    }

    private IPEndPoint BroadcastEndpoint => new(IPAddress.Broadcast, _localEndpoint.Port);

    private static (byte[], IPEndPoint)? ReceiveBytes(UdpClient udpClient)
    {
        try
        {
            var from = new IPEndPoint(0, 0);
            var receivedBytes = udpClient.Receive(ref from);
            return (receivedBytes, from);
        }
        catch (SocketException ex)
        {
            // blocking is fine
            if (ex.SocketErrorCode == SocketError.WouldBlock)
            {
                return null;
            }

            throw;
        }
    }

    public void DetectAvailableDevices()
    {
        var udpClient = new UdpClient();
        udpClient.Client.Bind(_localEndpoint);

        SendBroadcast(udpClient, IdentMessage);
        
        udpClient.Client.Blocking = false;
        
        var startTime = DateTime.Now;

        while (DateTime.Now < startTime + TimeSpan.FromSeconds(1))
        {
            var ret = ReceiveBytes(udpClient);
            if (ret == null)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                continue;
            }

            var (receivedBytes, from) = ret.Value;
            var receivedString = Encoding.UTF8.GetString(receivedBytes);
            
            var isLoopback = from.Equals(_localEndpoint) && receivedString == IdentMessage;
            if (isLoopback)
            {
                continue;
            }
            
            Console.WriteLine($"received: '{receivedString}' from {from}");
            
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }
    }

    public Task WaitAndRespond()
    {
        var udpClient = new UdpClient();
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, _localEndpoint.Port));

        var waitAndRespondTask = Task.Run(() =>
        {
            while (true)
            {
                var messageSource = new IPEndPoint(0, 0);
                var receivedBytes = udpClient.Receive(ref messageSource);
                var receivedString = Encoding.UTF8.GetString(receivedBytes);

                Console.WriteLine(
                    receivedString == IdentMessage
                        ? $"received ident from: {messageSource}"
                        : $"received other message from: {messageSource}");

                SendMessage(udpClient, messageSource, $"{_localEndpoint} {_userName}");
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
