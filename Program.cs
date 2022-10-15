string filename = "D:/Git repos/TcCliTools/tests/resources/LibA/LibA.sln";
// string library_name = "LibA";

Type? type = System.Type.GetTypeFromProgID("TcXaeShell.DTE.15.0");
_ = type ?? throw new ArgumentException("TcXaeShell not found");

MessageFilter.Register();

EnvDTE80.DTE2? dte = (EnvDTE80.DTE2?)System.Activator.CreateInstance(type);
_ = dte ?? throw new ArgumentException("Failed to open TcXaeShell");

dte.SuppressUI = false;
dte.MainWindow.Visible = true;
EnvDTE.Solution sln = dte.Solution;
sln.Open(filename);

Console.WriteLine(sln.Projects.GetType());
foreach (EnvDTE.Project project in sln.Projects)
{
    Console.WriteLine(project.Name);
}

dte.Quit();

MessageFilter.Revoke();

Console.WriteLine("Done");

