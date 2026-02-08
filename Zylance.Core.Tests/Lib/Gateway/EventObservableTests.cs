using Zylance.Core.Lib.Gateway.Services;
using Zylance.Core.Tests.Mocks;

namespace Zylance.Core.Tests.Lib.Gateway;

/// <summary>
///     Tests for the fluent EventObservable API (ObserveEvent).
/// </summary>
public class EventObservableTests
{
    private readonly GatewayService _gatewayService;
    private readonly RouterService _routerService = new();
    private readonly MockTransport _transport = new();

    public EventObservableTests()
    {
        _gatewayService = new GatewayService(_transport, _routerService);
    }

    #region Integration Tests

    [Fact]
    public async Task ObserveEvent_ComplexChain_WorksEndToEnd()
    {
        // Arrange
        var eventName = "item:added";
        var task = _gatewayService
            .ObserveEvent(eventName)
            .Where(evt => evt.Payload.DataJson.StartsWith("{"))
            .Select(evt => evt.Payload.DataJson.Length)
            .Where(length => length > 10)
            .Where(length => length < 50)
            .FirstAsync(TestContext.Current.CancellationToken);

        // Act
        _gatewayService.TriggerEvent(eventName, "invalid");
        _gatewayService.TriggerEvent(eventName, "{small}");
        _gatewayService.TriggerEvent(eventName, "{" + new string('x', 60) + "}");
        _gatewayService.TriggerEvent(eventName, "{valid-json-data}");

        var result = await task;

        // Assert
        Assert.Equal(17, result);
    }

    #endregion

    #region Helper Methods

    // TriggerEvent extension is available via GatewayTestExtensions

    #endregion

    #region FirstAsync Tests

    [Fact]
    public async Task ObserveEvent_FirstAsync_ReturnsFirstEvent()
    {
        // Arrange
        var eventName = "test:event";
        var task = _gatewayService.ObserveEvent(eventName).FirstAsync(TestContext.Current.CancellationToken);

        // Act
        _gatewayService.TriggerEvent(eventName, "first");

        var result = await task;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventName, result.Name);
        Assert.Equal("first", result.Payload.DataJson);
    }

    [Fact]
    public async Task ObserveEvent_FirstAsync_WithCancellationToken()
    {
        // Arrange
        var eventName = "test:cancellable";
        using var cts = new CancellationTokenSource();
        var task = _gatewayService.ObserveEvent(eventName).FirstAsync(cts.Token);

        // Act
        cts.Cancel();

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
    }

    #endregion

    #region Where Tests

    [Fact]
    public async Task ObserveEvent_Where_FiltersEvents()
    {
        // Arrange
        var eventName = "test:filtered";
        var task = _gatewayService
            .ObserveEvent(eventName)
            .Where(evt => evt.Payload.DataJson == "target")
            .FirstAsync(TestContext.Current.CancellationToken);

        // Act - Send non-matching event
        _gatewayService.TriggerEvent(eventName, "wrong");

        // Send matching event
        _gatewayService.TriggerEvent(eventName, "target");

        var result = await task;

        // Assert
        Assert.Equal("target", result.Payload.DataJson);
    }

    [Fact]
    public async Task ObserveEvent_Where_CanChainMultipleFilters()
    {
        // Arrange
        var eventName = "test:multi-filter";
        var task = _gatewayService
            .ObserveEvent(eventName)
            .Where(evt => evt.Payload.DataJson.Contains("foo"))
            .Where(evt => evt.Payload.DataJson.Contains("bar"))
            .FirstAsync(TestContext.Current.CancellationToken);

        // Act
        _gatewayService.TriggerEvent(eventName, "foo");
        _gatewayService.TriggerEvent(eventName, "bar");
        _gatewayService.TriggerEvent(eventName, "foo-bar");

        var result = await task;

        // Assert
        Assert.Equal("foo-bar", result.Payload.DataJson);
    }

    #endregion

    #region Select Tests

    [Fact]
    public async Task ObserveEvent_Select_ProjectsValue()
    {
        // Arrange
        var eventName = "test:projection";
        var task = _gatewayService
            .ObserveEvent(eventName)
            .Select(evt => evt.Payload.DataJson)
            .FirstAsync(TestContext.Current.CancellationToken);

        // Act
        _gatewayService.TriggerEvent(eventName, "projected-value");

        var result = await task;

        // Assert
        Assert.Equal("projected-value", result);
    }

    [Fact]
    public async Task ObserveEvent_Select_ProjectsComplexValue()
    {
        // Arrange
        var eventName = "test:complex-projection";
        var task = _gatewayService
            .ObserveEvent(eventName)
            .Select(evt => new
            {
                evt.Name,
                Data = evt.Payload.DataJson,
                evt.Payload.DataJson.Length,
            })
            .FirstAsync(TestContext.Current.CancellationToken);

        // Act
        _gatewayService.TriggerEvent(eventName, "test");

        var result = await task;

        // Assert
        Assert.Equal("test", result.Name);
        Assert.Equal("test", result.Data);
        Assert.Equal(4, result.Length);
    }

    #endregion

    #region Select + Where Tests

    [Fact]
    public async Task ObserveEvent_SelectThenWhere_FiltersProjectedValue()
    {
        // Arrange
        var eventName = "test:select-where";
        var task = _gatewayService
            .ObserveEvent(eventName)
            .Select(evt => evt.Payload.DataJson.Length)
            .Where(length => length > 5)
            .FirstAsync(TestContext.Current.CancellationToken);

        // Act
        _gatewayService.TriggerEvent(eventName, "hi");
        _gatewayService.TriggerEvent(eventName, "hello");
        _gatewayService.TriggerEvent(eventName, "hello world");

        var result = await task;

        // Assert
        Assert.Equal(11, result);
    }

    [Fact]
    public async Task ObserveEvent_SelectThenMultipleWhere_ChainsFilters()
    {
        // Arrange
        var eventName = "test:chain";
        var task = _gatewayService
            .ObserveEvent(eventName)
            .Select(evt => int.Parse(evt.Payload.DataJson))
            .Where(num => num > 5)
            .Where(num => num < 20)
            .Where(num => num % 2 == 0)
            .FirstAsync(TestContext.Current.CancellationToken);

        // Act
        _gatewayService.TriggerEvent(eventName, "3");
        _gatewayService.TriggerEvent(eventName, "7");
        _gatewayService.TriggerEvent(eventName, "22");
        _gatewayService.TriggerEvent(eventName, "10");

        var result = await task;

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public async Task ObserveEvent_WhereBeforeSelect_FiltersThenProjects()
    {
        // Arrange
        var eventName = "test:where-select";
        var task = _gatewayService
            .ObserveEvent(eventName)
            .Where(evt => evt.Payload.DataJson.StartsWith("valid"))
            .Select(evt => evt.Payload.DataJson.Length)
            .FirstAsync(TestContext.Current.CancellationToken);

        // Act
        _gatewayService.TriggerEvent(eventName, "invalid");
        _gatewayService.TriggerEvent(eventName, "valid-data");

        var result = await task;

        // Assert
        Assert.Equal(10, result);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ObserveEvent_Select_HandlesProjectionException()
    {
        // Arrange
        var eventName = "test:error";
        var task = _gatewayService
            .ObserveEvent(eventName)
            .Select(evt => int.Parse(evt.Payload.DataJson))
            .Where(num => num > 0)
            .FirstAsync(TestContext.Current.CancellationToken);

        // Act - Send invalid data that will throw during parse
        _gatewayService.TriggerEvent(eventName, "not-a-number");

        // Send valid data
        _gatewayService.TriggerEvent(eventName, "42");

        var result = await task;

        // Assert - Should skip the exception and wait for valid event
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ObserveEvent_Where_HandlesPredicateException()
    {
        // Arrange
        var eventName = "test:predicate-error";
        var receivedEvents = new List<string>();

        var task = _gatewayService
            .ObserveEvent(eventName)
            .Where(evt =>
            {
                receivedEvents.Add(evt.Payload.DataJson);
                if (evt.Payload.DataJson == "throw")
                    throw new InvalidOperationException("Test exception");

                return evt.Payload.DataJson == "valid";
            })
            .FirstAsync(TestContext.Current.CancellationToken);

        // Act
        _gatewayService.TriggerEvent(eventName, "invalid");
        _gatewayService.TriggerEvent(eventName, "throw");
        _gatewayService.TriggerEvent(eventName, "valid");

        var result = await task;

        // Assert
        Assert.Equal("valid", result.Payload.DataJson);
        Assert.Equal(3, receivedEvents.Count);
    }

    #endregion

    #region Multiple Waiters Tests

    [Fact]
    public async Task ObserveEvent_MultipleWaiters_AllReceiveEvent()
    {
        // Arrange
        var eventName = "test:multi-waiter";
        var task1 = _gatewayService.ObserveEvent(eventName).FirstAsync(TestContext.Current.CancellationToken);
        var task2 = _gatewayService.ObserveEvent(eventName).FirstAsync(TestContext.Current.CancellationToken);
        var task3 = _gatewayService.ObserveEvent(eventName).FirstAsync(TestContext.Current.CancellationToken);

        // Act
        _gatewayService.TriggerEvent(eventName, "broadcast");

        var results = await Task.WhenAll(task1, task2, task3);

        // Assert
        Assert.All(results, result => Assert.Equal("broadcast", result.Payload.DataJson));
    }

    [Fact]
    public async Task ObserveEvent_MultipleWaitersWithDifferentFilters()
    {
        // Arrange
        var eventName = "test:diff-filters";
        var task1 = _gatewayService
            .ObserveEvent(eventName)
            .Where(evt => evt.Payload.DataJson.Contains("A"))
            .FirstAsync(TestContext.Current.CancellationToken);

        var task2 = _gatewayService
            .ObserveEvent(eventName)
            .Where(evt => evt.Payload.DataJson.Contains("B"))
            .FirstAsync(TestContext.Current.CancellationToken);

        // Act
        _gatewayService.TriggerEvent(eventName, "value-A");
        _gatewayService.TriggerEvent(eventName, "value-B");

        var result1 = await task1;
        var result2 = await task2;

        // Assert
        Assert.Equal("value-A", result1.Payload.DataJson);
        Assert.Equal("value-B", result2.Payload.DataJson);
    }

    #endregion

    #region Subscribe Tests

    [Fact]
    public void ObserveEvent_Subscribe_ReceivesEvents()
    {
        // Arrange
        var eventName = "test:subscribe";
        var receivedEvents = new List<string>();

        _ = _gatewayService.ObserveEvent(eventName).Subscribe(evt => receivedEvents.Add(evt.Payload.DataJson));

        // Act
        _gatewayService.TriggerEvent(eventName, "event1");
        _gatewayService.TriggerEvent(eventName, "event2");
        _gatewayService.TriggerEvent(eventName, "event3");

        // Assert
        Assert.Equal(3, receivedEvents.Count);
        Assert.Equal(["event1", "event2", "event3"], receivedEvents);
    }

    [Fact]
    public void ObserveEvent_Subscribe_WithFilter_OnlyReceivesMatchingEvents()
    {
        // Arrange
        var eventName = "test:filtered-subscribe";
        var receivedEvents = new List<string>();

        _ = _gatewayService
            .ObserveEvent(eventName)
            .Where(evt => evt.Payload.DataJson.Contains("match"))
            .Subscribe(evt => receivedEvents.Add(evt.Payload.DataJson));

        // Act
        _gatewayService.TriggerEvent(eventName, "skip-1");
        _gatewayService.TriggerEvent(eventName, "match-1");
        _gatewayService.TriggerEvent(eventName, "skip-2");
        _gatewayService.TriggerEvent(eventName, "match-2");

        // Assert
        Assert.Equal(2, receivedEvents.Count);
        Assert.Equal(["match-1", "match-2"], receivedEvents);
    }

    [Fact]
    public void ObserveEvent_Subscribe_Unsubscribe_StopsReceivingEvents()
    {
        // Arrange
        var eventName = "test:unsubscribe";
        var receivedEvents = new List<string>();

        var subscription = _gatewayService
            .ObserveEvent(eventName)
            .Subscribe(evt => receivedEvents.Add(evt.Payload.DataJson));

        // Act
        _gatewayService.TriggerEvent(eventName, "before");
        subscription.Unsubscribe();
        _gatewayService.TriggerEvent(eventName, "after");

        // Assert
        Assert.Single(receivedEvents);
        Assert.Equal("before", receivedEvents[0]);
    }

    [Fact]
    public void ObserveEvent_Subscribe_WithProjection_ReceivesProjectedValues()
    {
        // Arrange
        var eventName = "test:projected-subscribe";
        var receivedValues = new List<int>();

        _ = _gatewayService
            .ObserveEvent(eventName)
            .Select(evt => evt.Payload.DataJson.Length)
            .Subscribe(length => receivedValues.Add(length));

        // Act
        _gatewayService.TriggerEvent(eventName, "ab");
        _gatewayService.TriggerEvent(eventName, "abcd");
        _gatewayService.TriggerEvent(eventName, "abcdef");

        // Assert
        Assert.Equal(3, receivedValues.Count);
        Assert.Equal([2, 4, 6], receivedValues);
    }

    [Fact]
    public void ObserveEvent_Subscribe_WithProjectionAndFilter_WorksCorrectly()
    {
        // Arrange
        var eventName = "test:complex-subscribe";
        var receivedValues = new List<int>();

        _ = _gatewayService
            .ObserveEvent(eventName)
            .Select(evt => int.Parse(evt.Payload.DataJson))
            .Where(num => num % 2 == 0)
            .Subscribe(num => receivedValues.Add(num));

        // Act
        _gatewayService.TriggerEvent(eventName, "1");
        _gatewayService.TriggerEvent(eventName, "2");
        _gatewayService.TriggerEvent(eventName, "3");
        _gatewayService.TriggerEvent(eventName, "4");
        _gatewayService.TriggerEvent(eventName, "5");
        _gatewayService.TriggerEvent(eventName, "6");

        // Assert
        Assert.Equal(3, receivedValues.Count);
        Assert.Equal([2, 4, 6], receivedValues);
    }

    [Fact]
    public void ObserveEvent_Subscribe_WithProjectionError_SkipsFailedEvents()
    {
        // Arrange
        var eventName = "test:projection-error-subscribe";
        var receivedValues = new List<int>();

        _ = _gatewayService
            .ObserveEvent(eventName)
            .Select(evt => int.Parse(evt.Payload.DataJson))
            .Subscribe(num => receivedValues.Add(num));

        // Act
        _gatewayService.TriggerEvent(eventName, "1");
        _gatewayService.TriggerEvent(eventName, "not-a-number");
        _gatewayService.TriggerEvent(eventName, "2");
        _gatewayService.TriggerEvent(eventName, "invalid");
        _gatewayService.TriggerEvent(eventName, "3");

        // Assert
        Assert.Equal(3, receivedValues.Count);
        Assert.Equal([1, 2, 3], receivedValues);
    }

    [Fact]
    public void ObserveEvent_Subscribe_MultipleSubscribers_AllReceiveEvents()
    {
        // Arrange
        var eventName = "test:multi-subscribe";
        var received1 = new List<string>();
        var received2 = new List<string>();

        _ = _gatewayService.ObserveEvent(eventName).Subscribe(_ => received1.Add("sub1"));
        _ = _gatewayService.ObserveEvent(eventName).Subscribe(_ => received2.Add("sub2"));

        // Act
        _gatewayService.TriggerEvent(eventName, "event");

        // Assert
        Assert.Single(received1);
        Assert.Single(received2);
    }

    #endregion
}
