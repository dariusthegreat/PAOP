using System;
using Daria.Aop;
using Daria.Core;

namespace ConsoleApp1
{
	[Serializable]
	public class TestAspectAttribute : OnMethodBoundaryAspect
	{
		public bool ThrowException { get; set; } = false;
        
		public override void OnEntry(MethodExecutionArgs args)
		{
			Console.Out.WriteLine($"entered: {args.Method.GetName()}");
            
			if(ThrowException)
				throw new TestException();
		}

		public override void OnSuccess(MethodExecutionArgs args)
		{
			Console.Out.WriteLine($"executed: {args.Method.GetName()}");
		}

		public override void OnException(MethodExecutionArgs args)
		{
			Console.Out.WriteLine($"exception in: {args.Method.GetName()}");
		}

		public override void OnExit(MethodExecutionArgs args)
		{
			Console.Out.WriteLine($"exited: {args.Method.GetName()}");
		}
	}
}