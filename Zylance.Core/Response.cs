namespace Zylance.Core;

public class Response
{
    public string Status { get; init; } = "OK";
}

public class ResponseWithData<TData> : Response
{
    public required TData Data { get; init; }
}
