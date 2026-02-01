using Zylance.Contract.Api.File;
using Zylance.Core.App.Services;
using Zylance.Core.Lib.Gateway.Attributes;
using Zylance.Core.Lib.Gateway.Models;

namespace Zylance.Core.App.Controllers;

/// <summary>
///     Handles all file-related requests for the Gateway.
///     Routes file: prefixed actions to the FileService.
/// </summary>
[Controller]
public class FileController(FileService fileService)
{
    [RequestHandler]
    public async Task SelectFile(ZyRequest<SelectFileReq> req, ZyResponse<SelectFileRes> res)
    {
        var data = req.GetData();

        var filters = data.Filters?.Select(f => (f.Name, f.Extensions.ToArray())).ToArray();

        var fileRef = await fileService.SelectFile(data.Title, filters, data.ReadOnly);

        res.SetData(new SelectFileRes { FileRef = fileRef });
    }

    [RequestHandler]
    public async Task CreateFile(ZyRequest<CreateFileReq> req, ZyResponse<CreateFileRes> res)
    {
        var data = req.GetData();

        var filters = data.Filters?.Select(f => (f.Name, f.Extensions.ToArray())).ToArray();

        var fileRef = await fileService.CreateFile(data.Title, data.Filename, filters);

        res.SetData(new CreateFileRes { FileRef = fileRef });
    }
}
