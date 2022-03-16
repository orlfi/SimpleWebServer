using System.Net.Sockets;
using System.Text;

namespace SimpleWebServer;

public class Client : IDisposable
{
    private const string contentType = "text/html; charset=UTF-8";
    private const string responseHeaderTempate = "HTTP/1.1 200 OK\nContent-Type: {0} \nContent-Length: {1}\n\n";
    private const string testHtml = "<html><body><h1>Работает!</h1></body></html>";
    private const int bufferSize = 4096;
    private const int maxRequestLength = 4096;

    private readonly TcpClient _client;

    public Client(TcpClient client)
    {
        _client = client;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        using var stream = _client.GetStream();

        var message = await ReadRequestAsync(stream).ConfigureAwait(false);
        Console.WriteLine(message);

        await WriteResponseAsync(stream, testHtml).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    private async Task<string> ReadRequestAsync(NetworkStream stream, CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[bufferSize];
        int bytesRead = 0;
        StringBuilder sb = new StringBuilder();
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
        {
            var text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            sb.Append(text);
            if (text.IndexOf("\r\n\r\n") >= 0 || sb.Length > maxRequestLength)
                break;
        }
        return sb.ToString();
    }

    private async Task WriteResponseAsync(NetworkStream stream, string message, CancellationToken cancellationToken = default)
    {
        string Headers = string.Format(responseHeaderTempate, contentType, message.Length);
        byte[] HeadersBuffer = Encoding.ASCII.GetBytes(Headers);
        await stream.WriteAsync(HeadersBuffer, 0, HeadersBuffer.Length, cancellationToken).ConfigureAwait(false);

        byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(messageBuffer, 0, message.Length, cancellationToken).ConfigureAwait(false);
    }
}
