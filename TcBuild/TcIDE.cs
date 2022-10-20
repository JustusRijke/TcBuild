using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace TcBuild;

public class TcIDE : IDisposable
{
    private bool _disposed; // To detect redundant calls
    private EnvDTE80.DTE2 dte;
    private MessageFilter messageFilter;
    private string progID = "TcXaeShell.DTE.15.0";
    private List<EnvDTE80.ErrorItem> _errorItems = new List<EnvDTE80.ErrorItem> { };

    public List<EnvDTE80.ErrorItem> ErrorItems
    {
        get { return _errorItems; }
    }

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

    private EnvDTE.Solution OpenSolution(string path)
    {
        EnvDTE.Solution sln = dte.Solution;
        sln.Open(Path.GetFullPath(path).Replace("\\", "/"));
        Thread.Sleep(1000);
        return sln;
    }

    public bool Build(string slnPath)
    {
        EnvDTE.Solution sln = OpenSolution(slnPath);

        // Build solution
        EnvDTE.SolutionBuild slnBuild = sln.SolutionBuild;
        slnBuild.Clean(true);
        Thread.Sleep(1000);
        slnBuild.Build(true);
        SpinWait.SpinUntil(() => slnBuild.BuildState == EnvDTE.vsBuildState.vsBuildStateDone);

        // Capture build output
        EnvDTE80.ErrorItems errorItems;
        try
        {
            // First try it the hard way, but which leads to nicely ordered error items
            EnvDTE.Window window = this.dte.Windows.Item(EnvDTE80.WindowKinds.vsWindowKindErrorList);
            EnvDTE80.ErrorList selection = (EnvDTE80.ErrorList)window.Selection;
            errorItems = selection.ErrorItems;
        }
        catch (System.Exception)
        {
            // If that didn't work, do it the traditional way
            errorItems = dte.ToolWindows.ErrorList.ErrorItems;
        }

        for (int i = 1; i <= errorItems.Count; i++) _errorItems.Add(errorItems.Item(i));

        // Return true on successful build
        return (BuildErrors.Count == 0);
    }


    public bool InstallLibrary(string slnPath, string projectName, string libraryName, string libraryPath)
    {
        EnvDTE.Solution sln = OpenSolution(slnPath);

        // Find project
        EnvDTE.Project project = null;
        foreach (EnvDTE.Project _project in sln.Projects)
        {
            if (_project.Name == projectName)
            {
                project = _project;
                break;
            }
        }
        _ = project ?? throw new ArgumentException($"Couldn't find project {projectName}");
        dynamic sysManager = project.Object ?? throw new ArgumentException("Can't access ITcSysManager");
        dynamic iecProject = sysManager.LookupTreeItem($"TIPC^{libraryName}^{libraryName} Project");

        // Save library
        libraryPath = Path.GetFullPath(libraryPath).Replace("\\", "/");
        iecProject.SaveAsLibrary(libraryPath, true);

        // Return true on successful build
        return true;
    }
}
