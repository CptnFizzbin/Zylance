using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Gateway.Services;
using Zylance.Core.Router.Services;
using Zylance.Core.Tests.Mocks;

namespace Zylance.Core.Tests.TestUtils.Gateway;

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
            .Where(evt => evt.Payload.DataJson.StartsWith('{'))
            .Select(evt => evt.Payload.DataJson.Length)
            .Where(length => length > 10)
            .Where(length => length < 50)
            .TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "invalid" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "{small}" });
        _transport.SendToGateway(
            new EventPayload { EventName = eventName, DataJson = "{" + new string('x', 60) + "}" }
        );
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "{valid-json-data}" });

        var result = await task;

        // Assert
        Assert.Equal(17, result);
    }

    #endregion

    #region TakeFirstAsync Tests

    [Fact]
    public async Task ObserveEvent_TakeFirstAsync_ReturnsFirstEvent()
    {
        // Arrange
        var eventName = "test:event";
        var task = _gatewayService.ObserveEvent(eventName).TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "first" });

        var result = await task;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventName, result.Name);
        Assert.Equal("first", result.Payload.DataJson);
    }

    [Fact]
    public async Task ObserveEvent_TakeFirstAsync_WithCancellationToken()
    {
        // Arrange
        var eventName = "test:cancellable";
        using var cts = new CancellationTokenSource();
        var task = _gatewayService.ObserveEvent(eventName).TakeFirstAsync(cts.Token);

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
            .TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act - Send non-matching event
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "wrong" });

        // Send matching event
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "target" });

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
            .TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "foo" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "bar" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "foo-bar" });

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
            .TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "projected-value" });

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
            .TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "test" });

        var result = await task;

        // Assert
        Assert.Equal(eventName, result.Name);
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
            .TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "hi" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "hello" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "hello world" });

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
            .TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "3" }); // fails where 1
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "22" }); // fails where 2
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "7" }); // fails where 3
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "10" });

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
            .TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "invalid" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "valid-data" });

        var result = await task;

        // Assert
        Assert.Equal(10, result);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ObserveEvent_Select_PropagatesProjectionException()
    {
        // Arrange
        var eventName = "test:error";
        var task = _gatewayService
            .ObserveEvent(eventName)
            .Select(evt => int.Parse(evt.Payload.DataJson))
            .Where(num => num > 0)
            .TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act - Send invalid data that will throw during parse
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "not-a-number" });

        // Assert - Should propagate the exception
        await Assert.ThrowsAsync<FormatException>(async () => await task);
    }

    [Fact]
    public async Task ObserveEvent_Where_PropagatesPredicateException()
    {
        // Arrange
        var eventName = "test:predicate-error";

        var task = _gatewayService
            .ObserveEvent(eventName)
            .Where(evt =>
            {
                if (evt.Payload.DataJson == "throw")
                    throw new InvalidOperationException("Test exception");

                return evt.Payload.DataJson == "valid";
            })
            .TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act - Send data that will throw in predicate
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "throw" });

        // Assert - Should propagate the exception
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);
    }

    #endregion

    #region Multiple Waiters Tests

    [Fact]
    public async Task ObserveEvent_MultipleWaiters_AllReceiveEvent()
    {
        // Arrange
        var eventName = "test:multi-waiter";
        var task1 = _gatewayService.ObserveEvent(eventName).TakeFirstAsync(TestContext.Current.CancellationToken);
        var task2 = _gatewayService.ObserveEvent(eventName).TakeFirstAsync(TestContext.Current.CancellationToken);
        var task3 = _gatewayService.ObserveEvent(eventName).TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "broadcast" });

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
            .TakeFirstAsync(TestContext.Current.CancellationToken);

        var task2 = _gatewayService
            .ObserveEvent(eventName)
            .Where(evt => evt.Payload.DataJson.Contains("B"))
            .TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "value-A" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "value-B" });

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
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "event1" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "event2" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "event3" });

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
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "skip-1" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "match-1" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "skip-2" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "match-2" });

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
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "before" });
        subscription.Dispose();
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "after" });

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
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "ab" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "abcd" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "abcdef" });

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
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "1" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "2" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "3" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "4" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "5" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "6" });

        // Assert
        Assert.Equal(3, receivedValues.Count);
        Assert.Equal([2, 4, 6], receivedValues);
    }

    [Fact]
    public void ObserveEvent_Subscribe_WithProjectionError_PropagatesException()
    {
        // Arrange
        var eventName = "test:projection-error-subscribe";
        var receivedValues = new List<int>();
        Exception? caughtException = null;

        _ = _gatewayService
            .ObserveEvent(eventName)
            .Select(evt =>
            {
                try
                {
                    return int.Parse(evt.Payload.DataJson);
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                    throw;
                }
            })
            .Subscribe(num => receivedValues.Add(num));

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "1" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "not-a-number" }); // This should throw

        // Assert - First event succeeded, second event threw exception
        Assert.Single(receivedValues);
        Assert.Equal(1, receivedValues[0]);
        Assert.NotNull(caughtException);
        Assert.IsType<FormatException>(caughtException);
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
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "event" });

        // Assert
        Assert.Single(received1);
        Assert.Single(received2);
    }

    #endregion

    #region TrySelect Tests

    [Fact]
    public async Task ObserveEvent_TrySelect_SkipsEventsWithProjectionErrors()
    {
        // Arrange
        var eventName = "test:try-select";
        var task = _gatewayService
            .ObserveEvent(eventName)
            .TrySelect(evt => int.Parse(evt.Payload.DataJson))
            .TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act - Send invalid data first (will be skipped), then valid data
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "not-a-number" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "also-invalid" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "42" });

        var result = await task;

        // Assert - Should skip the exceptions and wait for valid event
        Assert.Equal(42, result);
    }

    [Fact]
    public void ObserveEvent_TrySelect_Subscribe_SkipsFailedProjections()
    {
        // Arrange
        var eventName = "test:try-select-subscribe";
        var receivedValues = new List<int>();

        _ = _gatewayService
            .ObserveEvent(eventName)
            .TrySelect(evt => int.Parse(evt.Payload.DataJson))
            .Subscribe(num => receivedValues.Add(num));

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "1" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "not-a-number" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "2" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "invalid" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "3" });

        // Assert - Only valid numbers should be received
        Assert.Equal(3, receivedValues.Count);
        Assert.Equal([1, 2, 3], receivedValues);
    }

    [Fact]
    public void ObserveEvent_TrySelect_WithWhere_FiltersCorrectly()
    {
        // Arrange
        var eventName = "test:try-select-where";
        var receivedValues = new List<int>();

        _ = _gatewayService
            .ObserveEvent(eventName)
            .TrySelect(evt => int.Parse(evt.Payload.DataJson))
            .Where(num => num > 5)
            .Subscribe(num => receivedValues.Add(num));

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "1" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "not-a-number" }); // Skipped by TrySelect
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "3" }); // Filtered by Where
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "10" }); // Passes
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "invalid" }); // Skipped by TrySelect
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "7" }); // Passes

        // Assert
        Assert.Equal(2, receivedValues.Count);
        Assert.Equal([10, 7], receivedValues);
    }

    [Fact]
    public void ObserveEvent_TrySelect_ChainedProjections()
    {
        // Arrange
        var eventName = "test:try-select-chain";
        var receivedValues = new List<string>();

        _ = _gatewayService
            .ObserveEvent(eventName)
            .TrySelect(evt => int.Parse(evt.Payload.DataJson)) // Parse to int
            .TrySelect(num => num * 2) // Double it
            .Select(num => num.ToString()) // Convert back to string
            .Subscribe(str => receivedValues.Add(str));

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "5" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "not-a-number" });
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "10" });

        // Assert
        Assert.Equal(2, receivedValues.Count);
        Assert.Equal(["10", "20"], receivedValues);
    }

    [Fact]
    public async Task ObserveEvent_TrySelect_WithExistingPredicate()
    {
        // Arrange
        var eventName = "test:try-select-predicate";
        var task = _gatewayService
            .ObserveEvent(eventName)
            .Where(evt => evt.Payload.DataJson.Length > 1) // Filter first
            .TrySelect(evt => int.Parse(evt.Payload.DataJson)) // Then try parse
            .TakeFirstAsync(TestContext.Current.CancellationToken);

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "x" }); // Filtered by Where
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "ab" }); // Passes Where, fails TrySelect
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "42" }); // Passes both

        var result = await task;

        // Assert
        Assert.Equal(42, result);
    }

    #endregion

    #region Selector Call Count Tests

    [Fact]
    public void ObserveEvent_Select_SelectorCalledOncePerEvent()
    {
        // Arrange
        var eventName = "test:selector-call-count";
        var selectorCallCount = 0;

        var observable = _gatewayService
            .ObserveEvent(eventName)
            .Select(evt =>
            {
                selectorCallCount++;
                return evt.Payload.DataJson.Length;
            });

        var receivedValues = new List<int>();
        observable.Subscribe(receivedValues.Add);

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "first" });

        // Assert - Selector should be called exactly once per event
        Assert.Equal(1, selectorCallCount);
        Assert.Single(receivedValues);
        Assert.Equal([5], receivedValues);
    }

    [Fact]
    public void ObserveEvent_ChainedSelectors_EachCalledOncePerEvent()
    {
        // Arrange
        var eventName = "test:chained-selector-count";
        var firstSelectorCallCount = 0;
        var secondSelectorCallCount = 0;

        var observable = _gatewayService
            .ObserveEvent(eventName)
            .Select(evt =>
            {
                firstSelectorCallCount++;
                return int.Parse(evt.Payload.DataJson);
            })
            .Select(num =>
            {
                secondSelectorCallCount++;
                return num * 2;
            });

        var receivedValues = new List<int>();
        _ = observable.Subscribe(value => receivedValues.Add(value));

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "10" });

        // Assert - Each selector should be called exactly once per event
        Assert.Equal(1, firstSelectorCallCount);
        Assert.Equal(1, secondSelectorCallCount);
        Assert.Single(receivedValues);
        Assert.Equal([20], receivedValues);
    }

    [Fact]
    public void ObserveEvent_WhereBeforeSelect_SelectorNotCalledForFilteredEvents()
    {
        // Arrange
        var eventName = "test:where-select-call-count";
        var whereCallCount = 0;
        var selectorCallCount = 0;

        var observable = _gatewayService
            .ObserveEvent(eventName)
            .Where(evt =>
            {
                whereCallCount++;
                return evt.Payload.DataJson.Length > 3;
            })
            .Select(evt =>
            {
                selectorCallCount++;
                return evt.Payload.DataJson.Length;
            });

        var receivedValues = new List<int>();
        _ = observable.Subscribe(value => receivedValues.Add(value));

        // Act
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "ab" }); // Where: called (fails), Selector: not called
        _transport.SendToGateway(new EventPayload { EventName = eventName, DataJson = "abcd" }); // Where: called (passes), Selector: called

        // Assert
        Assert.Equal(2, whereCallCount);
        Assert.Equal(1, selectorCallCount);
        Assert.Single(receivedValues);
        Assert.Equal([4], receivedValues);
    }

    #endregion
}
