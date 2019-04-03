using Newtonsoft.Json;

namespace Daria.Core
{
    public static class ObjectExtensions
    {
        public static string ToJson<T>(this T obj) => JsonConvert.SerializeObject(obj, Formatting.Indented);
    }
}