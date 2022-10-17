using TcBuild;
using Xunit.Abstractions;

namespace TcBuildTest;

public class Test_TcIDE
{
    ITestOutputHelper testConsole;

    public Test_TcIDE(ITestOutputHelper output)
    {
        // NOTE: running the tests using:
        // dotnet test -l "console;verbosity=detailed"
        // will show output of testConsole.WriteLine() 
        this.testConsole = output;
    }

    [Fact]
    public void Test_Build()
    {
        // NOTE: RPC_E_SERVERCALL_RETRYLATER is thrown when path is wrong, at EnvDTE80.DTE2.Quit()
        string filename = "./resources/EmptyProject/EmptyProject.sln";
        using (TcIDE ide = new TcIDE())
        {
            Assert.True(ide.Build(filename));
            Assert.Empty(ide.BuildErrors);
        }


    }

    [Fact]
    public void Test_BuildBroken()
    {
        string filename = "./resources/BrokenProject/BrokenProject.sln";

        using (TcIDE ide = new TcIDE())
        {
            Assert.False(ide.Build(filename));
            Assert.NotEmpty(ide.BuildErrors);
        }
    }

    [Fact]
    public void Test_InstallLibrary()
    {
        string filename = "./resources/Library/Library.sln";

        using (TcIDE ide = new TcIDE())
        {
            Assert.True(ide.InstallLibrary(filename, "Library", "SomeLibrary", "SomeLibrary.library"));
        }
    }
}

