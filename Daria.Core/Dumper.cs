using System;
using Newtonsoft.Json;

namespace Daria.Core
{
    public static class Dumper
    {
        public static void Dump(object obj)
        {
            if (obj == null)
            {
                Console.Out.WriteLine("(null)");
                return;
            }

            if (obj is string)
            {
                Console.Out.WriteLine(obj);
                return;
            }


            var json = ToJson(obj);
            Console.Out.WriteLine(json);
        }

        private static string ToJson(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            catch
            {
                return obj.ToString();
            }
        }
    }
}