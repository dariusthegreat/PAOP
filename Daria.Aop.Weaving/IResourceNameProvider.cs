using Mono.Cecil;

namespace Daria.Aop.Weaving
{
	public interface IResourceNameProvider
	{
		string GetName(MemberReference methodDefinition, CustomAttribute customAttribute);
	}
}