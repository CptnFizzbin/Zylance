using Zylance.Core.Controllers.Models;

namespace Zylance.Core.Controllers.Delegates;

/// <summary>
///     Async event handler that processes an event.
///     Prefer async handlers for consistency with modern C# patterns.
/// </summary>
public delegate Task AsyncZyEventHandler(ZyEvent evt);

/// <summary>
///     Async typed event handler for strongly-typed events.
/// </summary>
public delegate Task AsyncZyEventHandler<TData>(ZyEvent<TData> evt);

/// <summary>
///     Sync typed event handler for strongly-typed events.
/// </summary>
public delegate void SyncZyEventHandler<TData>(ZyEvent<TData> evt);
