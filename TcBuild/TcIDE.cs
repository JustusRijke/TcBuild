namespace TcBuild;

public class TcIDE : IDisposable
{
    private bool _disposed; // To detect redundant calls
    private EnvDTE80.DTE2 dte;
    private MessageFilter messageFilter;
    private string progID = "TcXaeShell.DTE.15.0";

    public TcIDE()
    {
        messageFilter = new MessageFilter();
        messageFilter.Register();

        Type type = Type.GetTypeFromProgID(progID) ?? throw new ArgumentException($"Couldn't find program {progID}");
        dte = (EnvDTE80.DTE2)Activator.CreateInstance(type) ?? throw new ArgumentException($"Failed to create instance of {progID}");

        dte.SuppressUI = false;
        dte.MainWindow.Visible = true;
    }

    // Public implementation of Dispose pattern callable by consumers.
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Protected implementation of Dispose pattern.
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                dte.Quit();
                messageFilter.Revoke();
            }
        }
        _disposed = true;
    }


    public void OpenSolution(string path)
    {
        EnvDTE.Solution sln = dte.Solution;
        sln.Open(path);

        // foreach (EnvDTE.Project project in sln.Projects)
        // {
        //     Console.WriteLine(project.Name);
        // }

        Console.WriteLine("Done");
    }
}