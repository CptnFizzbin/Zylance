using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Zylance.Core.Controllers.Extensions;
using Zylance.Core.Controllers.Utils;
using Zylance.Core.Delegates;
using Zylance.Core.Models;
using Zylance.Core.Utils;
using static System.Reflection.BindingFlags;

namespace Zylance.Core.Controllers.Services;

public class RouterService
{
    private readonly Dictionary<string, AsyncZyEventHandler> _eventHandlers = [];
    private readonly Dictionary<string, AsyncZyRequestHandler> _requestHandlers = [];

    /// <summary>
    ///     Registers an async request handler for the specified action.
    /// </summary>
    public RouterService Use(string action, AsyncZyRequestHandler handler)
    {
        _requestHandlers.Add(action, handler);
        return this;
    }

    /// <summary>
    ///     Registers an async request handler for the specified action.
    /// </summary>
    public RouterService Use(string eventName, AsyncZyEventHandler handler)
    {
        _eventHandlers.Add(eventName, handler);
        return this;
    }

    /// <summary>
    ///     Automatically discovers and registers all methods marked with [RequestHandler] attribute
    ///     from the specified controller instance.
    /// </summary>
    [RequiresUnreferencedCode("This method uses reflection to discover and register controller methods at runtime.")]
    public RouterService UseController<
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)]
        TController>(TController controller) where TController : notnull
    {
        var controllerType = typeof(TController);
        Console.WriteLine($"[RequestRouter] UseController called for {controllerType.Name}");
        var methods = controllerType.GetMethods(Default | Instance | Public | NonPublic);
        Console.WriteLine($"[RequestRouter] Found {methods.Length} total methods in {controllerType.Name}");

        foreach (var method in methods)
            if (method.TryGetRequestHandlerAttribute(out var requestHandlerAttr))
            {
                Console.WriteLine($"[RequestRouter] Found RequestHandler {method.Name}");
                if (method.IsRequestHandler())
                    RegisterRequestHandler(controller, method, requestHandlerAttr.Action);
                else
                    throw new InvalidOperationException(
                        $"Method {method.Name} has [RequestHandler] attribute but doesn't match any supported signature. "
                        + $"Expected: Task(ZyRequest<TReq>, ZyResponse<TRes>) or void(ZyRequest<TReq>, ZyResponse<TRes>)");
            }
            else if (method.TryGetEventHandlerAttribute(out var eventHandlerAttr))
            {
                Console.WriteLine($"[RequestRouter] Found EventHandler {method.Name}");
                if (method.IsEventHandler())
                    RegisterEventHandler(controller, method, eventHandlerAttr.EventName);
                else
                    throw new InvalidOperationException(
                        $"Method {method.Name} has [EventHandler] attribute but doesn't match any supported signature. "
                        + $"Expected: Task(ZyEvent<TEvt>) or void(ZyEvent<TEvt>)");
            }

        return this;
    }

    [RequiresUnreferencedCode("This method uses reflection to discover and invoke controller methods at runtime.")]
    private void RegisterRequestHandler(object controller, MethodInfo method, string? action)
    {
        var parameters = method.GetParameters();
        var requestType = parameters[0].ParameterType.GetGenericArguments()[0];
        var responseType = parameters[1].ParameterType.GetGenericArguments()[0];

        if (string.IsNullOrEmpty(action))
            action = ResolveActionFromTypes(requestType, responseType, method);

        AsyncZyRequestHandler wrappedHandler;

        if (method.ReturnType == typeof(Task))
        {
            // Async handler (returns Task)
            var delegateType = typeof(AsyncZyRequestHandler<,>).MakeGenericType(requestType, responseType);
            var handlerDelegate = Delegate.CreateDelegate(delegateType, controller, method);

            var wrapMethod = typeof(RequestHandlerUtils).GetMethod(nameof(RequestHandlerUtils.Wrap))!
                .MakeGenericMethod(requestType, responseType);
            wrappedHandler = (AsyncZyRequestHandler)wrapMethod.Invoke(null, [handlerDelegate])!;
        }
        else
        {
            // Sync handler (returns void)
            var delegateType = typeof(SyncZyRequestHandler<,>).MakeGenericType(requestType, responseType);
            var handlerDelegate = Delegate.CreateDelegate(delegateType, controller, method);

            var wrapMethod = typeof(RequestHandlerUtils).GetMethod(nameof(RequestHandlerUtils.WrapSync))!
                .MakeGenericMethod(requestType, responseType);
            wrappedHandler = (AsyncZyRequestHandler)wrapMethod.Invoke(null, [handlerDelegate])!;
        }

        Use(action, wrappedHandler);
    }

    [RequiresUnreferencedCode("This method uses reflection to discover and invoke controller methods at runtime.")]
    private void RegisterEventHandler(
        object controller,
        MethodInfo method,
        string? eventName)
    {
        var parameters = method.GetParameters();
        var eventType = parameters[0].ParameterType.GetGenericArguments()[0];

        if (string.IsNullOrEmpty(eventName))
            eventName = ResolveEventFromType(eventType, method);

        AsyncZyEventHandler wrappedHandler;

        if (method.ReturnType == typeof(Task))
        {
            // Async handler (returns Task)
            var delegateType = typeof(AsyncZyEventHandler<>).MakeGenericType(eventType);
            var handlerDelegate = Delegate.CreateDelegate(delegateType, controller, method);

            var wrapMethod = typeof(EventHandlerUtils)
                .GetMethod(nameof(EventHandlerUtils.Wrap))!
                .MakeGenericMethod(eventType);
            wrappedHandler = (AsyncZyEventHandler)wrapMethod.Invoke(null, [handlerDelegate])!;
        }
        else
        {
            // Sync handler (returns void)
            var delegateType = typeof(SyncZyEventHandler<>).MakeGenericType(eventType);
            var handlerDelegate = Delegate.CreateDelegate(delegateType, controller, method);

            var wrapMethod = typeof(EventHandlerUtils)
                .GetMethod(nameof(EventHandlerUtils.WrapSync))!
                .MakeGenericMethod(eventType);
            wrappedHandler = (AsyncZyEventHandler)wrapMethod.Invoke(null, [handlerDelegate])!;
        }

        Use(eventName, wrappedHandler);
    }

    private static string ResolveActionFromTypes(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        Type requestType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        Type responseType,
        MethodInfo method
    )
    {
        var getActionMethod = typeof(ProtoActionUtils).GetMethod(nameof(ProtoActionUtils.GetAction))!;

        var reqActionMethod = getActionMethod.MakeGenericMethod(requestType);
        var resActionMethod = getActionMethod.MakeGenericMethod(responseType);

        var reqAction = reqActionMethod.Invoke(null, null) as string;
        var resAction = resActionMethod.Invoke(null, null) as string;

        if (string.IsNullOrEmpty(reqAction))
            throw new InvalidOperationException(
                $"Cannot auto-detect action for method {method.Name}: "
                + $"Request type {requestType.Name} is missing the (action) option in its .proto definition."
            );

        if (string.IsNullOrEmpty(resAction))
            throw new InvalidOperationException(
                $"Cannot auto-detect action for method {method.Name}: "
                + $"Response type {responseType.Name} is missing the (action) option in its .proto definition."
            );

        if (reqAction != resAction)
            throw new InvalidOperationException(
                $"Action mismatch for method {method.Name}: "
                + $"Request type {requestType.Name} has action '{reqAction}' but Response type {responseType.Name} has action '{resAction}'. "
                + "Both must have the same action name."
            );

        Console.WriteLine(
            $"[RequestRouter] Adding request handler for '{reqAction}' from {requestType.Name}/{responseType.Name}");
        return reqAction;
    }


    [RequiresUnreferencedCode("This method uses reflection with dynamically determined types.")]
    private static string ResolveEventFromType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        Type eventType,
        MethodInfo method
    )
    {
        var getActionMethod = typeof(ProtoActionUtils).GetMethod(nameof(ProtoActionUtils.GetEventName))!;

        var eventTypeMethod = getActionMethod.MakeGenericMethod(eventType);

        var eventAction = eventTypeMethod.Invoke(null, null) as string;

        if (string.IsNullOrEmpty(eventAction))
            throw new InvalidOperationException(
                $"Cannot auto-detect eventName for method {method.Name}: "
                + $"Request type {eventType.Name} is missing the (eventName) option in its .proto definition.");

        Console.WriteLine(
            $"[RequestRouter] Adding event handler for '{eventAction}' from {eventType.Name}");
        return eventAction;
    }

    public async Task<ZyResponse> HandleRequest(ZyRequest zyRequest, ZyResponse zyResponse)
    {
        if (_requestHandlers.TryGetValue(zyRequest.Action, out var handler))
            return await handler(zyRequest, zyResponse);

        return zyResponse;
    }

    public async Task HandleEvent(ZyEvent zyEvent)
    {
        if (_eventHandlers.TryGetValue(zyEvent.Name, out var handler))
            await handler(zyEvent);
    }
}
