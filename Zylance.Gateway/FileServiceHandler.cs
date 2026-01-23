using Zylance.Contract;

namespace Zylance.Gateway;

/// <summary>
///     Handles all file-related requests for the Gateway.
///     Routes file: prefixed actions to the FileService.
/// </summary>
public class FileServiceHandler(FileService fileService)
{
    /// <summary>
    ///     Handles a file request by routing to the appropriate file operation.
    /// </summary>
    public ResponsePayload HandleFileRequest(RequestPayload request)
    {
        var action = request.Action["file:".Length..]; // Remove "file:" prefix

        return action switch
        {
            "selectFile" => HandleSelectFile(request),
            "createFile" => HandleCreateFile(request),
            _ => throw new NotSupportedException($"File action '{action}' is not supported."),
        };
    }

    private ResponsePayload HandleSelectFile(RequestPayload request)
    {
        var req = MessageSerializer.Deserialize<SelectFileReq>(request.DataJson);

        // Convert FileFilter[] to tuple format
        var filters = req?.Filters?.Select(f => (f.Name, f.Extensions.ToArray())).ToArray();

        var fileRef = fileService.SelectFile(
            req?.Title,
            filters,
            req?.ReadOnly ?? true
        );

        return new ResponsePayload
        {
            RequestId = request.RequestId,
            Status = "success",
            DataJson = MessageSerializer.Serialize(fileRef),
        };
    }

    private ResponsePayload HandleCreateFile(RequestPayload request)
    {
        var req = MessageSerializer.Deserialize<CreateFileReq>(request.DataJson);

        // Convert FileFilter[] to tuple format
        var filters = req?.Filters?.Select(f => (f.Name, f.Extensions.ToArray())).ToArray();

        var fileRef = fileService.CreateFile(
            req?.Title,
            req?.Filename,
            filters
        );

        return new ResponsePayload
        {
            RequestId = request.RequestId,
            Status = "success",
            DataJson = MessageSerializer.Serialize(fileRef),
        };
    }
}
