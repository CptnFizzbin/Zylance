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
    ///     Registers a strongly-typed async request handler for the specified action.
    ///     The handler will be wrapped to work with the generic handler interface.
    /// </summary>
    public RequestRouter Use<TReq, TRes>(string action, AsyncZyRequestHandler<TReq, TRes> handler)
    {
        return Use(action, RequestHandlerUtils.Wrap(handler));
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
        var methods = controllerType.GetMethods(Default | Instance | Public | NonPublic);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<RequestHandlerAttribute>();
            if (attribute == null) continue;

            var parameters = method.GetParameters();

            // Determine handler type based on method signature
            if (IsTypedRequestResponseHandler(method, parameters))
                RegisterTypedHandler(controller, method, attribute.Action);
            else if (IsGenericHandler(method, parameters))
                RegisterGenericHandler(controller, method, attribute.Action);
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

    private bool IsActionOnlyHandler(MethodInfo method, ParameterInfo[] parameters)
    {
        if (parameters.Length != 1) return false;
        if (method.ReturnType != typeof(Task)) return false;

        var param1Type = parameters[0].ParameterType;
        return param1Type.IsGenericType && param1Type.GetGenericTypeDefinition() == typeof(ZyRequest<>);
    }

    private bool IsGenericHandler(MethodInfo method, ParameterInfo[] parameters)
    {
        if (parameters.Length != 2) return false;
        if (method.ReturnType != typeof(Task<ZyResponse>)) return false;

        return parameters[0].ParameterType == typeof(ZyRequest) && parameters[1].ParameterType == typeof(ZyResponse);
    }

    private void RegisterTypedHandler(object controller, MethodInfo method, string action)
    {
        var parameters = method.GetParameters();
        var requestType = parameters[0].ParameterType.GetGenericArguments()[0];
        var responseType = parameters[1].ParameterType.GetGenericArguments()[0];

        // Create AsyncZyRequestHandler<TReq, TRes> delegate
        var delegateType = typeof(AsyncZyRequestHandler<,>).MakeGenericType(requestType, responseType);
        var handlerDelegate = Delegate.CreateDelegate(delegateType, controller, method);

        // Call Wrap<TReq, TRes>
        var wrapMethod = typeof(RequestHandlerUtils).GetMethod(nameof(RequestHandlerUtils.Wrap))!
            .MakeGenericMethod(requestType, responseType);
        var wrappedHandler = (AsyncZyRequestHandler)wrapMethod.Invoke(null, new[] { handlerDelegate })!;

        Use(action, wrappedHandler);
    }

    private void RegisterGenericHandler(object controller, MethodInfo method, string action)
    {
        var handler = (AsyncZyRequestHandler)Delegate.CreateDelegate(typeof(AsyncZyRequestHandler), controller, method);
        Use(action, handler);
    }

    public async Task<ZyResponse> MessageReceived(ZyRequest zyRequest, ZyResponse zyResponse)
    {
        if (_handlers.TryGetValue(zyRequest.Action, out var handler))
            return await handler(zyRequest, zyResponse);

        return zyResponse;
    }
}
