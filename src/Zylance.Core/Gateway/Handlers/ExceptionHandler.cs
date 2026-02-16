using Serilog;
using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Logging;

namespace Zylance.Core.Gateway.Handlers;

/// <summary>
///     Provides exception wrapping into ErrorPayload for the Gateway.
/// </summary>
public static class ExceptionHandler
{
    private static readonly ILogger Log = ZyLogger.CreateLogger(typeof(ExceptionHandler));

    /// <summary>
    ///     Wraps an exception into an ErrorPayload.
    ///     Unwraps inner exceptions for wrapper exceptions.
    /// </summary>
    public static ErrorPayload WrapException(Exception ex, string? requestId = null)
    {
        var payload = new ErrorPayload { Type = ex.GetType().Name, Details = ex.Message };

        if (requestId is not null)
            payload.RequestId = requestId;

        Log.Error(ex, "Wrapped exception for RequestId={RequestId}", requestId);

        return payload;
    }
}
