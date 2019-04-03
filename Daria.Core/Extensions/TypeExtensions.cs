using System;
using System.CodeDom;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
// ReSharper disable MemberCanBePrivate.Global



namespace Daria.Core
{
    public static class TypeExtensions
    {
        public static string GetName(this Type type)
        {
            using (var provider = new CSharpCodeProvider())
            {
                var reference = CreateTypeReference(type);
                return provider.GetTypeOutput(reference);
            }
        }
        
        public static string GetName(this MethodBase method)
        {
            var parameters = method.GetParameters()
                .Select(p => $"{p.ParameterType.GetName()} {p.Name}")
                .Join(", ");

            return $"{method.ReflectedType.GetName()}.{method.Name}({parameters})";
        }
        
        private static CodeTypeReference CreateTypeReference(Type type)
        {
            var typeName = (type.IsPrimitive || type == typeof(string)) ? type.FullName : type.Name;
            
            var reference = new CodeTypeReference(typeName);
            
            if (type.IsArray)
            {
                reference.ArrayElementType = CreateTypeReference(type.GetElementType());
                reference.ArrayRank = type.GetArrayRank();
            }

            if (type.IsGenericType)
                type.GetGenericArguments().ForEach(x => reference.TypeArguments.Add(CreateTypeReference(x)));
            
            return reference;
        }
    }
}