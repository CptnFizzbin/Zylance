using Zylance.Core.System.Services;
using Zylance.Core.Tests.TestUtils.Mocks;

namespace Zylance.Core.Tests.TestUtils.Factories;

public static class FileServiceTestFactory
{
    public static TestFileProvider CreateProvider()
    {
        return new TestFileProvider();
    }

    public static FileService CreateFileService(TestFileProvider provider)
    {
        return new FileService(provider);
    }
}
