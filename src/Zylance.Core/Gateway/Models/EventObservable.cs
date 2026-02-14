using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Zylance.Core.Gateway.Services;

namespace Zylance.Core.Gateway.Models;

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

    /// <summary>
    ///     Subscribes an observer to this observable.
    /// </summary>
    public IDisposable Subscribe(IObserver<TResult> observer)
    {
        return _observable.Subscribe(observer);
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
    ///     Projects events to a new value, skipping events where the projection throws
    ///     an exception.
    ///     Unlike Select, exceptions in the selector will not propagate - the event
    ///     will be filtered out.
    /// </summary>
    public EventObservable<TNext> TrySelect<TNext>(Func<TResult, TNext> selector)
    {
        var observable = _observable
            .Select(result =>
            {
                try
                {
                    return (IsSuccess: true, Value: (TNext?)selector(result));
                }
                catch (Exception)
                {
                    return (IsSuccess: false, Value: default);
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
            return await _observable.FirstAsync().ToTask();

        // Use Rx's Amb to properly handle cancellation
        var cancellationObservable = Observable.Create<TResult>(observer =>
        {
            var registration = cancellationToken.Register(() =>
            {
                observer.OnError(new TaskCanceledException());
            });
            return registration;
        });

        return await _observable.FirstAsync().Amb(cancellationObservable).ToTask();
    }
}

/// <summary>
///     Observable helper that listens to gateway events by name.
/// </summary>
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
