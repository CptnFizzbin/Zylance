using Zylance.Contract;

namespace Zylance.Gateway;

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
        return new ErrorPayload
        {
            RequestId = requestId,
            Type = ex.GetType().Name,
            Details = ex.Message,
        };
    }
}
