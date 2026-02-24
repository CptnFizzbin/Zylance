using Moq;
using Zylance.Contract.Api.File;
using Zylance.Contract.Models.File;
using Zylance.Core.Platform.Interfaces;
using Zylance.Core.Router.Controllers;
using Zylance.Core.Tests.TestUtils.Factories;

namespace Zylance.Core.Tests.Router.Controllers;

public class FileControllerTests
{
    private readonly FileController _controller;
    private readonly Mock<IFileProvider> _providerMock;

    public FileControllerTests()
    {
        _providerMock = FileServiceTestFactory.CreateMockProvider();
        var fileService = FileServiceTestFactory.CreateFileService(_providerMock);

        _controller = new FileController(fileService);
    }

    [Fact]
    public async Task SelectFile_ReturnsFileRef()
    {
        // Arrange
        var fileRef = new FileRef
        {
            Id = "test-id",
            Filename = "file.txt",
            ReadOnly = false,
        };
        FileServiceTestFactory.SetupSelectFile(_providerMock, fileRef);

        var req = ZyRequestTestFactory.Create(new SelectFileReq { Title = "Pick a file", ReadOnly = false });
        var res = ZyResponseTestFactory.Create<SelectFileRes>();

        // Act
        await _controller.SelectFile(req, res);

        // Assert
        var result = res.GetData();
        Assert.NotNull(result);
        Assert.Equal(fileRef.Id, result.FileRef.Id);
        Assert.Equal(fileRef.Filename, result.FileRef.Filename);
        Assert.Equal(fileRef.ReadOnly, result.FileRef.ReadOnly);
    }

    [Fact]
    public async Task CreateFile_ReturnsFileRef()
    {
        // Arrange
        var fileRef = new FileRef
        {
            Id = "create-id",
            Filename = "newfile.txt",
            ReadOnly = false,
        };
        FileServiceTestFactory.SetupCreateFile(_providerMock, fileRef);

        var req = ZyRequestTestFactory.Create(new CreateFileReq { Title = "Create a file", Filename = "newfile.txt" });
        var res = ZyResponseTestFactory.Create<CreateFileRes>();

        // Act
        await _controller.CreateFile(req, res);

        // Assert
        var result = res.GetData();
        Assert.NotNull(result);
        Assert.Equal(fileRef.Id, result.FileRef.Id);
        Assert.Equal(fileRef.Filename, result.FileRef.Filename);
        Assert.Equal(fileRef.ReadOnly, result.FileRef.ReadOnly);
    }
}
