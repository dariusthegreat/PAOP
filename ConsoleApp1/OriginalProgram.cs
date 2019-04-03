using System;
using System.Diagnostics.Contracts;

namespace ConsoleApp1
{
    [Trace]
    public static class OriginalProgram
    {
        public static void TheMainMethod()
        {
            DoIt();
            TestMethodNotThrowing();
            EnsureExceptionThrowingWorks();
        }

        private static void EnsureExceptionThrowingWorks()
        {
            if(!IsExceptionThrowingWorking())
                throw new ApplicationException("Exception throwing method not working");
        }
        
        private static bool IsExceptionThrowingWorking()
        {
            try
            {
                TestMethodThrowing();
            }
            catch (TestException)
            {
                return true;
            }

            return false;
        }
        
        
        // ReSharper disable once UnusedMember.Local
        private static void DoIt2()
        {
            const int a = 42;
            const int b = 13;

            for (int i = 3; i < 17; i++)
            {
                var sum = Add(a, b);
                Console.Out.WriteLine($"sum: {sum}");    
            }
        }


        [LogInputOutput]
        private static int Add(int a, int b)
        {
            return a + b;
        }

        private static void DoIt()
        {
            const double radius = 27;
            
            var diff = Compare(radius);

            var ratio = diff / Math.Pow(radius, 2);
            
            Console.Out.WriteLine($"error: {ratio:P2}");

            const double threshold = 0.02;

            var pass = ratio <= threshold;
            
            Console.Out.WriteLine($"pass? {pass}");
        }

        
        [LogInputOutput]
        private static double Compare(double r)
        {
            var area1 = GetHalfCircleArea(r);
            var area2 = GetHalfCircleAreaUsingIntegration(r);
            
            Console.Out.WriteLine($"area1: {area1:N2}");
            Console.Out.WriteLine($"area2: {area2:N2}");
            
            var diff = Math.Abs(area2 - area1);
            
            Console.Out.WriteLine($"diff: {diff:N2}");

            var normalizedError = diff / Math.Pow(r, 2);
            Console.Out.WriteLine($"normalized: {normalizedError:P2}");
            
            
            Console.Out.WriteLine($"error/area 1: {diff/area1:P2}");
            Console.Out.WriteLine($"error/area 2: {diff/area2:P2}");
            
            

            return diff;
        }


        [LogInputOutput]
        private static double GetHalfCircleAreaUsingIntegration(double r) => Integrate(x => GetH(x, r), 0, 2 * r);

        [LogInputOutput]
        private static double GetHalfCircleArea(double r) => Math.PI * Math.Pow(r, 2) / 2;


        private static double GetH(double x, double r)
        {
            var angle = GetAngle(x, r);
            return r * Math.Sin(angle);
        }


        [Pure]
        private static double GetAngle(double x, double r)
        {
            return Math.Acos(X2D(x, r) / r);
        }

        [Pure]
        private static double X2D(double x, double r)
        {
            return Math.Abs(r - x);
        }



        private static double Integrate(Func<double, double> func, double lowerLimit, double upperLimit, int partitionCount = 100)
        {
            return MathNet.Numerics.Integration.SimpsonRule.IntegrateComposite(func, lowerLimit, upperLimit, partitionCount);
        }


        [TestAspect(ThrowException = false)]
        private static void TestMethodNotThrowing()
        {
            Console.Out.WriteLine("this method does not throw");
        }


        [TestAspect(ThrowException = true)]
        private static void TestMethodThrowing()
        {
            Console.Out.WriteLine("this method throws");
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
    }
}