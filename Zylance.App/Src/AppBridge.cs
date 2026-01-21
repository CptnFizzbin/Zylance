using System.Text.Json.Nodes;
using Photino.NET;

namespace Zylance.App;

public class AppBridge
{
    public static JsonObject HandleJsonMessage(JsonObject message, PhotinoWindow window)
    {
        Console.WriteLine($"Received JSON message: {message.ToJsonString()}");
        return new JsonObject { ["message"] = message };
    }
}
