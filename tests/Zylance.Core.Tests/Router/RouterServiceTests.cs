using Zylance.Contract;
using Zylance.Contract.Api.Echo;
using Zylance.Core.Gateway.Services;
using Zylance.Core.Gateway.Utils;
using Zylance.Core.Router.Services;
using Zylance.Core.Tests.TestUtils.Mocks;

namespace Zylance.Core.Tests.Router;

public class RouterServiceTests
{
    private readonly RouterService _router;
    private readonly TestTransport _testTransport;

    public RouterServiceTests()
    {
        _testTransport = new TestTransport();
        var gateway = new GatewayService(_testTransport);
        _router = new RouterService(gateway);
    }

    [Fact]
    public async Task HandleRequest_NormalFlow_HandlerCalled_ResponseSent()
    {
        // Arrange
        var handlerCalled = false;
        _router.Use(
            ZylanceActions.Echo_EchoMessage,
            RequestHandlerUtils.WrapSync<EchoReq, EchoRes>(
                (_, _) =>
                {
                    handlerCalled = true;
                }
            )
        );
        var requestId = Guid.NewGuid();
        var request = MessageUtils.ToRequestPayload(requestId, new EchoReq { Message = "Hello" });

        // Act
        await _testTransport.WaitForMessage(() =>
        {
            _testTransport.SendToGateway(request);
        });

        // Assert
        Assert.True(handlerCalled);
    }

    [Fact]
    public async Task HandleRequest_WhenAnErrorOccurs_IsCaughtAndSendsError()
    {
        // Arrange
        _router.Use(
            ZylanceActions.Echo_EchoMessage,
            RequestHandlerUtils.WrapSync<EchoReq, EchoRes>((_, _) => throw new Exception("Handler error"))
        );
        var requestId = Guid.NewGuid();
        var request = MessageUtils.ToRequestPayload(requestId, new EchoReq { Message = "Hello" });

        // Act
        var message = await _testTransport.WaitForMessage(() =>
        {
            _testTransport.SendToGateway(request);
        });

        // Assert
        Assert.True(message.Error is not null);
        Assert.Equal(requestId.ToString(), message.Error.RequestId);
        Assert.Equal("Handler error", message.Error.Details);
    }
}
