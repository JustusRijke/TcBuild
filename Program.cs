string filename = "D:/Git repos/TcCliTools/tests/resources/LibA/LibA.sln";
string progID = "TcXaeShell.DTE.15.0";

for (int i = 1; i <= 10; i++)
{
    Type type = Type.GetTypeFromProgID(progID) ?? throw new ArgumentException($"Couldn't find program {progID}");
    EnvDTE80.DTE2 dte = (EnvDTE80.DTE2)Activator.CreateInstance(type) ?? throw new ArgumentException($"Failed to create instance of {progID}");

    using (MessageFilter messageFilter = new MessageFilter())
    {

        dte.SuppressUI = false;
        dte.MainWindow.Visible = true;
        EnvDTE.Solution sln = dte.Solution;
        sln.Open(filename);

        foreach (EnvDTE.Project project in sln.Projects)
        {
            Console.WriteLine(project.Name);
        }

        dte.Quit();
    }
    Console.WriteLine($"Done ({i})");
}
