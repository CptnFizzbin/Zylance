using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Zylance.Core.Attributes;
using Zylance.Core.Delegates;
using Zylance.Core.Models;
using Zylance.Core.Utils;
using static System.Reflection.BindingFlags;

namespace Zylance.Core.Interfaces;

public class RequestRouter
{
    private readonly Dictionary<string, AsyncZyRequestHandler> _handlers = [];

    /// <summary>
    ///     Registers an async request handler for the specified action.
    /// </summary>
    public RequestRouter Use(string action, AsyncZyRequestHandler handler)
    {
        _handlers.Add(action, handler);
        return this;
    }

    /// <summary>
    ///     Automatically discovers and registers all methods marked with [RequestHandler] attribute
    ///     from the specified controller instance.
    /// </summary>
    public RequestRouter UseController<
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)]
        TController>(TController controller) where TController : notnull
    {
        var controllerType = typeof(TController);
        Console.WriteLine($"[RequestRouter] UseController called for {controllerType.Name}");
        var methods = controllerType.GetMethods(Default | Instance | Public | NonPublic);
        Console.WriteLine($"[RequestRouter] Found {methods.Length} total methods in {controllerType.Name}");

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<RequestHandlerAttribute>();
            if (attribute == null) continue;

            var parameters = method.GetParameters();
            if (IsTypedRequestResponseHandler(method, parameters))
                RegisterTypedHandler(controller, method, attribute.Action);
            else
                throw new InvalidOperationException(
                    $"Method {method.Name} has [RequestHandler] attribute but doesn't match any supported signature. "
                    + $"Expected: Task<ZyResponse<TRes>>(ZyRequest<TReq>, ZyResponse<TRes>), Task(ZyRequest<TReq>), or Task<ZyResponse>(ZyRequest, ZyResponse)");
        }

        return this;
    }

    private bool IsTypedRequestResponseHandler(MethodInfo method, ParameterInfo[] parameters)
    {
        if (parameters.Length != 2) return false;
        if (!method.ReturnType.IsGenericType) return false;

        var returnType = method.ReturnType.GetGenericTypeDefinition();
        if (returnType != typeof(Task<>)) return false;

        var taskResultType = method.ReturnType.GetGenericArguments()[0];
        if (!taskResultType.IsGenericType) return false;
        if (taskResultType.GetGenericTypeDefinition() != typeof(ZyResponse<>)) return false;

        var param1Type = parameters[0].ParameterType;
        var param2Type = parameters[1].ParameterType;

        return param1Type.IsGenericType
            && param1Type.GetGenericTypeDefinition() == typeof(ZyRequest<>)
            && param2Type.IsGenericType
            && param2Type.GetGenericTypeDefinition() == typeof(ZyResponse<>);
    }

    private void RegisterTypedHandler(object controller, MethodInfo method, string? action)
    {
        var parameters = method.GetParameters();
        var requestType = parameters[0].ParameterType.GetGenericArguments()[0];
        var responseType = parameters[1].ParameterType.GetGenericArguments()[0];

        if (string.IsNullOrEmpty(action))
            action = ResolveActionFromTypes(requestType, responseType, method);

        var delegateType = typeof(AsyncZyRequestHandler<,>).MakeGenericType(requestType, responseType);
        var handlerDelegate = Delegate.CreateDelegate(delegateType, controller, method);

        var wrapMethod = typeof(RequestHandlerUtils).GetMethod(nameof(RequestHandlerUtils.Wrap))!
            .MakeGenericMethod(requestType, responseType);
        var wrappedHandler = (AsyncZyRequestHandler)wrapMethod.Invoke(null, [handlerDelegate])!;

        Use(action, wrappedHandler);
    }

    private string ResolveActionFromTypes(
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
            $"[RequestRouter] Auto-detected action '{reqAction}' from {requestType.Name}/{responseType.Name}");
        return reqAction;
    }

    public async Task<ZyResponse> MessageReceived(ZyRequest zyRequest, ZyResponse zyResponse)
    {
        if (_handlers.TryGetValue(zyRequest.Action, out var handler))
            return await handler(zyRequest, zyResponse);

        return zyResponse;
    }
}
