using TcBuild;

namespace TcBuildTest;

public class Test_TcIDE
{
    [Fact]
    public void Test_OpenIDE()
    {
        string filename = "D:/Git repos/TcCliTools/tests/resources/LibA/LibA.sln";

        for (int i = 1; i <= 100; i++)
        {
            using (TcIDE ide = new TcIDE())
            {
                ide.OpenSolution(filename);
            }
        }
    }
}