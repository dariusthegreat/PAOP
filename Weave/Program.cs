using System;
using Daria.Aop;
using Daria.Aop.Weaving;

namespace Weave
{
    internal static class Program
    {
        private const string DefaultSourcePath = @"/Volumes/m10/users/Daria/Projects/AOP/ConsoleApp1/bin/Debug/netcoreapp2.2/ConsoleApp1.dll";
        private const string DefaultDestinationPath = @"/Volumes/m10/users/Daria/Projects/AOP/ConsoleApp1/bin/Debug/netcoreapp2.2/ConsoleApp1.reweaved.dll";

        private static void Main(string[] args)
        {
            var (source, destination) = args?.Length == 2 ? (args[0], args[1]) : (DefaultSourcePath, DefaultDestinationPath);
            Weaver.Weave(source, destination);   
        }
    }
}