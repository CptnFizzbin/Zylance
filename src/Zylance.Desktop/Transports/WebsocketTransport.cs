using Fleck;
using Serilog;
using Zylance.Core.Logging;
using Zylance.Core.Platform.Interfaces;

namespace Zylance.Desktop.Transports;

/// <summary>
///     A simple WebSocket-based ITransport implementation for local UI
///     connections.
/// </summary>
public class WebsocketTransport : ITransport, IDisposable
{
    private static readonly ILogger Log = ZyLogger.CreateLogger<WebsocketTransport>();
    private readonly WebSocketServer _server;
    private IWebSocketConnection? _client;
    private Action<string>? _receiveCallback;

    /// <summary>
    ///     Initializes a new WebsocketTransport listening on the specified port.
    /// </summary>
    /// <param name="port">Port to bind the WebSocket server to.</param>
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
                    Log.Information("Second client attempted to connect, rejected.");
                    return;
                }

                _client = socket;
                Log.Information("WebSocket client connected.");
            };
            socket.OnClose = () =>
            {
                if (_client != socket)
                    return;

                Log.Information("WebSocket client disconnected.");
                _client = null;
            };
            socket.OnMessage = message =>
            {
                _receiveCallback?.Invoke(message);
            };
        });
    }

    /// <summary>
    ///     Disposes the transport and closes any active WebSocket connection.
    /// </summary>
    public void Dispose()
    {
        Log.Information("Disposing WebsocketTransport.");
        _client?.Close();
        _server.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Sends a serialized message to the connected WebSocket client, if any.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public void Send(string message)
    {
        if (_client is not null && _client.IsAvailable)
            _client.Send(message);
        else
            Log.Information("Attempted to send message, but no WebSocket client is connected.");
    }

    /// <summary>
    ///     Registers a callback to receive messages from the WebSocket client.
    /// </summary>
    /// <param name="callback">Callback invoked with the received message string.</param>
    public void Receive(Action<string> callback)
    {
        Log.Information("WebSocket receive callback registered.");
        _receiveCallback = callback;
    }
}
