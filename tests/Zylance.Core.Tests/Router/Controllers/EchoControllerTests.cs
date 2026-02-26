using Zylance.Contract.Api.Echo;
using Zylance.Core.Router.Controllers;
using Zylance.Core.Tests.TestUtils.Factories;
using Zylance.Core.Tests.TestUtils.Factories.Models;

namespace Zylance.Core.Tests.Router.Controllers;

public class EchoControllerTests
{
    private readonly EchoController _controller = new();

    [Theory]
    [InlineData("Hello, world!")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Special chars: !@#$%^&*")]
    public void EchoMessage_ReturnsEchoedMessage(string input)
    {
        // Arrange
        var req = ZyRequestTestFactory.Create(new EchoReq { Message = input });
        var res = ZyResponseTestFactory.Create<EchoRes>();

        // Act
        _controller.EchoMessage(req, res);

        // Assert
        var result = res.GetData();
        Assert.NotNull(result);
        Assert.Equal(input, result.Echoed);
    }
}
