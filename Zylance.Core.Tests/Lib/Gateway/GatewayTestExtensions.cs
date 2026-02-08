using System.Reflection;
using Google.Protobuf;
using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Lib.Gateway.Services;
using Zylance.Core.Lib.Gateway.Utils;
using Zylance.Core.Tests.Mocks;

namespace Zylance.Core.Tests.Lib.Gateway;

/// <summary>
///     Test extensions for the Gateway to simulate events during testing.
/// </summary>
public static class GatewayTestExtensions
{
    /// <summary>
    ///     Retrieves the underlying MockTransport from the gateway using reflection.
    /// </summary>
    /// <remarks>
    ///     This is necessary because the transport field is private and managed
    ///     internally.
    ///     The test helper uses reflection to inject test events into the gateway's
    ///     message handling pipeline.
    /// </remarks>
    private static MockTransport GetMockTransport(GatewayService gatewayService)
    {
        var transportField = typeof(GatewayService).GetField(
            "_transport",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (transportField is null)
            throw new InvalidOperationException(
                $"Could not find _transport field on {nameof(GatewayService)}. This method requires access to the transport layer."
            );

        var transport = transportField.GetValue(gatewayService);

        if (transport is not MockTransport mockTransport)
            throw new InvalidOperationException(
                $"Expected MockTransport but found {transport?.GetType().Name ?? "null"}. "
                    + "This extension can only be used with MockTransport in tests."
            );

        return mockTransport;
    }

    /// <param name="gatewayService">The gateway instance.</param>
    extension(GatewayService gatewayService)
    {
        /// <summary>
        ///     Triggers an event on the gateway by simulating it coming from the transport
        ///     layer.
        /// </summary>
        /// <param name="eventName">The name of the event to trigger.</param>
        /// <param name="eventData">The event data (optional).</param>
        /// <remarks>
        ///     This extension is designed for testing purposes. It uses reflection to
        ///     access
        ///     the private _transport field to inject simulated events into the gateway.
        /// </remarks>
        public void TriggerEvent(string eventName, IMessage? eventData = null)
        {
            var dataJson = eventData is not null ? MessageUtils.ToJson(eventData) : "";

            var eventPayload = new EventPayload { EventName = eventName, DataJson = dataJson };
            var envelope = new GatewayEnvelope { Event = eventPayload };

            var mockTransport = GetMockTransport(gatewayService);
            mockTransport.SendToGateway(MessageUtils.ToJson(envelope));
        }

        /// <summary>
        ///     Triggers an event on the gateway with untyped JSON data.
        /// </summary>
        /// <param name="eventName">The name of the event to trigger.</param>
        /// <param name="dataJson">The raw JSON data for the event.</param>
        public void TriggerEvent(string eventName, string dataJson)
        {
            var eventPayload = new EventPayload { EventName = eventName, DataJson = dataJson };
            var envelope = new GatewayEnvelope { Event = eventPayload };

            var mockTransport = GetMockTransport(gatewayService);
            mockTransport.SendToGateway(MessageUtils.ToJson(envelope));
        }
    }
}
