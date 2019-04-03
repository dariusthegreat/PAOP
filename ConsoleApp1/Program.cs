using System;
using System.Reflection;
using Daria.Aop;

namespace ConsoleApp1
{
    internal static class Program
    {
        private static void Main()
        {
            const double a = 3;
            const double b = 4;
            
            var result = DivideRobust(a, b);
            Console.Out.WriteLine($"divide robust result: {result}");

            var normalResult = Divide(a, b);
            Console.Out.WriteLine($"divide result: {normalResult}");
        }

        private static readonly LogInputOutputAttribute s_aspectAttribute = new LogInputOutputAttribute();
        
        // ReSharper disable once UnusedMember.Global
        public static double Template(double a, double b)
        {
            var args = new MethodExecutionArgs
            {
                Method = typeof(Program).GetMethod("Template"),
                Arguments = new object[] {a, b},
                Instance = null
            };
            
            s_aspectAttribute.OnEntry(args);
            
            try
            {
                var result =  a / b;
                args.ReturnValue = result;
                s_aspectAttribute.OnSuccess(args);
                return (double) args.ReturnValue;
            }
            catch (Exception e)
            {
                args.Exception = e;
                s_aspectAttribute.OnException(args);

                if (args.FlowBehavior == FlowBehavior.Default)
                    throw;

                return (double) args.ReturnValue;
            }
        }

        public static double DivideRobust(double a, double b)
        {
            try
            {
                return a / b;
            }
            catch (DivideByZeroException)
            {
                return double.NaN;
            }
        }
        
        [LogInputOutput]
        public static double Divide(double a, double b)
        {
            return a / b;
        }
    }
}