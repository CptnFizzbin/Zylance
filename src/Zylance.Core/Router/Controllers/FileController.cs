using Zylance.Contract.Api.File;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Router.Attributes;
using Zylance.Core.System.Services;

namespace Zylance.Core.Router.Controllers;

/// <summary>
///     Handles all file-related requests for the Gateway.
///     Routes file: prefixed actions to the FileService.
/// </summary>
[Controller]
public class FileController(FileService fileService)
{
    /// <summary>
    ///     Selects a file using the platform file provider and returns the selected
    ///     file reference.
    /// </summary>
    /// <param name="req">Gateway request containing selection parameters.</param>
    /// <param name="res">Gateway response to write results into.</param>
    [RequestHandler]
    public async Task SelectFile(ZyRequest<SelectFileReq> req, ZyResponse<SelectFileRes> res)
    {
        var data = req.GetData();

        var filters = data.Filters?.Select(f => (f.Name, f.Extensions.ToArray())).ToArray();

        var fileRef = await fileService.SelectFile(data.Title, filters, data.ReadOnly);

        res.SetData(new SelectFileRes { FileRef = fileRef });
    }

    /// <summary>
    ///     Creates a new file using the platform file provider and returns the created
    ///     file reference.
    /// </summary>
    /// <param name="req">Gateway request containing file creation parameters.</param>
    /// <param name="res">Gateway response to write results into.</param>
    [RequestHandler]
    public async Task CreateFile(ZyRequest<CreateFileReq> req, ZyResponse<CreateFileRes> res)
    {
        var data = req.GetData();

        var filters = data.Filters?.Select(f => (f.Name, f.Extensions.ToArray())).ToArray();

        var fileRef = await fileService.CreateFile(data.Title, data.Filename, filters);

        res.SetData(new CreateFileRes { FileRef = fileRef });
    }
}
