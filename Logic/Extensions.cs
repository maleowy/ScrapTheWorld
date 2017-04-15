using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;

namespace Logic
{
    public static class Extensions
    {
        public static T Clone<T>(this T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
