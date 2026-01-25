using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Zylance.Core.Controllers.Attributes;
using Zylance.Core.Models;

namespace Zylance.Core.Controllers.Extensions;

public static class RouterExtensions
{
    extension(MethodInfo methodInfo)
    {
        internal bool TryGetRequestHandlerAttribute([NotNullWhen(true)] out RequestHandlerAttribute? attribute)
        {
            attribute = methodInfo.GetCustomAttribute<RequestHandlerAttribute>(true);
            return attribute is not null;
        }

        internal bool TryGetEventHandlerAttribute([NotNullWhen(true)] out EventHandlerAttribute? attribute)
        {
            attribute = methodInfo.GetCustomAttribute<EventHandlerAttribute>(true);
            return attribute is not null;
        }

        internal bool IsRequestHandler()
        {
            var parameters = methodInfo.GetParameters();

            if (parameters.Length != 2)
                return false;

            var reqParam = parameters[0];
            var resParam = parameters[1];

            var isReqValid = reqParam.ParameterType.IsGenericType
                && reqParam.ParameterType.GetGenericTypeDefinition() == typeof(ZyRequest<>);
            var isResValid = resParam.ParameterType.IsGenericType
                && resParam.ParameterType.GetGenericTypeDefinition() == typeof(ZyResponse<>);

            return isReqValid && isResValid;
        }

        internal bool IsEventHandler()
        {
            var parameters = methodInfo.GetParameters();

            if (parameters.Length != 1)
                return false;

            var evtParam = parameters[0];

            return evtParam.ParameterType.IsGenericType
                && evtParam.ParameterType.GetGenericTypeDefinition() == typeof(ZyEvent<>);
        }
    }
}
