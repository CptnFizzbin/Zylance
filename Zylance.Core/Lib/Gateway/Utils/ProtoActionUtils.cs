using Google.Protobuf;
using Google.Protobuf.Reflection;
using Zylance.Contract.Extensions;

namespace Zylance.Core.Lib.Gateway.Utils;

/// <summary>
///     Utility for extracting action names from protobuf messages using custom options.
/// </summary>
public static class ProtoActionUtils
{
    /// <summary>
    ///     Gets the action name from a protobuf message type using the custom [action] option.
    /// </summary>
    /// <typeparam name="TReqData">The protobuf message type (must implement IMessage)</typeparam>
    /// <returns>The action name if specified, otherwise null</returns>
    public static string GetAction<TReqData>()
        where TReqData : IMessage, new()
    {
        return GetAction(new TReqData());
    }

    public static string GetAction<TReqData>(TReqData reqData)
        where TReqData : IMessage, new()
    {
        var descriptor = reqData.Descriptor;
        return GetActionFromDescriptor(descriptor)
            ?? throw new InvalidOperationException($"Action option not found in descriptor for {descriptor.Name}.");
    }

    /// <summary>
    ///     Gets the action name from a protobuf message descriptor.
    /// </summary>
    private static string? GetActionFromDescriptor(MessageDescriptor descriptor)
    {
        var customOptions = descriptor.GetOptions();
        if (customOptions == null)
            return null;

        // Use the generated extension to get the action value
        var actionValue = customOptions.GetExtension(ZylanceExtensions.Action);
        return string.IsNullOrEmpty(actionValue) ? null : actionValue;
    }

    public static string GetEventName<TMessage>()
        where TMessage : IMessage, new()
    {
        return GetEventName(new TMessage());
    }

    public static string GetEventName<TEvtData>(TEvtData eventData)
        where TEvtData : IMessage, new()
    {
        var descriptor = eventData.Descriptor;
        return GetEventNameFromDescriptor(descriptor)
            ?? throw new InvalidOperationException($"EventName option not found in descriptor for {descriptor.Name}.");
    }

    /// <summary>
    ///     Gets the action name from a protobuf message descriptor.
    /// </summary>
    private static string? GetEventNameFromDescriptor(MessageDescriptor descriptor)
    {
        var customOptions = descriptor.GetOptions();
        if (customOptions == null)
            return null;

        // Use the generated extension to get the action value
        var eventName = customOptions.GetExtension(ZylanceExtensions.EventName);
        return string.IsNullOrEmpty(eventName) ? null : eventName;
    }
}
