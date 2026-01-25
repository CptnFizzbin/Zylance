using Zylance.Core.Models;

namespace Zylance.Core.Delegates;

/// <summary>
///     Async request handler that processes a request and returns a response.
///     This is the primary handler type - prefer this over sync variants.
/// </summary>
public delegate Task<ZyResponse> AsyncZyRequestHandler(ZyRequest req, ZyResponse res);

/// <summary>
///     Async typed request handler for strongly-typed request/response pairs.
///     Handlers should mutate the response parameter and return void or Task.
/// </summary>
public delegate Task AsyncZyRequestHandler<TReq, TRes>(ZyRequest<TReq> req, ZyResponse<TRes> res);

/// <summary>
///     Sync typed request handler for strongly-typed request/response pairs.
///     Handlers should mutate the response parameter and return void.
/// </summary>
public delegate void SyncZyRequestHandler<TReq, TRes>(ZyRequest<TReq> req, ZyResponse<TRes> res);
