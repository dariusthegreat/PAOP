using Mono.Cecil;

namespace Daria.Aop.Weaving
{
	internal class CustomResolver : BaseAssemblyResolver
	{
		private readonly DefaultAssemblyResolver _defaultResolver;
        
		public CustomResolver(string sourceDir)
		{
			_defaultResolver = new DefaultAssemblyResolver();
			_defaultResolver.AddSearchDirectory(sourceDir);
		}

		public override AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			return _defaultResolver.Resolve(name);
		}
	}
}