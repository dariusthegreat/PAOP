using System;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Daria.Aop.Weaving.CodeInjection
{
	public class MethodExecutionArgumentsCreator : InjectorBase 
	{
		public MethodExecutionArgumentsCreator(MethodDefinition methodDefinition, ILProcessor ilp) : base(methodDefinition, ilp) {}
		
		public VariableDefinition ArgsVariable { get; private set; }

		public void Inject()
		{
			CreateArgsVariable();
			CreateMethodExecutionArgsInstance();
			SetPropertyValues();
		}

		private void CreateArgsVariable()
		{
			ArgsVariable = new VariableDefinition(ImportReference(typeof(MethodExecutionArgs)));
			_methodDefinition.Body.Variables.Add(ArgsVariable);
		}

		private void CreateMethodExecutionArgsInstance()
		{
			AddTail(_ilp.Create(OpCodes.Newobj, _methodDefinition.Module.ImportReference(GetCtor<MethodExecutionArgs>())));
			AddTail(_ilp.Create(OpCodes.Stloc, ArgsVariable));
		}

		private void SetPropertyValues()
		{
			SetMethodPropertyValue();
			SetInstancePropertyValue();
			SetArgumentsPropertyValue();
		}

		private void SetMethodPropertyValue()
		{
			AddTail(_ilp.Create(OpCodes.Ldloc, ArgsVariable));
			AddTail(_ilp.Create(OpCodes.Ldtoken, _methodDefinition));
			var getMethodFromHandle = typeof(MethodBase).GetMethod(nameof(MethodBase.GetMethodFromHandle), new[] {typeof(RuntimeMethodHandle)});
			AddTail(_ilp.Create(OpCodes.Call, ImportReference(getMethodFromHandle)));		// --> method base object is on the stack
			CallSetter<MethodExecutionArgs, MethodBase>(x => x.Method);
			AddTail(_ilp.Create(OpCodes.Nop));
		}

		private void SetInstancePropertyValue()
		{
			if (_methodDefinition.IsStatic) return;
			
			AddTail(_ilp.Create(OpCodes.Ldloc, ArgsVariable));
			AddTail(_ilp.Create(OpCodes.Ldarg_0));
			CallSetter<MethodExecutionArgs, object>(x => x.Instance);
			AddTail(_ilp.Create(OpCodes.Nop));
		}

		private void SetArgumentsPropertyValue()
		{
			if (!_methodDefinition.HasParameters) return;
			
			AddTail(_ilp.Create(OpCodes.Ldloc, ArgsVariable));
			AddTail(_ilp.Create(OpCodes.Ldc_I4, _methodDefinition.Parameters.Count));
			AddTail(_ilp.Create(OpCodes.Newarr, ImportReference(typeof(object))));
			
			int offset = _methodDefinition.IsStatic ? 0 : 1;
            
			for (int i = 0; i < _methodDefinition.Parameters.Count; i++)
			{
				AddTail(_ilp.Create(OpCodes.Dup));
				AddTail(_ilp.Create(OpCodes.Ldc_I4, i));
				AddTail(_ilp.Create(OpCodes.Ldarg, i + offset));

				var parameterTypeReference = _methodDefinition.Parameters[i].ParameterType;

				if (!parameterTypeReference.IsByReference)
					AddTail(_ilp.Create(OpCodes.Box, _methodDefinition.Module.ImportReference(parameterTypeReference)));
				
				AddTail(_ilp.Create(OpCodes.Stelem_Ref));
			}
			
			CallSetter<MethodExecutionArgs, object[]>(x => x.Arguments);
			AddTail(_ilp.Create(OpCodes.Nop));
		}
	}
}