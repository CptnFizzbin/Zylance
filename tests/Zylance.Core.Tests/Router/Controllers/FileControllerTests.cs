using Zylance.Contract.Api.File;
using Zylance.Core.Router.Controllers;
using Zylance.Core.System.Services;
using Zylance.Core.Tests.TestUtils.Factories;
using Zylance.Core.Tests.TestUtils.Mocks;

namespace Zylance.Core.Tests.Router.Controllers;

public class FileControllerTests : IDisposable
{
    private readonly FileController _controller;
    private readonly TestFileProvider _fileProvider;

    public FileControllerTests()
    {
        _fileProvider = new TestFileProvider();

        var fileService = new FileService(_fileProvider);

        _controller = new FileController(fileService);
    }

    public void Dispose()
    {
        _fileProvider.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task SelectFile_ReturnsFileRef()
    {
        // Arrange - create a real file in the test directory and queue it for selection
        var fileRef = _fileProvider.CreateFile("select-test.txt");
        _fileProvider.QueueSelectFile(_fileProvider.GetFilePath(fileRef));

        var req = ZyRequestTestFactory.Create(new SelectFileReq { Title = "Pick a file", ReadOnly = false });
        var res = ZyResponseTestFactory.Create<SelectFileRes>();

        // Act
        await _controller.SelectFile(req, res);

        // Assert
        var result = res.GetData();
        Assert.NotNull(result);
        Assert.Equal("select-test.txt", result.FileRef.Filename);
        Assert.False(result.FileRef.ReadOnly);
    }

    [Fact]
    public async Task CreateFile_ReturnsFileRef()
    {
        // Arrange - queue a path for the provider to return when CreateFile is called
        var newFilePath = Path.Combine(_fileProvider.RootPath, "newfile.txt");
        _fileProvider.QueueCreateFile(newFilePath);

        var req = ZyRequestTestFactory.Create(new CreateFileReq { Title = "Create a file", Filename = "newfile.txt" });
        var res = ZyResponseTestFactory.Create<CreateFileRes>();

        // Act
        await _controller.CreateFile(req, res);

        // Assert
        var result = res.GetData();
        Assert.NotNull(result);
        Assert.Equal("newfile.txt", result.FileRef.Filename);
        Assert.False(result.FileRef.ReadOnly);
    }
}
