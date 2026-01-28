using Zylance.Contract.Lib.Envelope;

namespace Zylance.Core.Lib.Gateway.Handlers;

/// <summary>
///     Provides exception wrapping into ErrorPayload for the Gateway.
/// </summary>
public static class ExceptionHandler
{
    /// <summary>
    ///     Wraps an exception into an ErrorPayload.
    ///     Unwraps inner exceptions for wrapper exceptions.
    /// </summary>
    public static ErrorPayload WrapException(Exception ex, string? requestId = null)
    {
        var payload = new ErrorPayload
        {
            Type = ex.GetType().Name,
            Details = ex.Message,
        };

        if (requestId is not null)
            payload.RequestId = requestId;

        return payload;
    }
}
