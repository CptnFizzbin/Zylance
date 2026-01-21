using Photino.NET;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Zylance.App.Extensions;

public static class PhotinoExtensions
{
    extension(PhotinoWindow window)
    {
        public PhotinoWindow RegisterJsonMessageHandler(
            Func<JsonObject, PhotinoWindow, JsonObject?> handler,
            JsonSerializerOptions? options = null
        )
        {
            options ??= new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            window.RegisterWebMessageReceivedHandler((sender, message) =>
            {
                try
                {
                    if (sender is not PhotinoWindow senderWindow) return;

                    var jsonObject = JsonNode.Parse(message)?.AsObject();
                    if (jsonObject is null) return;

                    var response = handler(jsonObject, senderWindow);
                    if (response is null) return;

                    var responseJson = response.ToJsonString(options);
                    senderWindow.SendWebMessage(responseJson);
                }
                catch (JsonException)
                {
                    // Silently ignore JSON parsing errors
                }
                catch (Exception ex)
                {
                    // Log handler errors but don't crash
                    Console.Error.WriteLine($"JSON message handler error: {ex.Message}");
                }
            });

            return window;
        }
    }
}
