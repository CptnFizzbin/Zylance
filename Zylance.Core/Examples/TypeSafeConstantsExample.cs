using Zylance.Contract;
using Zylance.Contract.Api.Vault;
using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Lib.Gateway.Utils;

namespace Zylance.Core.Examples;

/// <summary>
/// Example demonstrating usage of type-safe action and event name constants.
/// </summary>
/// <remarks>
/// This is a reference example showing how to use ZylanceConstants.Actions and ZylanceConstants.Events
/// for type-safe action/event name handling. In practice, controllers use [RequestHandler] and [EventHandler]
/// attributes which automatically extract names from protobuf messages.
/// </remarks>
public static class TypeSafeConstantsExample
{
    /// <summary>
    /// Example: Using action constants for logging or debugging.
    /// </summary>
    public static void LogActionExample()
    {
        // Use constants instead of hardcoded strings
        var action = ZylanceConstants.Actions.Vault_OpenVault;
        Console.WriteLine($"Processing action: {action}");

        // Type-safe - the constant value is guaranteed to match the proto definition
        if (action == "Vault:OpenVault")
        {
            Console.WriteLine("Matched vault open action");
        }
    }

    /// <summary>
    /// Example: Using event constants when emitting events.
    /// </summary>
    public static EventPayload CreateEventExample()
    {
        // The event name is guaranteed to match the proto definition
        var eventName = ZylanceConstants.Events.Vault_VaultOpened;

        var evt = new VaultOpenedEvt
        { /* ... */
        };

        // In practice, use MessageUtils.ToEventPayload(evt) which extracts the name automatically
        // This example shows manual construction for demonstration purposes
        return new EventPayload { EventName = eventName, DataJson = MessageUtils.ToJson(evt) };
    }

    /// <summary>
    /// Example: Checking if an action matches a known constant.
    /// </summary>
    public static bool IsVaultAction(string action)
    {
        // Use constants for comparison to avoid typos
        return action == ZylanceConstants.Actions.Vault_OpenVault
            || action == ZylanceConstants.Actions.Vault_CreateVault
            || action == ZylanceConstants.Actions.Vault_CloseVault
            || action == ZylanceConstants.Actions.Vault_GetStatus;
    }

    /// <summary>
    /// Example: Using constants in logging or metrics.
    /// </summary>
    public static void MetricsExample(string action, long duration)
    {
        // Constants ensure consistent naming in logs and metrics
        Console.WriteLine($"Action {action} completed in {duration}ms");

        // Could be used with a metrics library
        // Metrics.RecordTiming(ZylanceConstants.Actions.Vault_OpenVault, duration);
    }

    /// <summary>
    /// Example: All available actions and events.
    /// </summary>
    public static void ListAllActionsAndEvents()
    {
        // Access to all action constants
        var actions = new[]
        {
            ZylanceConstants.Actions.Vault_OpenVault,
            ZylanceConstants.Actions.Vault_CreateVault,
            ZylanceConstants.Actions.File_SelectFile,
            ZylanceConstants.Actions.Echo_EchoMessage,
            // ... etc
        };

        // Access to all event constants
        var events = new[]
        {
            ZylanceConstants.Events.Vault_VaultOpened,
            ZylanceConstants.Events.Vault_VaultClosed,
            ZylanceConstants.Events.Background_WorkStart,
            // ... etc
        };

        Console.WriteLine($"Total actions: {actions.Length}");
        Console.WriteLine($"Total events: {events.Length}");
    }
}
