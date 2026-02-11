using Fleck;
using Zylance.Core.Lib.Gateway;

namespace Zylance.Desktop.Transports;

public class WebsocketTransport : ITransport, IDisposable
{
    private readonly WebSocketServer _server;
    private IWebSocketConnection? _client;
    private Action<string>? _receiveCallback;

    public WebsocketTransport(int port)
    {
        _server = new WebSocketServer($"ws://localhost:{port}");
        _server.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                if (_client is not null && _client.IsAvailable)
                {
                    socket.Close(); // Only allow one client at a time
                    Console.WriteLine("Second client attempted to connect, rejected.");
                    return;
                }

                _client = socket;
            };
            socket.OnClose = () =>
            {
                if (_client == socket)
                    _client = null;
            };
            socket.OnMessage = message => _receiveCallback?.Invoke(message);
        });
    }

    public void Dispose()
    {
        _client?.Close();
        _server.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Send(string message)
    {
        if (_client is not null && _client.IsAvailable)
            _client.Send(message);
    }

    public void Receive(Action<string> callback)
    {
        _receiveCallback = callback;
    }
}
