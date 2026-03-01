using JetBrains.Annotations;
using Serilog;
using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Gateway.Handlers;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Gateway.Services;

namespace Zylance.Core.Router.Services;

/// <summary>
///     Lightweight routing service used by the gateway to map actions and event
///     names to handler delegates.
///     Generated registration code calls into this service to register
///     controllers.
/// </summary>
public class RouterService
{
    private readonly Dictionary<string, AsyncZyEventHandler> _eventHandlers = [];
    private readonly GatewayService _gateway;
    private readonly Dictionary<string, AsyncZyRequestHandler> _requestHandlers = [];

    /// <summary>
    ///     Lightweight routing service used by the gateway to map actions and event
    ///     names to handler delegates.
    ///     Generated registration code calls into this service to register
    ///     controllers.
    /// </summary>
    public RouterService(GatewayService gateway)
    {
        _gateway = gateway;

        _gateway
            .ObserveRequests()
            .Subscribe(
                async void (req) =>
                {
                    try
                    {
                        await HandleRequest(req);
                    }
                    catch (Exception e)
                    {
                        gateway.HandleError(e, req.RequestId);
                    }
                }
            );

        _gateway
            .ObserveEvents()
            .Subscribe(
                async void (evt) =>
                {
                    try
                    {
                        await HandleEvent(evt);
                    }
                    catch (Exception e)
                    {
                        gateway.HandleError(e);
                    }
                }
            );
    }

    /// <summary>
    ///     Registers an async request handler for the specified action.
    /// </summary>
    [UsedImplicitly(Reason = "Called by generated code.")]
    public RouterService Use(string action, AsyncZyRequestHandler handler)
    {
        Log.Information("Registering event handler for action {action}", action);
        _requestHandlers.Add(action, handler);
        return this;
    }

    /// <summary>
    ///     Registers an async request handler for the specified action.
    /// </summary>
    [UsedImplicitly(Reason = "Called by generated code.")]
    public RouterService Use(string eventName, AsyncZyEventHandler handler)
    {
        Log.Information("Registering event handler for event {EventName}", eventName);
        _eventHandlers.Add(eventName, handler);
        return this;
    }

    private async Task HandleRequest(RequestPayload reqPayload)
    {
        var zyRequest = new ZyRequest { Payload = reqPayload };

        var resPayload = new ResponsePayload { RequestId = reqPayload.RequestId };
        var zyResponse = new ZyResponse { Payload = resPayload, OnSend = res => _gateway.Send(res.Payload) };

        if (_requestHandlers.TryGetValue(zyRequest.Action, out var handler))
            await handler(zyRequest, zyResponse);

        if (!zyResponse.ResponseSent)
            zyResponse.Send();
    }

    private async Task HandleEvent(EventPayload evtPayload)
    {
        var zyEvent = new ZyEvent { Payload = evtPayload };

        if (_eventHandlers.TryGetValue(zyEvent.Name, out var handler))
            await handler(zyEvent);
    }
}
