namespace Zylance.Core.Lib.Gateway;

/// <summary>
///     Defines a transport layer for bidirectional message communication between the application core and UI layer.
///     This abstraction allows different implementations (e.g., Photino for desktop, web sockets for web)
///     while maintaining a consistent communication interface.
/// </summary>
/// <remarks>
///     Implementations should handle serialization/deserialization of Protocol Buffer messages
///     and provide reliable message delivery in both directions.
/// </remarks>
public interface ITransport
{
    /// <summary>
    ///     Sends a message from the application core to the UI layer.
    /// </summary>
    /// <param name="message">The serialized message string to send. Typically, a Protocol Buffer message serialized to JSON.</param>
    /// <remarks>
    ///     This method should be non-blocking and handle transmission errors gracefully.
    ///     Messages are typically Gateway envelopes containing requests, responses, or events.
    /// </remarks>
    public void Send(string message);

    /// <summary>
    ///     Registers a callback to receive messages from the UI layer.
    /// </summary>
    /// <param name="callback">
    ///     The callback function to invoke when a message is received. The callback receives the message as
    ///     a string parameter.
    /// </param>
    /// <remarks>
    ///     Multiple callbacks can be registered and all will be invoked when a message arrives.
    ///     The callback should handle deserialization and processing of the message.
    ///     Callbacks should not block the transport layer.
    /// </remarks>
    public void Receive(Action<string> callback);
}
