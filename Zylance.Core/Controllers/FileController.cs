using Zylance.Contract.Messages.File;
using Zylance.Core.Interfaces;
using Zylance.Core.Models;
using Zylance.Core.Services;

namespace Zylance.Core.Controllers;

/// <summary>
///     Handles all file-related requests for the Gateway.
///     Routes file: prefixed actions to the FileService.
/// </summary>
public class FileController
{
    private const string Name = "File";

    private readonly FileService _fileService;
    private readonly RequestRouter _router;

    public FileController(FileService fileService)
    {
        _fileService = fileService;
        _router = new RequestRouter()
            .Use<SelectFileReq, SelectFileRes>($"{Name}:SelectFile", SelectFile)
            .Use<CreateFileReq, CreateFileRes>($"{Name}:CreateFile", CreateFile);
    }

    public Task<ZyResponse> HandleRequest(ZyRequest zyRequest, ZyResponse zyResponse)
    {
        return _router.MessageReceived(zyRequest, zyResponse);
    }

    private Task<ZyResponse<SelectFileRes>> SelectFile(ZyRequest<SelectFileReq> req, ZyResponse<SelectFileRes> res)
    {
        var data = req.GetData();

        var filters = data.Filters?.Select(f => (f.Name, f.Extensions.ToArray())).ToArray();

        var fileRef = _fileService.SelectFile(
            data.Title,
            filters,
            data.ReadOnly
        );

        res.SetData(new SelectFileRes { FileRef = fileRef });
        return Task.FromResult(res);
    }

    private Task<ZyResponse<CreateFileRes>> CreateFile(ZyRequest<CreateFileReq> req, ZyResponse<CreateFileRes> res)
    {
        var data = req.GetData();

        var filters = data.Filters?.Select(f => (f.Name, f.Extensions.ToArray())).ToArray();

        var fileRef = _fileService.CreateFile(
            data.Title,
            data.Filename,
            filters
        );

        res.SetData(new CreateFileRes { FileRef = fileRef });
        return Task.FromResult(res);
    }
}
