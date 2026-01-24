using Zylance.Contract.Messages.Echo;

namespace Zylance.Core.Controllers;

public static class EchoController
{
    public static ResponseWithData<EchoRes> Echo(EchoReq message)
    {
        return new ResponseWithData<EchoRes>
        {
            Data = new EchoRes
            {
                Echoed = message.Message,
            },
        };
    }
}
