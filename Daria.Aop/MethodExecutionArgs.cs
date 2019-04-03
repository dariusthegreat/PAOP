using System;
using System.Reflection;
using System.Text;
using Daria.Core;

namespace Daria.Aop
{
    public abstract class AspectExecutionArgs {}
    
    public class MethodExecutionArgs : AspectExecutionArgs
    {
        public MethodBase Method { get; set; }
        public object Instance { get; set; }
        public object[] Arguments { get; set; }
        public FlowBehavior FlowBehavior { get; set; }
        public object ReturnValue { get; set; }
        public object YieldValue { get; set; }
        public Exception Exception { get; set; }
        public object MethodExecutionTag { get; set; }


        public override string ToString()
        {
            var str = ToStringInternal();
            str = System.Text.RegularExpressions.Regex.Replace(str, @"^|$", "\t\t", System.Text.RegularExpressions.RegexOptions.Multiline);
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[\n\s]+(?=$)", "", System.Text.RegularExpressions.RegexOptions.None);
            
            return new StringBuilder()
                .AppendLine()
                .Append("\t\t")
                .AppendLine(new string('-', 80))
                .AppendLine(str)
                .Append("\t\t")
                .AppendLine(new string('-', 80))
                .ToString();
        }

        private string ToStringInternal()
        {
            var sb = new StringBuilder()
                .AppendLine($"method: {Method?.GetName() ?? "(null)"}")
                .AppendLine($"instance? {(Instance == null ? "no" : "yes")}");


            if (Arguments == null)
            {
                sb.AppendLine("args: (none)");
            }
            else
            {
                sb.AppendLine($"args: ({Arguments.Length})");

                foreach (var x in Arguments)
                    sb.AppendLine(x?.ToString() ?? "(null)");
            }

            sb.AppendLine($"flow behavior: {FlowBehavior}");
            sb.AppendLine($"Exception: {Exception?.GetType().Name ?? "none"}");
            sb.AppendLine($"Return value: {ReturnValue ?? "(null)"}");
            
            return sb.ToString();
        }
    }
}