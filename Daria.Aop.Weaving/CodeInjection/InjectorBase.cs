using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Daria.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Daria.Aop.Weaving.CodeInjection
{
	public abstract class InjectorBase
	{
		protected readonly MethodDefinition _methodDefinition;
		protected readonly ILProcessor _ilp;
		private readonly Instruction _originalFirstInstruction;

		protected InjectorBase(MethodDefinition methodDefinition, ILProcessor ilp)
		{
			_methodDefinition = methodDefinition;
			_ilp = ilp;
			_originalFirstInstruction = _methodDefinition.Body.Instructions[0];
			_dumperMethodReference = new Lazy<MethodReference>(GetDumperMethodReference);
		}

		protected static ConstructorInfo GetCtor<T>() => typeof(T).GetConstructors().Where(x => x.IsPublic).Single(x => !x.GetParameters().Any());

		protected void AddTail(Instruction instruction)
		{
			_ilp.InsertBefore(_originalFirstInstruction, instruction);
		}

		protected TypeReference ImportReference(Type type) => _methodDefinition.Module.ImportReference(type);
		protected MethodReference ImportReference(MethodBase method) => _methodDefinition.Module.ImportReference(method);

		public static PropertyInfo GetProperty<T, TProp>(Expression<Func<T, TProp>> expression)
		{
			return (PropertyInfo) ((MemberExpression) expression.Body).Member;
		}
		
		
		protected void CallSetter<T, TProp>(Expression<Func<T, TProp>> expression)
		{
			var property = GetProperty(expression);
			AddTail(_ilp.Create(OpCodes.Callvirt, ImportReference(property.SetMethod)));
		}
        
		// ReSharper disable once UnusedMember.Global
		protected void CallGetter<T, TProp>(Expression<Func<T, TProp>> expression)
		{
			var property = GetProperty(expression);
			AddTail(_ilp.Create(OpCodes.Callvirt, ImportReference(property.GetMethod)));
		}
		
		#region Dump support
		
		protected void WriteLine(string message)
		{
			AddTail(_ilp.Create(OpCodes.Ldstr, message));
			AddTail(_ilp.Create(OpCodes.Call, DumperMethod));
		}
		
		private readonly Lazy<MethodReference> _dumperMethodReference;
		protected MethodReference DumperMethod => _dumperMethodReference.Value;
		
		private MethodReference GetDumperMethodReference()
		{
			var methodInfo = typeof(Dumper).GetMethod(nameof(Dumper.Dump));
			return ImportReference(methodInfo);
		}
		
		#endregion
	}
}