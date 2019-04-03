using System;
using Mono.Cecil;

namespace Daria.Aop.Weaving
{
	public class ResourceNameProvider : IResourceNameProvider
	{
		public string GetName(MemberReference methodDefinition, CustomAttribute customAttribute) => $"{methodDefinition.FullName}-{customAttribute.AttributeType.FullName}.{Guid.NewGuid():N}";
	}
}