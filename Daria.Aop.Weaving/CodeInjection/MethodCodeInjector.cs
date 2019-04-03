using System;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Daria.Aop.Weaving.CodeInjection
{
	public interface IMethodCodeInjector
	{
		void InjectCode(CustomAttribute customAttribute, string resourceName);
	}

	public class MethodCodeInjector : InjectorBase, IMethodCodeInjector
	{
		private VariableDefinition _aspectVariable;
		private VariableDefinition _returnValueVariable;
		private VariableDefinition _argsVariable;
		private VariableDefinition _exceptionVariable;
		
		public MethodCodeInjector(MethodDefinition methodDefinition) : this(methodDefinition, methodDefinition.Body.GetILProcessor()) {}
		
		public MethodCodeInjector(MethodDefinition methodDefinition, ILProcessor ilp) : base(methodDefinition, ilp) {}

		private string AssemblyQualifiedTypeName => GetAqn(_methodDefinition.DeclaringType);

        
		public void InjectCode(CustomAttribute customAttribute, string resourceName)
		{
			AddTail(_ilp.Create(OpCodes.Nop));

			CreateAspectVariable();
			LoadAspectFromResource(resourceName);

			var argsCreator = new MethodExecutionArgumentsCreator(_methodDefinition, _ilp);
			argsCreator.Inject();
			var argsVariable = argsCreator.ArgsVariable;
            
			//WriteLine("about to call OnEntry()...");
            
			AddTail(_ilp.Create(OpCodes.Ldloc, _aspectVariable));
			AddTail(_ilp.Create(OpCodes.Ldloc, argsVariable));
			AddTail(_ilp.Create(OpCodes.Callvirt, GetAspectMethod(nameof(OnMethodBoundaryAspect.OnEntry))));
            
			//WriteLine("OnEntry() called.");


			var nopBeforeTryCatch = _ilp.Create(OpCodes.Nop); 
			AddTail(nopBeforeTryCatch);

			// inject code to create 2 sets of try/catch and try/finally
			// call on entry, success, exception, exit
			// 

			var nameOfVoid = _methodDefinition.Module.ImportReference(typeof(void)).FullName;
			
			var methodReturns = _methodDefinition.ReturnType.FullName != nameOfVoid;

			if (methodReturns)
				InjectMethodCallsIntoMthodWithReturnValue(argsVariable, nopBeforeTryCatch.Next);
		}

		private void InjectMethodCallsIntoMthodWithReturnValue(VariableDefinition argsVariable, Instruction tryStart)
		{
			_argsVariable = argsVariable;
			
			_returnValueVariable = new VariableDefinition(_methodDefinition.ReturnType);
			_methodDefinition.Body.Variables.Add(_returnValueVariable);
			
			_exceptionVariable = new VariableDefinition(_methodDefinition.Module.ImportReference(typeof(Exception)));
			_methodDefinition.Body.Variables.Add(_exceptionVariable);
			
			//_argsVariable = new VariableDefinition(_methodDefinition.Module.ImportReference(typeof(MethodExecutionArgs)));
			//_methodDefinition.Body.Variables.Add(_argsVariable);

			var originalReturn = _methodDefinition.Body.Instructions.Reverse().First(x => x.OpCode == OpCodes.Ret);
			_ilp.Remove(originalReturn);
			

			var leaveTryPlaceHolder = _ilp.Create(OpCodes.Nop);
			var leaveCatchPlaceHolder = _ilp.Create(OpCodes.Nop);
			var jumpOverRethrowPlaceHolder = _ilp.Create(OpCodes.Nop);
			
			Instruction afterCatch = null;
			Instruction returnInsteadOfReThrow = null;
			Instruction boxReturnValue = null;
			Instruction unboxReturnValue = null;
			Instruction catchStart = null;
			
			
			var instructionsList = new System.Collections.Generic.List<Instruction>
			{
				_ilp.Create(OpCodes.Stloc, _returnValueVariable),
				_ilp.Create(OpCodes.Ldloc, _argsVariable),
				_ilp.Create(OpCodes.Ldloc, _returnValueVariable),
				
				(boxReturnValue = _ilp.Create(OpCodes.Box, _methodDefinition.ReturnType)),
				
				_ilp.Create(OpCodes.Callvirt, _methodDefinition.Module.ImportReference(GetProperty<MethodExecutionArgs,object>(x=>x.ReturnValue).SetMethod)),
				_ilp.Create(OpCodes.Nop),
				
				_ilp.Create(OpCodes.Ldloc, _aspectVariable),
				_ilp.Create(OpCodes.Ldloc, _argsVariable),
				_ilp.Create(OpCodes.Callvirt, GetAspectMethod(nameof(OnMethodBoundaryAspect.OnSuccess))),
				_ilp.Create(OpCodes.Nop),
				
				leaveTryPlaceHolder,
				
				(catchStart = _ilp.Create(OpCodes.Stloc, _exceptionVariable)),
				_ilp.Create(OpCodes.Nop),
				
				_ilp.Create(OpCodes.Ldloc, _argsVariable),
				_ilp.Create(OpCodes.Ldloc, _exceptionVariable),
				_ilp.Create(OpCodes.Callvirt, _methodDefinition.Module.ImportReference(GetProperty<MethodExecutionArgs,Exception>(x=>x.Exception).SetMethod)),
				_ilp.Create(OpCodes.Nop),
				
				_ilp.Create(OpCodes.Ldloc, _aspectVariable),
				_ilp.Create(OpCodes.Ldloc, _argsVariable),
				_ilp.Create(OpCodes.Callvirt, GetAspectMethod(nameof(OnMethodBoundaryAspect.OnException))),
				_ilp.Create(OpCodes.Nop),

				_ilp.Create(OpCodes.Ldloc, _argsVariable),
				_ilp.Create(OpCodes.Callvirt, _methodDefinition.Module.ImportReference(GetProperty<MethodExecutionArgs,FlowBehavior>(x=>x.FlowBehavior).GetMethod)),
				_ilp.Create(OpCodes.Ldc_I4, (int) FlowBehavior.Default),
				_ilp.Create(OpCodes.Ceq),
				
				jumpOverRethrowPlaceHolder,
				
				_ilp.Create(OpCodes.Rethrow),
				
				(returnInsteadOfReThrow = _ilp.Create(OpCodes.Ldloc, _argsVariable)),
				_ilp.Create(OpCodes.Callvirt, _methodDefinition.Module.ImportReference(GetProperty<MethodExecutionArgs,object>(x=>x.ReturnValue).GetMethod)),
				(unboxReturnValue = _ilp.Create(OpCodes.Unbox_Any, _methodDefinition.ReturnType)), // remove if not value type
				_ilp.Create(OpCodes.Stloc, _returnValueVariable),
				_ilp.Create(OpCodes.Nop),
				
				leaveCatchPlaceHolder,
				
				(afterCatch = _ilp.Create(OpCodes.Ldloc, _returnValueVariable)),
				_ilp.Create(OpCodes.Ret)
			};
			
			instructionsList[instructionsList.IndexOf(leaveTryPlaceHolder)] = _ilp.Create(OpCodes.Leave, afterCatch);
			instructionsList[instructionsList.IndexOf(leaveCatchPlaceHolder)] = _ilp.Create(OpCodes.Leave, afterCatch);
			instructionsList[instructionsList.IndexOf(jumpOverRethrowPlaceHolder)] = _ilp.Create(OpCodes.Brfalse, returnInsteadOfReThrow);

			if (!_methodDefinition.ReturnType.IsValueType)
			{
				instructionsList.Remove(boxReturnValue);
				instructionsList.Remove(unboxReturnValue);
			}

			foreach (var instruction in instructionsList)
				_ilp.Append(instruction);
			
			
			var handler = new ExceptionHandler (ExceptionHandlerType.Catch) 
			{
				TryStart = tryStart,
				TryEnd = catchStart,
				HandlerStart = catchStart,
				HandlerEnd = afterCatch.Previous,
				CatchType = _methodDefinition.Module.ImportReference(typeof (Exception))
			};

			_methodDefinition.Body.ExceptionHandlers.Add(handler);
		}

		private void CreateAspectVariable()
		{
			_aspectVariable = new VariableDefinition(_methodDefinition.Module.ImportReference(typeof(Aspect)));
			_methodDefinition.Body.Variables.Add(_aspectVariable);
		}
		
		private void LoadAspectFromResource(string resourceName)
		{
			AddTail(_ilp.Create(OpCodes.Ldstr, AssemblyQualifiedTypeName));
			AddTail(_ilp.Create(OpCodes.Ldstr, resourceName));
			var loadWithTypeNameMethodReference = _methodDefinition.Module.ImportReference(typeof(ResourceLoader).GetMethod(nameof(ResourceLoader.LoadWithTypeName)));
			AddTail(_ilp.Create(OpCodes.Call, loadWithTypeNameMethodReference));
			AddTail(_ilp.Create(OpCodes.Stloc, _aspectVariable)); // pop aspect instance from stack and store to local variable
			AddTail(_ilp.Create(OpCodes.Nop));
		}
		
		private MethodReference GetAspectMethod(string name)
		{
			var methodInfo = typeof(OnMethodBoundaryAspect).GetMethod(name);
			return _methodDefinition.Module.ImportReference(methodInfo);
		}

		private static string GetAqn(MemberReference type) => $"{type.FullName}, {type.Module.Assembly.FullName}";
	}
}