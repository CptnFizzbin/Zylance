using Google.Protobuf;
using Google.Protobuf.Reflection;
using Zylance.Contract.Extensions;

namespace Zylance.Core.Controllers.Utils;

/// <summary>
///     Utility for extracting action names from protobuf messages using custom options.
/// </summary>
public static class ProtoActionUtils
{
    /// <summary>
    ///     Gets the action name from a protobuf message type using the custom [action] option.
    /// </summary>
    /// <typeparam name="TMessage">The protobuf message type (must implement IMessage)</typeparam>
    /// <returns>The action name if specified, otherwise null</returns>
    public static string? GetAction<TMessage>() where TMessage : IMessage, new()
    {
        var instance = new TMessage();
        var descriptor = instance.Descriptor;
        return GetActionFromDescriptor(descriptor);
    }

    /// <summary>
    ///     Gets the action name from a protobuf message descriptor.
    /// </summary>
    private static string? GetActionFromDescriptor(MessageDescriptor descriptor)
    {
        var customOptions = descriptor.GetOptions();
        if (customOptions == null) return null;

        // Use the generated extension to get the action value
        var actionValue = customOptions.GetExtension(ZylanceExtensions.Action);
        return string.IsNullOrEmpty(actionValue)
            ? null
            : actionValue;
    }

    public static string? GetEventName<TMessage>() where TMessage : IMessage, new()
    {
        var instance = new TMessage();
        var descriptor = instance.Descriptor;
        return GetEventNameFromDescriptor(descriptor);
    }

    /// <summary>
    ///     Gets the action name from a protobuf message descriptor.
    /// </summary>
    private static string? GetEventNameFromDescriptor(MessageDescriptor descriptor)
    {
        var customOptions = descriptor.GetOptions();
        if (customOptions == null) return null;

        // Use the generated extension to get the action value
        var actionValue = customOptions.GetExtension(ZylanceExtensions.EventName);
        return string.IsNullOrEmpty(actionValue)
            ? null
            : actionValue;
    }
}
