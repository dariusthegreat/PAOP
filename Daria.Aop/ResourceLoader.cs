using System;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Daria.Core;

namespace Daria.Aop
{
    public class DummyAspect : OnMethodBoundaryAspect
    {
        public override bool CompileTimeValidate(object target)
        {
            return true;
        }

        public override void OnEntry(MethodExecutionArgs args)
        {
            Console.Out.WriteLine($"method entered: {args.Method.GetName()}");
        }
        
        public override void OnSuccess(MethodExecutionArgs args)
        {
            Console.Out.WriteLine($"method succeeded: {args.Method.GetName()}");
        }
        
        public override void OnException(MethodExecutionArgs args)
        {
            Console.Out.WriteLine($"method exception: {args.Method.GetName()}");
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            Console.Out.WriteLine($"method exited: {args.Method.GetName()}");
        }
    }
    
    public class ResourceLoader
    {
        public static Aspect LoadWithTypeName(string typeName, string resourceName)
        {
            var type = Type.GetType(typeName, throwOnError:false);
            
            if (type == null)
            {
                Console.Error.WriteLine("type could not be loaded:");
                Console.Error.WriteLine(typeName);
                throw new NotSupportedException($"type not found: {typeName}");
            }

            return Load(type.Assembly, resourceName);
        }
        

        public static Aspect Load(Assembly assembly, string name)
        {
            try
            {
                return LoadInternal(assembly, name);
            }
            catch
            {
                Console.Error.WriteLine($"failed to load:\n{name}\nfrom assembly:\n{assembly.FullName}");
                var names = assembly.GetManifestResourceNames().Join("\n");
                Console.Error.WriteLine($"available resources:\n{names}");
                throw;
            }
        }
        
        private static Aspect LoadInternal(Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                var formatter = new BinaryFormatter();
                return (Aspect) formatter.Deserialize(stream);
            }
        }
    }
}