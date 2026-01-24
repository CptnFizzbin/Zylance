using Photino.NET;
using Zylance.Core.Interfaces;

namespace Zylance.Desktop;

public class PhotinoTransport(PhotinoWindow window) : ITransport
{
    public void Send(string message)
    {
        window.SendWebMessage(message);
    }

    public void Receive(Action<string> callback)
    {
        window.RegisterWebMessageReceivedHandler((_, message) => callback(message));
    }
}
