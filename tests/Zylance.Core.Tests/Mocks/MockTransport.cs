using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Gateway.Utils;
using Zylance.Core.Platform.Interfaces;

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

    public void SendToGateway(GatewayEnvelope envelope)
    {
        envelope.MessageId = Guid.CreateVersion7().ToString();
        var msgJson = MessageUtils.ToJson(envelope);
        _messageHandler?.Invoke(msgJson);
    }

    public void SendToGateway(EventPayload payload)
    {
        var message = new GatewayEnvelope { Event = payload };
        SendToGateway(message);
    }

    public void SendToGateway(RequestPayload payload)
    {
        var message = new GatewayEnvelope { Request = payload };
        SendToGateway(message);
    }

    public void ReceiveFromGateway(Action<string> callback)
    {
        _messageReceiver = callback;
    }
}
