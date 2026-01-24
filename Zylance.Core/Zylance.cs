using Zylance.Core.Controllers;
using Zylance.Core.Handlers;
using Zylance.Core.Providers;
using Zylance.Core.Services;
using Zylance.Core.Transports;
using EchoController = Zylance.Core.Controllers.EchoController;

namespace Zylance.Core;

public class Zylance
{
    private readonly FileService _fileService;
    private readonly Gateway? _gateway;
    private readonly VaultService _vaultService;

    public Zylance(
        ITransport transport,
        IFileProvider fileProvider,
        IVaultProvider vaultProvider
    )
    {
        _fileService = new FileService(fileProvider);
        _vaultService = new VaultService(vaultProvider);

        _gateway = new Gateway(transport)
            .AddRequestHandler(new FileController(_fileService).HandleRequest)
            .AddRequestHandler(new VaultController(_vaultService).HandleRequest)
            .AddRequestHandler(new EchoController().HandleRequest)
            .AddRequestHandler(new StatusController().HandleRequest);
    }
}
