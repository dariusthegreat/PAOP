using System;
using System.Reflection;
using Daria.Aop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public class MethodExecutionArgsTests
    {
        [TestMethod]
        public void ToStringMethodOfMethodExecutionArgs()
        {
            var args = new MethodExecutionArgs
            {
                Method = MethodBase.GetCurrentMethod(),
                Instance = this,
                Arguments = new object[] {1, 3, "something", null, new { index = 1, other = "seven"}, 4.3},
                FlowBehavior = FlowBehavior.Yield,
                Exception = new ApplicationException(),
                MethodExecutionTag = new System.Diagnostics.Stopwatch(),
                ReturnValue = 42,
                YieldValue = "yes"
            };
            
            Console.Out.WriteLine(args);
            Console.Out.WriteLine(args);
            Console.Out.WriteLine(args);
        }
    }
}