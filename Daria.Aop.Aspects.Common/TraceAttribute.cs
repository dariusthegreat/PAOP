using System;
using Daria.Core;

namespace Daria.Aop.Aspects.Common
{
    [Serializable]
    public class TraceAttribute : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs args)
        {
            var timer = new System.Diagnostics.Stopwatch();
            args.MethodExecutionTag = timer;
            timer.Start();
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            var timer = (System.Diagnostics.Stopwatch) args.MethodExecutionTag;
            timer.Stop();
            WriteLine($"{args.Method.GetName()} execution time: {timer.ElapsedMilliseconds:N2} ms");
        }

        private static void WriteLine(string message) => Console.Out.WriteLine(message);
    }
}