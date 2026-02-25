using Serilog;
using Zylance.Contract.Api.File;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Logging;
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
    private static readonly ILogger Log = ZyLogger.ForContext<FileController>();

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
        Log.Debug("SelectFile called Title={Title} ReadOnly={ReadOnly}", data.Title, data.ReadOnly);

        var filters = data.Filters?.Select(f => (f.Name, f.Extensions.ToArray())).ToArray();

        var fileRef = await fileService.SelectFileAsync(data.Title, filters, data.ReadOnly);

        res.SetData(new SelectFileRes { FileRef = fileRef });
        Log.Debug("SelectFile returned FileRef={FileRef}", fileRef);
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
        Log.Debug("CreateFile called Title={Title} Filename={Filename}", data.Title, data.Filename);

        var filters = data.Filters?.Select(f => (f.Name, f.Extensions.ToArray())).ToArray();

        var fileRef = await fileService.CreateFileAsync(data.Title, data.Filename, filters);

        res.SetData(new CreateFileRes { FileRef = fileRef });
        Log.Debug("CreateFile returned FileRef={FileRef}", fileRef);
    }
}
