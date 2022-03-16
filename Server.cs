using System.Net;
using System.Net.Sockets;

namespace SimpleWebServer;

public class Server : IDisposable
{
    private readonly TcpListener _listener;
    private readonly int _port;

    public Server(int port)
    {
        _port = port;
        _listener = new TcpListener(IPAddress.Loopback, _port);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _listener.Start();
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var tcpClient = await _listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                _ = ProcessClient(tcpClient, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Cancel!!!");
            }
        }

    }

    public async Task ProcessClient(TcpClient tcpClient, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("new connection {0}", Thread.CurrentThread.ManagedThreadId);
        var client = new Client(tcpClient);
        await client.ProcessAsync(cancellationToken).ConfigureAwait(false);
        Console.WriteLine("close connection {0}", Thread.CurrentThread.ManagedThreadId);
    }

    public void Dispose()
    {
        _listener?.Stop();
    }
}
