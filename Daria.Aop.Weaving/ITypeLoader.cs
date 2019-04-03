using System;

namespace Daria.Aop.Weaving
{
	public interface ITypeLoader
	{
		Type GetType(string typeFullName, params string[] assemblyFilePaths);
		Type GetTypeFromAssembly(string typeFullName, string assemblyFilePath);
	}
}