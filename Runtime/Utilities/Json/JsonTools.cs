using System;
using System.Collections.Generic;
using Chartboost.Logging;
using Newtonsoft.Json;

namespace Chartboost.Json
{
    public static class JsonTools
    {
        private const string JsonToolsTag = "[JsonTools]";
        
        public static string SerializeObject<T>(T target, Formatting formatting)
        {
            if (target != null) return JsonConvert.SerializeObject(target, formatting);
            LogController.Log($"{JsonToolsTag}/SerializeObject string value cannot be null or empty.", LogLevel.Warning);
            return string.Empty;
        }

        public static string SerializeObject<T>(T target)
        {
            return SerializeObject(target, Formatting.None);
        }

        public static T DeserializeObject<T>(string objectJson)
        {
            if (!string.IsNullOrEmpty(objectJson)) 
                return JsonConvert.DeserializeObject<T>(objectJson)!;
            LogController.Log($"{JsonToolsTag}/DeserializeObject string value cannot be null or empty.", LogLevel.Warning);
            return default!;
        }
        
        public static T? DeserializeNullableObject<T>(string objectJson) where T: struct
        {
            if (!string.IsNullOrEmpty(objectJson)) 
                return JsonConvert.DeserializeObject<T>(objectJson);
            LogController.Log($"{JsonToolsTag}/DeserializeNullableObject string value cannot be null or empty.", LogLevel.Warning);
            return null;
        }
        
        public static object DeserializeObject(this string json)
        {
            try
            {
                json = json.Trim();
                if (json.StartsWith("{"))
                    return JsonConvert.DeserializeObject<IDictionary<object, object>>(json);
                return json.StartsWith("[") ? JsonConvert.DeserializeObject<IList<object>>(json) : JsonConvert.DeserializeObject(json);
            }
            catch (Exception jsonException)
            {
                LogController.Log(jsonException.Message, LogLevel.Error);
                return null;
            }
        }
    }
}
