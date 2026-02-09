namespace Zylance.Core.Lib.Gateway.Models;

public record Subscription : IDisposable
{
    public required Guid Id { get; init; }
    public required Action Unsubscribe { get; init; }

    public void Dispose()
    {
        Unsubscribe();
        GC.SuppressFinalize(this);
    }
}
