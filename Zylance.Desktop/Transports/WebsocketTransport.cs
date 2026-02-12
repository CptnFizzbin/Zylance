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
        _server = new WebSocketServer($"ws://127.0.0.1:{port}");
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
                Console.WriteLine("WebSocket client connected.");
            };
            socket.OnClose = () =>
            {
                if (_client == socket)
                {
                    Console.WriteLine("WebSocket client disconnected.");
                    _client = null;
                }
            };
            socket.OnMessage = message =>
            {
                Console.WriteLine($"WebSocket message received: {message}");
                _receiveCallback?.Invoke(message);
            };
        });
    }

    public void Dispose()
    {
        Console.WriteLine("Disposing WebsocketTransport.");
        _client?.Close();
        _server.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Send(string message)
    {
        if (_client is not null && _client.IsAvailable)
        {
            Console.WriteLine($"Sending WebSocket message: {message}");
            _client.Send(message);
        }
        else
        {
            Console.WriteLine("Attempted to send message, but no WebSocket client is connected.");
        }
    }

    public void Receive(Action<string> callback)
    {
        Console.WriteLine("WebSocket receive callback registered.");
        _receiveCallback = callback;
    }
}
