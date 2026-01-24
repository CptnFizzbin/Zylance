namespace Zylance.Gateway.Transports;

public interface ITransport
{
    public void Send(string message);
    public void Receive(Action<string> callback);
}
