using System;
using System.Linq;
using System.Reflection;

namespace Daria.Aop.Weaving
{
	public class TypeLoader : ITypeLoader
	{
		public Type GetType(string fullName, params string[] assemblyFilePaths)
		{
			Console.Out.WriteLine($"searching for type: {fullName}");
			
			return assemblyFilePaths
				.Select(x => GetTypeFromAssembly(fullName, x))
				.First(x => x!=null);
		}
        
		public Type GetTypeFromAssembly(string typeFullName, string assemblyFilePath)
		{
			try
			{
				var assembly = Assembly.LoadFile(assemblyFilePath);
				return assembly.GetType(typeFullName, throwOnError:true, ignoreCase:false);
			}
			catch
			{
				return null;
			}
		}
	}
}