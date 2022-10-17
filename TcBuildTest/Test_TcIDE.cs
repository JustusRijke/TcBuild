using TcBuild;

namespace TcBuildTest;

public class Test_TcIDE
{
    [Fact]
    public void Test_OpenIDE()
    {
        string filename = "D:/Git repos/TcCliTools/tests/resources/LibA/LibA.sln";

        using (TcIDE ide = new TcIDE())
        {
            ide.OpenSolution(filename);
        }
    }
}