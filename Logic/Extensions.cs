using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;
using System.ComponentModel;

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

        public static Dictionary<string, object> DynamicToDictionary(dynamic d)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(d))
            {
                object obj = propertyDescriptor.GetValue(d);
                dictionary.Add(propertyDescriptor.Name, obj);
            }

            return dictionary;
        }

        public static ExpandoObject ToExpando(this IDictionary<string, object> dictionary)
        {
            var expando = new ExpandoObject();
            var expandoDict = (IDictionary<string, object>)expando;

            foreach (var kvp in dictionary)
            {
                if (kvp.Value is IDictionary<string, object>)
                {
                    var expandoValue = ((IDictionary<string, object>)kvp.Value).ToExpando();
                    expandoDict.Add(kvp.Key, expandoValue);
                }
                else if (kvp.Value is ICollection)
                {
                    var itemList = new List<object>();

                    foreach (var item in (ICollection)kvp.Value)
                    {
                        if (item is IDictionary<string, object>)
                        {
                            var expandoItem = ((IDictionary<string, object>)item).ToExpando();
                            itemList.Add(expandoItem);
                        }
                        else
                        {
                            itemList.Add(item);
                        }
                    }

                    expandoDict.Add(kvp.Key, itemList);
                }
                else
                {
                    expandoDict.Add(kvp);
                }
            }

            return expando;
        }
    }
}
