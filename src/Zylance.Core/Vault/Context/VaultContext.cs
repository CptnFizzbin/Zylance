using Serilog;
using Zylance.Contract.Api.Vault;
using Zylance.Core.Gateway.Utils;
using Zylance.Core.Logging;
using Zylance.Core.Vault.Interfaces;

namespace Zylance.Core.Vault.Context;

/// <summary>
///     Manages the active vault state and handles vault state transitions.
///     Implements a state machine pattern for vault lifecycle events.
/// </summary>
public class VaultContext(ZylanceCore zylanceCore)
{
    private static readonly ILogger Log = ZyLogger.CreateLogger<VaultContext>();

    /// <summary>
    ///     Gets or sets the currently active vault.
    ///     Setting this property triggers appropriate vault lifecycle events.
    /// </summary>
    public IVault? ActiveVault
    {
        get;
        set
        {
            var oldVault = field;
            var transition = DetermineTransition(oldVault, value);
            field = value;

            // Dispose the previous vault when it's closed or switched to a different vault
            if (transition == VaultTransition.Closed || transition == VaultTransition.Switched)
                if (oldVault is IAsyncDisposable asyncDisposable)
                    asyncDisposable.DisposeAsync().AsTask().Wait();

            HandleTransition(transition, value);
        }
    }

    /// <summary>
    ///     Gets the currently active vault or throws if none is set.
    /// </summary>
    public IVault ActiveVaultOrThrow => ActiveVault ?? throw new InvalidOperationException("No active vault.");

    private static VaultTransition DetermineTransition(IVault? oldVault, IVault? newVault)
    {
        return (Old: oldVault, New: newVault) switch
        {
            (not null, null) => VaultTransition.Closed,
            (null, not null) => VaultTransition.Opened,
            ({ } old, { } @new) when old.VaultId != @new.VaultId => VaultTransition.Switched,
            ({ Locked: true }, { Locked: false }) => VaultTransition.Unlocked,
            ({ Locked: false }, { Locked: true }) => VaultTransition.Locked,
            _ => VaultTransition.None,
        };
    }

    private void HandleTransition(VaultTransition transition, IVault? vault)
    {
        switch (transition)
        {
            case VaultTransition.None:
                break;

            case VaultTransition.Closed:
                SendVaultClosedEvent();
                break;

            case VaultTransition.Opened:
                SendVaultOpenedEvent(vault!);
                break;

            case VaultTransition.Switched:
                SendVaultClosedEvent();
                SendVaultOpenedEvent(vault!);
                break;

            case VaultTransition.Locked:
                SendVaultLockedEvent(vault!);
                break;

            case VaultTransition.Unlocked:
                SendVaultUnlockedEvent(vault!);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(transition), transition, null);
        }
    }

    private void SendVaultOpenedEvent(IVault vault)
    {
        var evt = new VaultOpenedEvt { VaultRef = vault.ToRef() };
        zylanceCore.Gateway.Send(MessageUtils.ToEventPayload(evt));
    }

    private void SendVaultClosedEvent()
    {
        Log.Information("Transitioning vault to closed state");
        var evt = new VaultClosedEvt();
        zylanceCore.Gateway.Send(MessageUtils.ToEventPayload(evt));
    }

    private void SendVaultLockedEvent(IVault vault)
    {
        Log.Information("Transitioning vault to locked state");
        var evt = new VaultLockedEvt { VaultRef = vault.ToRef() };
        zylanceCore.Gateway.Send(MessageUtils.ToEventPayload(evt));
    }

    private void SendVaultUnlockedEvent(IVault vault)
    {
        Log.Information("Transitioning vault to unlocked state");
        var evt = new VaultUnlockedEvt { VaultRef = vault.ToRef() };
        zylanceCore.Gateway.Send(MessageUtils.ToEventPayload(evt));
    }

    private enum VaultTransition
    {
        None,
        Opened,
        Closed,
        Switched,
        Locked,
        Unlocked,
    }
}
