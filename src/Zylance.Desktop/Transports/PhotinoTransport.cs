using Photino.NET;
using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Lib.Gateway;
using Zylance.Core.Lib.Gateway.Utils;

namespace Zylance.Desktop.Transports;

/// <summary>
///     Desktop implementation of <see cref="ITransport" /> using Photino.NET for native window communication.
///     Provides bidirectional message passing between the .NET backend and the embedded web UI.
/// </summary>
/// <remarks>
///     This transport implementation:
///     <list type="bullet">
///         <item>Uses Photino's web message API to communicate with the embedded browser</item>
///         <item>Handles special "Desktop:" prefixed events for window management operations</item>
///         <item>Supports a single registered message handler for received messages</item>
///         <item>Automatically deserializes messages to validate Gateway envelope format</item>
///     </list>
/// </remarks>
public class PhotinoTransport : ITransport
{
    private readonly PhotinoWindow _window;
    private Action<string>? _messageHandler;

    /// <summary>
    ///     Initializes a new instance of <see cref="PhotinoTransport" /> for the specified Photino window.
    /// </summary>
    /// <param name="window">The Photino window instance to use for message transport.</param>
    /// <remarks>
    ///     Automatically registers a web message handler with the Photino window to receive messages from the UI.
    /// </remarks>
    public PhotinoTransport(PhotinoWindow window)
    {
        _window = window;
        _window.RegisterWebMessageReceivedHandler(HandleWebMessageReceived);
    }

    /// <summary>
    ///     Sends a message to the UI layer via the Photino window's web message API.
    /// </summary>
    /// <param name="message">The serialized message string to send to the UI.</param>
    public void Send(string message)
    {
        _window.SendWebMessage(message);
    }

    /// <summary>
    ///     Registers a callback to receive messages from the UI layer.
    /// </summary>
    /// <param name="callback">The callback function to invoke when a message is received from the UI.</param>
    /// <remarks>
    ///     Only one callback can be registered at a time. Calling this method will replace any previously registered callback.
    ///     Messages with "Desktop:" event prefix are handled internally by <see cref="HandleDesktopEvent" /> and will not be
    ///     forwarded to the callback.
    /// </remarks>
    public void Receive(Action<string> callback)
    {
        _messageHandler = callback;
    }

    /// <summary>
    ///     Internal handler for web messages received from the Photino window.
    ///     Deserializes the message and routes it appropriately based on whether it's a desktop-specific event.
    /// </summary>
    /// <param name="sender">The sender of the web message (typically the Photino window).</param>
    /// <param name="message">The raw message string received from the UI.</param>
    /// <remarks>
    ///     Messages with "Desktop:" event prefix are handled internally for window management.
    ///     All other messages are forwarded to the registered message handler if one exists.
    /// </remarks>
    private void HandleWebMessageReceived(object? sender, string message)
    {
        var envelope = MessageUtils.FromJson<GatewayEnvelope>(message);
        if (envelope is null)
            return;

        if (MessageUtils.IsEventWithPrefix(envelope, "Desktop:"))
            HandleDesktopEvent(envelope.Event);
        else
            _messageHandler?.Invoke(message);
    }

    /// <summary>
    ///     Handles desktop-specific events that control window behavior.
    /// </summary>
    /// <param name="payload">The event payload containing the desktop event name and optional data.</param>
    private void HandleDesktopEvent(EventPayload payload)
    {
        Console.WriteLine("Intercepted desktop event: " + payload.EventName);
        switch (payload.EventName)
        {
            case "Desktop:Exit":
                _window.Close();
                break;
            default:
                Console.WriteLine("Unknown desktop event: " + payload.EventName);
                break;
        }
    }
}
