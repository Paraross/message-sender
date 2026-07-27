using System.Net;
using System.Net.Sockets;

namespace MessageSender;

public static class IpUtils
{
    public static IPAddress? GetLocalIpAddress()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect("8.8.8.8", 65530);
        var endPoint = socket.LocalEndPoint as IPEndPoint;
        var localIpAddress = endPoint?.Address;

        return localIpAddress;
    }

    public static IPEndPoint? GetLocalEndpoint(int port)
    {
        var ipAddress = GetLocalIpAddress();
        return ipAddress != null ? new IPEndPoint(ipAddress, port) : null;
    }
}
