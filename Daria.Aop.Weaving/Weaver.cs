using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using Daria.Aop.Weaving.CodeInjection;
using Daria.Core;
using Mono.Cecil;
using ManifestResourceAttributes = Mono.Cecil.ManifestResourceAttributes;

namespace Daria.Aop.Weaving
{
    public class Weaver
    {
        private readonly IResourceNameProvider _resourceNameProvider;
        private readonly IBinarySerializer _serializer;
        
        private readonly string _assemblyFilePath;
        private AssemblyDefinition _assemblyDefinition;

        public static void Weave(string inputAssemblyPath, string outputAssemblyPath = null)
        {
            new Weaver(inputAssemblyPath)
                .LoadAssembly()
                .Weave()
                .SaveTo(outputAssemblyPath ?? inputAssemblyPath);
        }

        public Weaver(string assemblyFilePath, IResourceNameProvider resourceNameProvider = null)
        {
            _assemblyFilePath = assemblyFilePath;
            _resourceNameProvider = resourceNameProvider ?? new ResourceNameProvider();
            _serializer = new BinarySerializer();
        }

        private Weaver LoadAssembly()
        {
            _assemblyDefinition = AssemblyDefinition.ReadAssembly(_assemblyFilePath, new ReaderParameters
            {
                AssemblyResolver = new CustomResolver(Path.GetDirectoryName(_assemblyFilePath)),
                ReadSymbols = false
            });

            return this;
        }

        private Weaver Weave()
        {
            InjectMethods();
            return this;
        }

        private void SaveTo(string filePath)
        {
            try
            {
                _assemblyDefinition.Write(filePath, new WriterParameters {WriteSymbols = false});
            }
            catch
            {
                Console.Error.WriteLine("failed to write output assembly to:");
                Console.Error.WriteLine(filePath);
                Console.Error.WriteLine($"assembly name: {_assemblyDefinition.FullName}");
                throw;
            }
        }

        private void InjectMethods()
        {
            new AspectExtractor()
                .ExtractAspects(_assemblyDefinition)
                .SelectMany(x => x.Select(customAttribute => new {methodDefinition = x.Key, customAttribute}))
                .Select(x => (x.methodDefinition, x.customAttribute))
                .ForEach(x =>
                {
                    var (methodDefinition, customAttribute) = x;
                    InjectMethod(methodDefinition, customAttribute);
                });
        }

        private void InjectMethod(MethodDefinition methodDefinition, CustomAttribute customAttribute)
        {
            var instance = CreateInstance(customAttribute);
            var serialized = _serializer.Serialize(instance);
            var name = _resourceNameProvider.GetName(methodDefinition, customAttribute);
            
            _assemblyDefinition.MainModule.Resources.Add(new EmbeddedResource(name, ManifestResourceAttributes.Public, serialized));
            
            var injector = GetInjector(methodDefinition);
            injector.InjectCode(customAttribute, name);
        }
        
        private static IMethodCodeInjector GetInjector(MethodDefinition methodDefinition) => new MethodCodeInjector(methodDefinition);

        private MethodLevelAspect CreateInstance(ICustomAttribute customAttribute)
        {
            var ctor = GetCtor(customAttribute);

            var instance = ctor.Invoke(customAttribute.ConstructorArguments.Select(x => x.Value).ToArray());

            foreach (var prop in customAttribute.Properties)
                instance.GetType().GetProperty(prop.Name).SetValue(instance, prop.Argument.Value);

            foreach (var field in customAttribute.Fields)
                instance.GetType().GetField(field.Name).SetValue(instance, field.Argument.Value);

            return (MethodLevelAspect) instance;
        }

        private ConstructorInfo GetCtor(ICustomAttribute customAttribute)
        {
            var directoryPath = Path.GetDirectoryName(_assemblyFilePath);
            var filePaths = Directory.GetFiles(directoryPath, "*.dll", SearchOption.AllDirectories);
            var typeLoader = new TypeLoader();
            var type = typeLoader.GetType(customAttribute.AttributeType.FullName, filePaths);

            var ctorArgTypes = customAttribute.ConstructorArguments
                .Select(x => x.Type)
                .Select(GetType)
                .ToArray();
            
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var ctor = type.GetConstructor(bindingFlags, null, ctorArgTypes, null);
            
            if(ctor==null) throw new ApplicationException($"failed to get ctor for type: {customAttribute.AttributeType.FullName}");

            return ctor;
        }

        [Pure]
        private static Type GetType(TypeReference type) => Type.GetType(type.GetAqn(), true);
    }
}