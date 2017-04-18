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

        public static ExpandoObject Merge(this ExpandoObject additionalData1, ExpandoObject additionalData2)
        {
            IDictionary<string, object> prevExpando = additionalData1;
            ExpandoObject newExpando = additionalData2;

            foreach (var elem in newExpando)
            {
                prevExpando[elem.Key] = elem.Value;
            }

            return (ExpandoObject)prevExpando;
        }
    }
}
