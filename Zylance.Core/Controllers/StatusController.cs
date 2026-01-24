using Zylance.Contract.Messages.Status;

namespace Zylance.Core.Controllers;

public class StatusController
{
    public static ResponseWithData<GetStatusRes> GetStatus()
    {
        return new ResponseWithData<GetStatusRes>
        {
            Data = new GetStatusRes
            {
                Status = "OK",
            },
        };
    }
}
