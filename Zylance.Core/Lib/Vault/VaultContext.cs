using Zylance.Contract.Api.Vault;
using Zylance.Core.Lib.Gateway.Utils;

namespace Zylance.Core.Lib.Vault;

/// <summary>
///     Manages the active vault state and handles vault state transitions.
///     Implements a state machine pattern for vault lifecycle events.
/// </summary>
public class VaultContext(Gateway.Gateway gateway)
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
            // No change - both null
            (null, null) => VaultTransition.None,

            // No change - same vault with same lock state
            ({ } old, { } @new) when old.VaultId == @new.VaultId && old.Unlocked == @new.Unlocked =>
                VaultTransition.None,

            // Vault closed
            (not null, null) => VaultTransition.Closed,

            // Vault opened
            (null, { } vault) => vault.Unlocked ? VaultTransition.OpenedUnlocked : VaultTransition.Opened,

            // Vault switched
            ({ } old, { } @new) when old.VaultId != @new.VaultId => @new.Unlocked
                ? VaultTransition.SwitchedUnlocked
                : VaultTransition.Switched,

            // Lock state changed
            ({ Unlocked: false }, { Unlocked: true }) => VaultTransition.Unlocked,
            ({ Unlocked: true }, { Unlocked: false }) => VaultTransition.Locked,

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

            case VaultTransition.OpenedUnlocked:
                SendVaultOpenedEvent(vault!);
                SendVaultUnlockedEvent(vault!);
                break;

            case VaultTransition.Switched:
                SendVaultClosedEvent();
                SendVaultOpenedEvent(vault!);
                break;

            case VaultTransition.SwitchedUnlocked:
                SendVaultClosedEvent();
                SendVaultOpenedEvent(vault!);
                SendVaultUnlockedEvent(vault!);
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
        gateway.Send(MessageUtils.ToEventPayload(evt));
    }

    private void SendVaultClosedEvent()
    {
        var evt = new VaultClosedEvt();
        gateway.Send(MessageUtils.ToEventPayload(evt));
    }

    private void SendVaultLockedEvent(IVault vault)
    {
        var evt = new VaultLockedEvt { VaultRef = vault.ToRef() };
        gateway.Send(MessageUtils.ToEventPayload(evt));
    }

    private void SendVaultUnlockedEvent(IVault vault)
    {
        var evt = new VaultUnlockedEvt { VaultRef = vault.ToRef() };
        gateway.Send(MessageUtils.ToEventPayload(evt));
    }

    /// <summary>
    ///     Represents the possible state transitions for a vault.
    /// </summary>
    private enum VaultTransition
    {
        None,
        Opened,
        OpenedUnlocked,
        Closed,
        Switched,
        SwitchedUnlocked,
        Locked,
        Unlocked,
    }
}
