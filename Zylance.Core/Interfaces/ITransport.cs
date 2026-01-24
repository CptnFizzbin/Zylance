namespace Zylance.Core.Transports;

public interface ITransport
{
    public void Send(string message);
    public void Receive(Action<string> callback);
}
