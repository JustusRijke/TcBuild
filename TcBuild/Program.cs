using System.CommandLine; // https://learn.microsoft.com/en-us/dotnet/standard/commandline/
using TcBuild;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLI
{
    internal class Program

    {
        enum ReturnCode
        {
            SUCCESS = 0,
            BUILD_WARNINGS = 1,
            BUILD_ERRORS = 2,
            COM_EXCEPTION = 3,
            UNKNOWN_EXCEPTION = 4,
            MISSING_FILE=5
        };

        static async Task<int> Main(string[] args)
        {
            int returnCode = 0;

            var fileArg = new Argument<String>(name: "file", description: "The TwinCAT solution file (.sln)");
            var xaeProjectName = new Option<String>(new String[] { "--xaeproject", "-x" }, "The XAE project containing the PLC project.") { IsRequired = true };
            var plcProjectName = new Option<String>(new String[] { "--plcproject", "-p" }, "The PLC project to be used as a library.") { IsRequired = true };
            var libraryFilename = new Option<String>(new String[] { "--libraryfile", "-l" }, "The filename of the resulting .library file. Overwrites existing file.\nIf left empty, the PLC project name is used.");

            var rootCommand = new RootCommand("CLI tool for building Beckhoff TwinCAT solutions");
            var buildCommand = new Command("build", "Build the solution.")
            {
                fileArg,
            };
            buildCommand.SetHandler(file =>
            {
                returnCode = Build(file!);
            }, fileArg);
            rootCommand.AddCommand(buildCommand);

            var installCommand = new Command("install", "Save and install a PLC project in the solution as a library.")
            {
                fileArg,
                xaeProjectName,
                plcProjectName,
                libraryFilename
            };
            installCommand.SetHandler((file, xaeproject, plcproject, libraryfile) =>
            {
                returnCode = InstallAndSaveAsLibrary(file!, xaeproject, plcproject, libraryfile);
            }, fileArg, xaeProjectName, plcProjectName, libraryFilename);
            rootCommand.AddCommand(installCommand);

            await rootCommand.InvokeAsync(args);

            return returnCode;
        }

        private static int Build(string slnPath)
        {
            if (!File.Exists(slnPath)) return (int)ReturnCode.MISSING_FILE;
               
            try
            {
                using (TcIDE ide = new TcIDE())
                {
                    ide.Build(slnPath);
                    ShowBuildOutput(ide);
                    if (ide.BuildErrors.Any())
                        return (int)ReturnCode.BUILD_ERRORS;
                    if (ide.BuildWarnings.Any())
                        return (int)ReturnCode.BUILD_WARNINGS;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e is System.Runtime.InteropServices.COMException)
                    return (int)ReturnCode.COM_EXCEPTION;
                return (int)ReturnCode.UNKNOWN_EXCEPTION;
            }

            return (int)ReturnCode.SUCCESS;
        }

        private static int InstallAndSaveAsLibrary(string slnPath, string xaeProject, string plcProject, string libraryFile)
        {
            if (!File.Exists(slnPath)) return (int)ReturnCode.MISSING_FILE;

            try
            {
                using (TcIDE ide = new TcIDE())
                {
                    libraryFile = libraryFile ?? (plcProject + ".library");
                    ide.InstallLibrary(slnPath, xaeProject, plcProject, libraryFile);
                    ShowBuildOutput(ide);
                    if (ide.BuildErrors.Any())
                        return (int)ReturnCode.BUILD_ERRORS;
                    if (ide.BuildWarnings.Any())
                        return (int)ReturnCode.BUILD_WARNINGS;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e is System.Runtime.InteropServices.COMException)
                    return (int)ReturnCode.COM_EXCEPTION;
                return (int)ReturnCode.UNKNOWN_EXCEPTION;
            }

            return (int)ReturnCode.SUCCESS;
        }

        private static void ShowBuildOutput(TcIDE ide)
        {
            foreach (EnvDTE80.ErrorItem item in ide.ErrorItems)
            {
                switch (item.ErrorLevel)
                {
                    case EnvDTE80.vsBuildErrorLevel.vsBuildErrorLevelHigh:
                        Console.Write("E: ");
                        break;
                    case EnvDTE80.vsBuildErrorLevel.vsBuildErrorLevelMedium:
                        Console.Write("W: ");
                        break;
                    case EnvDTE80.vsBuildErrorLevel.vsBuildErrorLevelLow:
                        Console.Write("I: ");
                        break;
                }
                if (!String.IsNullOrEmpty(item.FileName)) Console.Write($"{item.FileName}:{item.Line} - ");
                Console.WriteLine(item.Description);
            }
        }
    }
}