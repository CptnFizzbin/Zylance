namespace Zylance.Core.Lib.Interfaces;

public interface ITransport
{
    public void Send(string message);
    public void Receive(Action<string> callback);
}
