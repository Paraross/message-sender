using System.Net;
using System.Net.Sockets;
using System.Text;

var mode = args[0];

const int port = 6969;

if (mode == "send")
{
    var address = args[1];
    var ipAddress = IPAddress.Parse(address);
    var ipEndPoint = new IPEndPoint(ipAddress, port);
    
    using TcpClient client = new();
    
    await client.ConnectAsync(ipEndPoint);
    await using var stream = client.GetStream();
    
    var message = args[2];
    var bytes = Encoding.UTF8.GetBytes(message);
    
    await stream.WriteAsync(bytes);
    
    Console.WriteLine($"Sent {bytes.Length} bytes");
    Console.WriteLine($"Message sent: \"{message}\"");   
}
else if (mode == "listen")
{
    var ipEndPoint = new IPEndPoint(IPAddress.Any, port);
    TcpListener listener = new(ipEndPoint);

    try
    {    
        Console.WriteLine("Listening...");
        listener.Start();

        using var handler = await listener.AcceptTcpClientAsync();
        await using var stream = handler.GetStream();

        var buffer = new byte[1024];

        var read = await stream.ReadAsync(buffer);

        var message = Encoding.UTF8.GetString(buffer);
        
        Console.WriteLine($"Got {read} bytes");
        Console.WriteLine($"Message: \"{message}\"");
    }
    finally
    {
        listener.Stop();
    }  
}
