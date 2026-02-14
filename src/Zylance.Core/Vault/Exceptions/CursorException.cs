namespace Zylance.Core.Vault.Exceptions;

/// <summary>
///     Exception thrown when a cursor-based operation is invalid, for example
///     when attempting to fetch a next/previous page that does not exist.
/// </summary>
public class CursorException(string message) : Exception(message);
