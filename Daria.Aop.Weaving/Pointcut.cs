using Mono.Cecil;

namespace Daria.Aop.Weaving
{
	public class Pointcut
	{
		public MethodDefinition Method { get; set; }
		public CustomAttribute Attribute { get; set; }
		public int AttributeIndex { get; set; }
	}
}