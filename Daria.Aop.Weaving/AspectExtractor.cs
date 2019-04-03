using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Daria.Aop.Weaving
{
	public class AspectExtractor
	{
		public ILookup<MethodDefinition, CustomAttribute> ExtractAspects(AssemblyDefinition assemblyDefinition)
		{
			var assemblyLevel = GetAssemblyLevelAspects(assemblyDefinition);
			var classLevel = GetClassLevelAspects(assemblyDefinition);
			var methodLevel = GetMethodLevelAspects(assemblyDefinition);

			return assemblyLevel.Concat(classLevel)
				.Concat(methodLevel)
				.ToLookup(x => x.Item1, x => x.Item2);
		}

		private static IEnumerable<(MethodDefinition, CustomAttribute)> GetAssemblyLevelAspects(AssemblyDefinition assemblyDefinition)
		{
			var aspects = assemblyDefinition.Modules
				.SelectMany(module => module.CustomAttributes)
				.Where(IsAspect);

			var methods = assemblyDefinition.Modules
				.SelectMany(module => module.Types)
				.SelectMany(type => type.Methods);

			return from method in methods
				from aspect in aspects
				select (method, aspect);
		}

		private static IEnumerable<(MethodDefinition, CustomAttribute)> GetClassLevelAspects(AssemblyDefinition assemblyDefinition)
		{
			return assemblyDefinition
				.Modules.SelectMany(module => module.Types)
				.SelectMany(type => type.CustomAttributes.Where(IsAspect).Select(attribute => new {type, attribute}))
				.SelectMany(x => x.type.Methods.Select(method => (method, x.attribute)));
		}

		private static IEnumerable<(MethodDefinition, CustomAttribute)> GetMethodLevelAspects(AssemblyDefinition assemblyDefinition)
		{
			return assemblyDefinition.Modules
				.SelectMany(module => module.GetTypes())
				.SelectMany(type => type.Methods)
				.SelectMany(method => method.CustomAttributes.Where(IsAspect).Select(attribute => (method, attribute)));
		}

		private static bool IsAspect(CustomAttribute attribute)
		{
			var baseType = typeof(Aspect);
			var imported = attribute.AttributeType.Module.ImportReference(baseType);
			var fullName = imported.FullName;
			return GetInheritedTypes(attribute.AttributeType).Any(x => x.FullName == fullName);
		}

		private static IEnumerable<TypeDefinition> GetInheritedTypes(TypeReference typeReference)
		{
			for (var type = typeReference.Resolve(); type != null; type = type.BaseType?.Resolve())
				yield return type;
		}
	}
}