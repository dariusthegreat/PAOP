using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Daria.Core;

namespace Daria.Aop.Aspects.Common
{
    [Serializable]
    public class LogInputOutputAttribute : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs args)
        {
            WriteLine($"method called: {args.Method.GetName()}");

            var parameters = args.Method.GetParameters();

            if (!parameters.Any())
                return;

            var argsMap = parameters
                .Select((p, index) => new {name = p.Name, value = args.Arguments[index]})
                .ToDictionary(x => x.name, x => x.value);


            var toPrint = new StringBuilder()
                .AppendLine("arguments:")
                .AppendLine(argsMap.ToJson())
                .ToString();
            
            WriteLine(toPrint);
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            var methodInfo = args.Method as MethodInfo;

            if (methodInfo == null)
                return;

            if (methodInfo.ReturnType == typeof(void))
                return;

            WriteLine($"{args.Method.GetName()} returning: {args.ReturnValue}");
        }
        
        private static void WriteLine(string message) => Console.Out.WriteLine(message);
    }
}