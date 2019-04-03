using Mono.Cecil;

namespace Daria.Aop.Weaving
{
	public static class Extensions
	{
		public static string GetAqn(this MemberReference type) => $"{type.FullName}, {type.Module.Assembly.FullName}";
	}
}