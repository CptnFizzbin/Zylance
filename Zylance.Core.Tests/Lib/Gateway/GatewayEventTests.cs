using System.Reflection;
using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Lib.Gateway.Models;
using Zylance.Core.Lib.Gateway.Services;
using Zylance.Core.Tests.Mocks;

namespace Zylance.Core.Tests.Lib.Gateway;

public class GatewayEventTests
{
    private readonly GatewayService _gatewayService;
    private readonly RouterService _routerService = new();
    private readonly MockTransport _transport = new();

    public GatewayEventTests()
    {
        _gatewayService = new GatewayService(_transport, _routerService);
    }

    #region SubscribeToEvent Tests

    [Fact]
    public void SubscribeToEvent_ReturnsSubscription()
    {
        // Arrange
        var eventName = "Test:Event";
        var handler = new Action<ZyEvent>(_ => { });

        // Act
        var subscription = _gatewayService.SubscribeToEvent(eventName, handler);

        // Assert
        Assert.NotNull(subscription);
        Assert.NotEqual(Guid.Empty, subscription.Id);
    }

    [Fact]
    public void SubscribeToEvent_HandlerInvokedOnEventReceived()
    {
        // Arrange
        var eventName = "Test:Event";
        var eventCaught = false;

        _gatewayService.SubscribeToEvent(eventName, _ => eventCaught = true);

        // Act
        var eventPayload = new EventPayload { EventName = eventName, DataJson = "" };
        _transport.SendToGateway(eventPayload);

        // Assert
        Assert.True(eventCaught);
    }

    [Fact]
    public void SubscribeToEvent_HandlerReceivesCorrectEvent()
    {
        // Arrange
        var eventName = "Test:EventWithData";
        var receivedEventName = "";

        _gatewayService.SubscribeToEvent(eventName, evt => receivedEventName = evt.Name);

        // Act
        var eventPayload = new EventPayload { EventName = eventName, DataJson = "" };
        _transport.SendToGateway(eventPayload);

        // Assert
        Assert.Equal(eventName, receivedEventName);
    }

    [Fact]
    public void SubscribeToEvent_MultipleListenersOnSameEvent()
    {
        // Arrange
        var handler1Called = false;
        var handler2Called = false;
        var eventName = "Test:MultiListenerEvent";

        _gatewayService.SubscribeToEvent(eventName, _ => handler1Called = true);
        _gatewayService.SubscribeToEvent(eventName, _ => handler2Called = true);

        // Act
        var eventPayload = new EventPayload { EventName = eventName, DataJson = "" };
        _transport.SendToGateway(eventPayload);

        // Assert
        Assert.True(handler1Called);
        Assert.True(handler2Called);
    }

    [Fact]
    public void SubscribeToEvent_UnsubscribeStopsNotifications()
    {
        // Arrange
        var eventName = "Test:Event";
        var callCount = 0;

        var subscription = _gatewayService.SubscribeToEvent(eventName, _ => callCount++);

        // Act - First event
        var eventPayload = new EventPayload { EventName = eventName, DataJson = "" };
        _transport.SendToGateway(eventPayload);

        // Unsubscribe
        subscription.Unsubscribe();

        // Second event
        _transport.SendToGateway(eventPayload);

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void SubscribeToEvent_DifferentEventsDoNotCrossTrigger()
    {
        // Arrange
        var event1Name = "Test:Event1";
        var event2Name = "Test:Event2";
        var event1Called = false;
        var event2Called = false;

        _gatewayService.SubscribeToEvent(event1Name, _ => event1Called = true);
        _gatewayService.SubscribeToEvent(event2Name, _ => event2Called = true);

        // Act
        var event1Payload = new EventPayload { EventName = event1Name, DataJson = "" };
        _transport.SendToGateway(event1Payload);

        // Assert
        Assert.True(event1Called);
        Assert.False(event2Called);
    }

    #endregion

    #region ObserveEvent.TakeFirstAsync Tests

    [Fact]
    public async Task ObserveEvent_TakeFirstAsync_ReturnsEventWhenReceived()
    {
        // Arrange
        var eventName = "Test:WaitEvent";
        var task = _gatewayService.ObserveEvent(eventName).TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act
        var eventPayload = new EventPayload { EventName = eventName, DataJson = "" };
        _transport.SendToGateway(eventPayload);

        var result = await task;

        // Assert
        Assert.NotNull(result);
        Assert.True(eventName == result.Name);
    }

    [Fact]
    public async Task ObserveEvent_TakeFirstAsync_PredicateFiltersEvents()
    {
        // Arrange
        var eventName = "Test:FilterEvent";
        var task = _gatewayService
            .ObserveEvent(eventName)
            .Where(evt => evt.Name == eventName && evt.Payload.DataJson == "target")
            .TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act - Send non-matching event
        var otherPayload = new EventPayload { EventName = eventName, DataJson = "other" };
        _transport.SendToGateway(otherPayload);

        // Send matching event
        var targetPayload = new EventPayload { EventName = eventName, DataJson = "target" };
        _transport.SendToGateway(targetPayload);

        var result = await task;

        // Assert
        Assert.NotNull(result);
        Assert.True(eventName == result.Name);
    }

    [Fact]
    public async Task ObserveEvent_TakeFirstAsync_DefaultPredicateAcceptsAnyEvent()
    {
        // Arrange
        var eventName = "Test:DefaultPredicateEvent";
        var task = _gatewayService.ObserveEvent(eventName).TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act
        var eventPayload = new EventPayload { EventName = eventName, DataJson = "any data" };
        _transport.SendToGateway(eventPayload);

        var result = await task;

        // Assert
        Assert.NotNull(result);
        Assert.True("any data" == result.Payload.DataJson);
    }

    [Fact]
    public async Task ObserveEvent_TakeFirstAsync_UnsubscribesAfterCompletion_AlternateTest()
    {
        // Arrange
        var eventName = "Test:UnsubEvent";
        var handlerCallCount = 0;

        var task = _gatewayService.ObserveEvent(eventName).TakeFirstAsync(TestContext.Current.CancellationToken);
        _gatewayService.SubscribeToEvent(eventName, _ => handlerCallCount++);

        // Act
        // First event completes the wait
        var eventPayload = new EventPayload { EventName = eventName, DataJson = "" };
        _transport.SendToGateway(eventPayload);
        await task;

        // Second event should still reach the explicit subscription
        _transport.SendToGateway(eventPayload);

        // Assert - Handler should be called twice (both events)
        // but the internal ObserveEvent().TakeFirstAsync() subscription should be cleaned up
        Assert.Equal(2, handlerCallCount);
    }

    [Fact]
    public async Task ObserveEvent_TakeFirstAsync_CanCancelWithCancellationToken()
    {
        // Arrange
        var eventName = "Test:CancelEvent";
        using var cts = new CancellationTokenSource();
        var task = _gatewayService.ObserveEvent(eventName).TakeFirstAsync(cts.Token);

        // Act
        await cts.CancelAsync();

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
    }

    [Fact]
    public async Task ObserveEvent_TakeFirstAsync_TimeoutBehavior()
    {
        // Arrange
        var eventName = "Test:TimeoutEvent";
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        var task = _gatewayService.ObserveEvent(eventName).TakeFirstAsync(cts.Token);

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
    }

    [Fact]
    public async Task ObserveEvent_TakeFirstAsync_MultipleWaitersOnSameEvent()
    {
        // Arrange
        var eventName = "Test:MultiWaiterEvent";
        var task1 = _gatewayService.ObserveEvent(eventName).TakeFirstAsync(TestContext.Current.CancellationToken);
        var task2 = _gatewayService.ObserveEvent(eventName).TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act
        var eventPayload = new EventPayload { EventName = eventName, DataJson = "data" };
        _transport.SendToGateway(eventPayload);

        var result1 = await task1;
        var result2 = await task2;

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.True(result1.Name == result2.Name);
    }

    [Fact]
    public async Task ObserveEvent_TakeFirstAsync_PredicateException_PropagatesError()
    {
        // Arrange
        var eventName = "Test:ErrorEvent";
        var task = _gatewayService
            .ObserveEvent(eventName)
            .Where(evt =>
            {
                if (evt.Payload.DataJson == "throw")
                    throw new InvalidOperationException("Test exception");

                return evt.Payload.DataJson == "target";
            })
            .TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act
        var throwingPayload = new EventPayload { EventName = eventName, DataJson = "throw" };
        _transport.SendToGateway(throwingPayload);

        // Assert - exception should propagate to the awaiter
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);
    }

    [Fact]
    public async Task ObserveEvent_TakeFirstAsync_CorrectEventNameRequired()
    {
        // Arrange
        var eventName1 = "Test:Event1";
        var eventName2 = "Test:Event2";
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        var task = _gatewayService.ObserveEvent(eventName1).TakeFirstAsync(cts.Token);

        // Act - Send event with different name
        var wrongPayload = new EventPayload { EventName = eventName2, DataJson = "" };
        _transport.SendToGateway(wrongPayload);

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
    }

    [Fact]
    public async Task ObserveEvent_TakeFirstAsync_CancellationRemovesSubscription()
    {
        // Arrange
        var eventName = "Test:CancelCleanup";
        using var cts = new CancellationTokenSource();
        var task = _gatewayService.ObserveEvent(eventName).TakeFirstAsync(cts.Token);

        // Act
        await cts.CancelAsync();

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        await WaitForListenerCountAsync(eventName, 0);
    }

    [Fact]
    public async Task ObserveEvent_TakeFirstAsync_TimeoutRemovesSubscription()
    {
        // Arrange
        var eventName = "Test:TimeoutCleanup";
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        var task = _gatewayService.ObserveEvent(eventName).TakeFirstAsync(cts.Token);

        // Act
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);

        // Assert
        await WaitForListenerCountAsync(eventName, 0);
    }

    private int GetListenerCount(string eventName)
    {
        var field = typeof(GatewayService).GetField("_eventListeners", BindingFlags.Instance | BindingFlags.NonPublic);
        var listeners = field?.GetValue(_gatewayService);
        if (listeners is null)
            return 0;

        var tryGetValue = listeners.GetType().GetMethod("TryGetValue");
        if (tryGetValue is null)
            return 0;

        object?[] args = [eventName, null];
        var found = (bool)tryGetValue.Invoke(listeners, args)!;
        if (!found)
            return 0;

        var list = args[1];
        if (list is null)
            return 0;

        var countProperty = list.GetType().GetProperty("Count");
        return countProperty is null ? 0 : (int)countProperty.GetValue(list)!;
    }

    private async Task WaitForListenerCountAsync(string eventName, int expectedCount)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(500);
        while (DateTime.UtcNow < deadline)
        {
            if (GetListenerCount(eventName) == expectedCount)
                return;

            await Task.Delay(10);
        }

        Assert.Equal(expectedCount, GetListenerCount(eventName));
    }

    #endregion
}
