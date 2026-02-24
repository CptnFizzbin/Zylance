using Moq;
using Zylance.Contract.Models.File;
using Zylance.Core.Platform.Interfaces;
using Zylance.Core.System.Services;

namespace Zylance.Core.Tests.TestUtils.Factories;

public static class FileServiceTestFactory
{
    public static Mock<IFileProvider> CreateMockProvider()
    {
        return new Mock<IFileProvider>(MockBehavior.Strict);
    }

    public static FileService CreateFileService(Mock<IFileProvider> providerMock)
    {
        return new FileService(providerMock.Object);
    }

    public static Mock<IFileProvider> SetupSelectFile(Mock<IFileProvider> providerMock, FileRef fileRef)
    {
        providerMock
            .Setup(p =>
                p.SelectFile(It.IsAny<string>(), It.IsAny<(string Name, string[] Extensions)[]>(), It.IsAny<bool>())
            )
            .ReturnsAsync(fileRef);
        return providerMock;
    }

    public static Mock<IFileProvider> SetupCreateFile(Mock<IFileProvider> providerMock, FileRef fileRef)
    {
        providerMock
            .Setup(p =>
                p.CreateFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<(string Name, string[] Extensions)[]>())
            )
            .ReturnsAsync(fileRef);
        return providerMock;
    }

    public static Mock<IFileProvider> SetupExists(Mock<IFileProvider> providerMock, FileRef fileRef, bool exists)
    {
        providerMock.Setup(p => p.Exists(fileRef)).ReturnsAsync(exists);
        return providerMock;
    }
}
