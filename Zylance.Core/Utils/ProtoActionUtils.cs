using Google.Protobuf;
using Google.Protobuf.Reflection;
using Zylance.Contract.Extensions;

namespace Zylance.Core.Utils;

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
        var actionValue = customOptions.GetExtension(ActionOptionExtensions.Action);
        return string.IsNullOrEmpty(actionValue)
            ? null
            : actionValue;
    }
}
