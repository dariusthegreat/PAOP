using System.IO;
using Daria.Aop.Weaving;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public class WeaverTests
    {
        [DataRow(@"/Volumes/m10/users/Daria/repo/AOP/out/ConsoleApp1.backup.dll", @"/Volumes/m10/users/Daria/repo/AOP/out/ConsoleApp1.dll")]
        [DataTestMethod]
        public void OutputFileShouldBeCreated(string source, string destination)
        {
            InitializeFiles(source, destination);
            Weaver.Weave(source, destination);
            Assert.IsTrue(File.Exists(destination), "destination file missing");
        }

        private static void InitializeFiles(string source, string destination)
        {
            if (!File.Exists(source))
            {
                File.Move(destination, source);
            }
            else
            {
                if(File.Exists(destination))
                    File.Delete(destination);
            }
        }
    }
}