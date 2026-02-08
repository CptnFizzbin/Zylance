using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Zylance.Core.Lib.Gateway.Services;

namespace Zylance.Core.Lib.Gateway.Models;

/// <summary>
///     Typed version of EventObservable that implements IObservable pattern.
/// </summary>
public class EventObservable<TResult> : IObservable<TResult>
{
    private readonly IObservable<TResult> _observable;

    internal EventObservable(IObservable<TResult> observable)
    {
        _observable = observable;
    }

    public IDisposable Subscribe(IObserver<TResult> observer)
    {
        return _observable.Subscribe(observer);
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
        var disposable = _observable.Subscribe(handler);
        return new Subscription { Id = Guid.NewGuid(), Unsubscribe = disposable.Dispose };
    }

    /// <summary>
    ///     Projects events to a new value.
    /// </summary>
    public EventObservable<TNext> Select<TNext>(Func<TResult, TNext> selector)
    {
        return new EventObservable<TNext>(_observable.Select(selector));
    }

    /// <summary>
    ///     Filters events based on the projected value.
    /// </summary>
    public EventObservable<TResult> Where(Func<TResult, bool> predicate)
    {
        return new EventObservable<TResult>(_observable.Where(predicate));
    }

    /// <summary>
    ///     Projects events to a new value, skipping events where the projection throws an exception.
    ///     Unlike Select, exceptions in the selector will not propagate - the event will be filtered out.
    /// </summary>
    public EventObservable<TNext> TrySelect<TNext>(Func<TResult, TNext> selector)
    {
        var observable = _observable
            .Select(result =>
            {
                try
                {
                    return new Result<TNext>(true, selector(result));
                }
                catch
                {
                    return new Result<TNext>(false, default);
                }
            })
            .Where(res => res.IsSuccess)
            .Select(res => res.Value!);

        return new EventObservable<TNext>(observable);
    }

    /// <summary>
    ///     Waits for the first projected value matching the current filters.
    /// </summary>
    public async Task<TResult> TakeFirstAsync(CancellationToken cancellationToken = default)
    {
        if (!cancellationToken.CanBeCanceled)
        {
            return await _observable.FirstAsync().ToTask();
        }

        // Use Rx's TakeUntil to properly handle cancellation
        var cancellationObservable = Observable.Create<TResult>(observer =>
        {
            var registration = cancellationToken.Register(() =>
            {
                observer.OnError(new TaskCanceledException());
            });
            return registration;
        });

        try
        {
            return await _observable.FirstAsync().Amb(cancellationObservable).ToTask();
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw new TaskCanceledException();
        }
    }
}

public class EventObservable(GatewayService gatewayService, string eventName)
    : EventObservable<ZyEvent>(CreateObservable(gatewayService, eventName))
{
    private static IObservable<ZyEvent> CreateObservable(GatewayService gatewayService, string eventName)
    {
        return Observable.Create<ZyEvent>(observer =>
        {
            var subscription = gatewayService.SubscribeToEvent(
                eventName,
                evt =>
                {
                    try
                    {
                        observer.OnNext(evt);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                }
            );

            return subscription;
        });
    }
}

internal record Result<T>(bool IsSuccess, T? Value);
