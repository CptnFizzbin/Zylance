using Zylance.Core.Lib.Gateway;

namespace Zylance.Core.Tests.Mocks;

/// <summary>
///     Mock implementation of ITransport for testing Gateway event handling.
///     Allows tests to simulate messages being received from the UI layer.
/// </summary>
public class MockTransport : ITransport
{
    private Action<string>? _messageHandler;
    private Action<string>? _messageReceiver;

    public void Send(string message)
    {
        _messageReceiver?.Invoke(message);
    }

    public void Receive(Action<string> callback)
    {
        _messageHandler = callback;
    }

    public void SendToGateway(string message)
    {
        _messageHandler?.Invoke(message);
    }

    public void ReceiveFromGateway(Action<string> callback)
    {
        _messageReceiver = callback;
    }
}
