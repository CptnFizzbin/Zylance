using Zylance.Contract.Api.Vault;
using Zylance.Core.Lib.Gateway.Utils;

namespace Zylance.Core.Lib.Vault;

/// <summary>
///     Manages the active vault state and handles vault state transitions.
///     Implements a state machine pattern for vault lifecycle events.
/// </summary>
public class VaultContext(Zylance zylance)
{
    /// <summary>
    ///     Gets or sets the currently active vault.
    ///     Setting this property triggers appropriate vault lifecycle events.
    /// </summary>
    public IVault? ActiveVault
    {
        get;
        set
        {
            var transition = DetermineTransition(field, value);
            field = value;
            HandleTransition(transition, value);
        }
    }

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
        zylance.Gateway.Send(MessageUtils.ToEventPayload(evt));
    }

    private void SendVaultClosedEvent()
    {
        var evt = new VaultClosedEvt();
        zylance.Gateway.Send(MessageUtils.ToEventPayload(evt));
    }

    private void SendVaultLockedEvent(IVault vault)
    {
        var evt = new VaultLockedEvt { VaultRef = vault.ToRef() };
        zylance.Gateway.Send(MessageUtils.ToEventPayload(evt));
    }

    private void SendVaultUnlockedEvent(IVault vault)
    {
        var evt = new VaultUnlockedEvt { VaultRef = vault.ToRef() };
        zylance.Gateway.Send(MessageUtils.ToEventPayload(evt));
    }

    /// <summary>
    ///     Represents the possible state transitions for a vault.
    /// </summary>
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
