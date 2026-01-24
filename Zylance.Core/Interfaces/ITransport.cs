namespace Zylance.Core.Interfaces;

public interface ITransport
{
    public void Send(string message);
    public void Receive(Action<string> callback);
}
