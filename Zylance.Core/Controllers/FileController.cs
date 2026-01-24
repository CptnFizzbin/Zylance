using Zylance.Contract.Messages.File;
using Zylance.Core.Attributes;
using Zylance.Core.Models;
using Zylance.Core.Services;

namespace Zylance.Core.Controllers;

/// <summary>
///     Handles all file-related requests for the Gateway.
///     Routes file: prefixed actions to the FileService.
/// </summary>
[RequestController]
public class FileController(FileService fileService)
{
    [RequestHandler("File:SelectFile")]
    private Task<ZyResponse<SelectFileRes>> SelectFile(ZyRequest<SelectFileReq> req, ZyResponse<SelectFileRes> res)
    {
        var data = req.GetData();

        var filters = data.Filters?.Select(f => (f.Name, f.Extensions.ToArray())).ToArray();

        var fileRef = fileService.SelectFile(
            data.Title,
            filters,
            data.ReadOnly
        );

        res.SetData(new SelectFileRes { FileRef = fileRef });
        return Task.FromResult(res);
    }

    [RequestHandler("File:CreateFile")]
    private Task<ZyResponse<CreateFileRes>> CreateFile(ZyRequest<CreateFileReq> req, ZyResponse<CreateFileRes> res)
    {
        var data = req.GetData();

        var filters = data.Filters?.Select(f => (f.Name, f.Extensions.ToArray())).ToArray();

        var fileRef = fileService.CreateFile(
            data.Title,
            data.Filename,
            filters
        );

        res.SetData(new CreateFileRes { FileRef = fileRef });
        return Task.FromResult(res);
    }
}
