using Zylance.Core.Lib.Gateway.Services;

namespace Zylance.Core.Lib.Gateway.Models;

/// <summary>
///     Typed version of EventObservable that supports projection results.
/// </summary>
public class EventObservable<TResult>
{
    private readonly string _eventName;
    private readonly GatewayService _gatewayService;
    private readonly Func<ZyEvent, bool>? _predicate;
    private readonly Func<ZyEvent, TResult> _selector;

    internal EventObservable(
        GatewayService gatewayService,
        string eventName,
        Func<ZyEvent, bool>? predicate,
        Func<ZyEvent, TResult> selector
    )
    {
        _gatewayService = gatewayService;
        _eventName = eventName;
        _predicate = predicate;
        _selector = selector;
    }

    public EventObservable<TNext> Select<TNext>(Func<TResult, TNext> selector)
    {
        return new EventObservable<TNext>(
            _gatewayService,
            _eventName,
            _predicate,
            CombineSelectors(_selector, selector)
        );
    }

    /// <summary>
    ///     Projects events to a new value, skipping events where the projection throws an exception.
    ///     Unlike Select, exceptions in the selector will not propagate - the event will simply be filtered out.
    /// </summary>
    public EventObservable<TNext> TrySelect<TNext>(Func<TResult, TNext> selector)
    {
        // Add a predicate that evaluates the selector and returns false if it throws
        var safePredicate = new Func<ZyEvent, bool>(evt =>
        {
            try
            {
                // Evaluate existing predicate first
                if (_predicate is not null && !_predicate(evt))
                    return false;

                // Try to evaluate the selector - if it throws, filter out this event
                var intermediateResult = _selector(evt);
                selector(intermediateResult);
                return true;
            }
            catch
            {
                // Swallow exception and filter out this event
                return false;
            }
        });

        return new EventObservable<TNext>(
            _gatewayService,
            _eventName,
            safePredicate,
            CombineSelectors(_selector, selector)
        );
    }

    /// <summary>
    ///     Filters events based on the projected value.
    /// </summary>
    public EventObservable<TResult> Where(Func<TResult, bool> predicate)
    {
        return new EventObservable<TResult>(
            _gatewayService,
            _eventName,
            CombinePredicates(_predicate, evt => predicate(_selector(evt))),
            _selector
        );
    }

    /// <summary>
    ///     Subscribes to projected values matching the current filters.
    /// </summary>
    /// <param name="handler">
    ///     Action to invoke with the projected value when a matching
    ///     event is received.
    /// </param>
    /// <returns>A subscription that can be used to unsubscribe.</returns>
    public Subscription Subscribe(Action<TResult> handler)
    {
        var wrappedHandler = new Action<ZyEvent>(evt =>
        {
            // Exceptions in predicate or selector will propagate to the caller
            if (_predicate is not null && !_predicate(evt))
                return;

            var result = _selector(evt);

            handler(result);
        });

        return _gatewayService.SubscribeToEvent(_eventName, wrappedHandler);
    }

    /// <summary>
    ///     Waits for the first projected value matching the current filters.
    /// </summary>
    public Task<TResult> TakeFirstAsync(CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Wrap the entire subscription to catch exceptions from predicate/selector evaluation
        Subscription? subscription = null;

        subscription = _gatewayService.SubscribeToEvent(
            _eventName,
            evt =>
            {
                try
                {
                    // Evaluate predicate - may throw
                    if (_predicate is not null && !_predicate(evt))
                        return;

                    // Evaluate selector - may throw
                    var result = _selector(evt);

                    // Set the result
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    // Capture any exception from predicate or selector
                    tcs.TrySetException(ex);
                }
            }
        );

        var cancellationRegistration = cancellationToken.CanBeCanceled
            ? cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled(cancellationToken);
            })
            : default;

        tcs.Task.ContinueWith(
            _ =>
            {
                subscription.Unsubscribe();
                cancellationRegistration.Dispose();
            },
            CancellationToken.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default
        );

        return tcs.Task;
    }

    private static Func<ZyEvent, bool> CombinePredicates(
        Func<ZyEvent, bool>? predicateBase,
        Func<ZyEvent, bool> predicateIncoming
    )
    {
        return predicateBase is null ? predicateIncoming : evt => predicateBase(evt) && predicateIncoming(evt);
    }

    private static Func<ZyEvent, TNext> CombineSelectors<TNext>(
        Func<ZyEvent, TResult> selectorBase,
        Func<TResult, TNext> selectorIncoming
    )
    {
        return evt => selectorIncoming(selectorBase(evt));
    }
}

public class EventObservable(GatewayService gatewayService, string eventName, Func<ZyEvent, bool>? predicate = null)
    : EventObservable<ZyEvent>(gatewayService, eventName, predicate, evt => evt);
