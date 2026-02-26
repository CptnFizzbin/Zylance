using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Gateway.Services;
using Zylance.Core.Tests.TestUtils.Mocks;

namespace Zylance.Core.Tests.Gateway.Services;

public class GatewayServiceTests
{
    private readonly List<string> _sentMessages;
    private readonly GatewayService _service;
    private readonly TestTransport _transport;

    public GatewayServiceTests()
    {
        _sentMessages = [];
        _transport = new TestTransport();
        _transport.ReceiveFromGateway(msg => _sentMessages.Add(msg));
        _service = new GatewayService(_transport);
    }

    [Fact]
    public void Send_ResponsePayload_SendsEnvelope()
    {
        // Arrange
        var response = new ResponsePayload { RequestId = "req-1", DataJson = "{}" };

        // Act
        _service.Send(response);

        // Assert
        Assert.Single(_sentMessages);
        Assert.Contains("response", _sentMessages[0].ToLower());
        _service.Dispose();
    }

    [Fact]
    public void SendEvent_EventPayload_SendsEnvelope()
    {
        // Arrange
        var evt = new EventPayload { EventName = "TestEvent", DataJson = "{}" };

        // Act
        _service.Send(evt);

        // Assert
        Assert.Single(_sentMessages);
        Assert.Contains("Event", _sentMessages[0]);
        _service.Dispose();
    }

    [Fact]
    public void Send_ErrorPayload_SendsEnvelope()
    {
        // Arrange
        var error = new ErrorPayload { Type = "TestError", Details = "fail" };

        // Act
        _service.Send(error);

        // Assert
        Assert.Single(_sentMessages);
        Assert.Contains("Error", _sentMessages[0]);
        _service.Dispose();
    }

    [Fact]
    public void ObserveRequests_ObserverReceivesRequest()
    {
        // Arrange
        var observed = new List<RequestPayload>();
        var subscription = _service.ObserveRequests().Subscribe(observed.Add);
        var payload = new RequestPayload { Action = "TestAction", DataJson = "{}" };

        // Act
        _transport.SendToGateway(payload);

        // Assert
        Assert.Single(observed);
        Assert.Equal("TestAction", observed[0].Action);
        subscription.Dispose();
        _service.Dispose();
    }

    [Fact]
    public void ObserveEvents_ObserverReceivesEvent()
    {
        // Arrange
        var observed = new List<EventPayload>();
        var subscription = _service.ObserveEvents().Subscribe(observed.Add);
        var payload = new EventPayload { EventName = "TestEvent", DataJson = "{}" };

        // Act
        _transport.SendToGateway(payload);

        // Assert
        Assert.Single(observed);
        Assert.Equal("TestEvent", observed[0].EventName);
        subscription.Dispose();
        _service.Dispose();
    }

    [Fact]
    public void SubscribeToEvent_HandlerIsCalled()
    {
        // Arrange
        ZyEvent? received = null;
        var subscription = _service.SubscribeToEvent("TestEvent", evt => received = evt);
        var payload = new EventPayload { EventName = "TestEvent", DataJson = "{}" };

        // Act
        _transport.SendToGateway(payload);

        // Assert
        Assert.NotNull(received);
        Assert.Equal("TestEvent", received.Payload.EventName);
        subscription.Dispose();
        _service.Dispose();
    }

    [Fact]
    public void Dispose_CompletesObserversAndListeners()
    {
        // Arrange
        var requestCompleted = false;
        var eventCompleted = false;
        _service.ObserveRequests().Subscribe(_ => { }, () => requestCompleted = true);
        _service.ObserveEvents().Subscribe(_ => { }, () => eventCompleted = true);

        // Act
        _service.Dispose();

        // Assert
        Assert.True(requestCompleted);
        Assert.True(eventCompleted);
    }
}
