using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TcBuild;

namespace TcBuildTest
{
    [TestClass]
    public class Test_TcIDE
    {
        [TestMethod]
        public void Test_Build()
        {
            // NOTE: RPC_E_SERVERCALL_RETRYLATER is thrown when path is wrong, at EnvDTE80.DTE2.Quit()
            string filename = "./resources/EmptyProject/EmptyProject.sln";
            using (TcIDE ide = new TcIDE())
            {
                Assert.IsTrue(ide.Build(filename));
                Assert.IsTrue(ide.BuildErrors.Count==0);
            }
        }

        [TestMethod]
        public void Test_BuildBroken()
        {
            string filename = "./resources/BrokenProject/BrokenProject.sln";

            using (TcIDE ide = new TcIDE())
            {
                Assert.IsFalse(ide.Build(filename));
                Assert.IsFalse(ide.BuildErrors.Count == 0);
            }
        }

        [TestMethod]
        public void Test_InstallLibrary()
        {
            string prjFilename = "./resources/Library/Library.sln";
            string libFilename = "./SomeLibrary.library";

            using (TcIDE ide = new TcIDE())
            {
                Assert.IsTrue(ide.InstallLibrary(prjFilename, "Library", "SomeLibrary", "SomeLibrary.library"));
            }

            Assert.IsTrue(System.IO.File.Exists(libFilename));

        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void Test_InstallNotALibrary()
        {
            string filename = "./resources/NotAlibrary/Library.sln";

            using (TcIDE ide = new TcIDE())
            {
                ide.InstallLibrary(filename, "Library", "SomeLibrary", "SomeLibrary.library");
            }
        }


    }
}
