namespace TcBuild;

public class TcIDE : IDisposable
{
    private bool _disposed; // To detect redundant calls
    private EnvDTE80.DTE2 dte;
    private MessageFilter messageFilter;
    private string progID = "TcXaeShell.DTE.15.0";
    private List<EnvDTE80.ErrorItem> _errorItems = new List<EnvDTE80.ErrorItem> { };

    public List<EnvDTE80.ErrorItem> BuildErrors
    {
        get { return _errorItems.FindAll(item => item.ErrorLevel == EnvDTE80.vsBuildErrorLevel.vsBuildErrorLevelHigh); }
    }
    public List<EnvDTE80.ErrorItem> BuildWarnings
    {
        get { return _errorItems.FindAll(item => item.ErrorLevel == EnvDTE80.vsBuildErrorLevel.vsBuildErrorLevelMedium); }
    }
    public List<EnvDTE80.ErrorItem> BuildMessages
    {
        get { return _errorItems.FindAll(item => item.ErrorLevel == EnvDTE80.vsBuildErrorLevel.vsBuildErrorLevelLow); }
    }

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


    public bool Build(string path)
    {
        path = Path.GetFullPath(path).Replace("\\", "/");
        EnvDTE.Solution sln = dte.Solution;
        sln.Open(path);
        EnvDTE.SolutionBuild slnBuild = sln.SolutionBuild;
        slnBuild.Clean(true);
        slnBuild.Build(true);
        SpinWait.SpinUntil(() => slnBuild.BuildState == EnvDTE.vsBuildState.vsBuildStateDone);

        // Capture build output
        EnvDTE80.ErrorItems errorItems = dte.ToolWindows.ErrorList.ErrorItems;
        for (int i = 1; i <= errorItems.Count; i++) _errorItems.Add(errorItems.Item(i));

        return (BuildErrors.Count == 0);
    }
}
// foreach (EnvDTE.Project project in sln.Projects)
// {
//     Console.WriteLine(project.Name);
// }