using System.Collections.Generic;
using Chartboost.Constants;
using Chartboost.Logging;
using UnityEngine;

namespace Chartboost
{
    public static class AndroidExtensions 
    {
        public static AndroidJavaObject DictionaryToMap(this IDictionary<string, string> source)
            => DictionaryToMap(source, SharedAndroidConstants.ClassString);
        
        public static AndroidJavaObject DictionaryToMap(this IReadOnlyDictionary<string, string> source)
            => DictionaryToMap(source, SharedAndroidConstants.ClassString);
        public static AndroidJavaObject DictionaryToMap(this IReadOnlyDictionary<string, bool> source)
            => DictionaryToMap(source, SharedAndroidConstants.ClassBoolean);

        private static AndroidJavaObject DictionaryToMap<TValue>(IEnumerable<KeyValuePair<string, TValue>> source, string valueFunc)
        {
            var map = new AndroidJavaObject(SharedAndroidConstants.ClassHashMap);
            
            if (source == null)
                return map;
            
            foreach (var kv in source)
            {
                var partnerId = kv.Key;
                if (string.IsNullOrEmpty(partnerId))
                    continue;
                using var key = new AndroidJavaObject(SharedAndroidConstants.ClassString, partnerId);
                using var value = new AndroidJavaObject(valueFunc, kv.Value);
                map.Call<AndroidJavaClass>(SharedAndroidConstants.FunctionPut, partnerId, value);
            }
            return map;
        }

        public static HashSet<T> SetToHashSet<T>(this AndroidJavaObject source)
        {
            var ret = new HashSet<T>();

            if (source == null)
                return ret;
            
            var iter = source.Call<AndroidJavaObject>(SharedAndroidConstants.FunctionIterator);
            
            while (iter.Call<bool>(SharedAndroidConstants.FunctionHasNext))
            {
                var value = iter.Call<T>(SharedAndroidConstants.FunctionNext);
                ret.Add(value);
            }
            return ret;
        }
        
        public static Dictionary<string, string> MapToDictionary(this AndroidJavaObject source) {
            var ret = new Dictionary<string, string>();
            
            if (source == null)
                return ret;

            var size = source.Call<int>(SharedAndroidConstants.FunctionSize);
            if (size == 0)
                return ret;
            
            var entries = source.Call<AndroidJavaObject>(SharedAndroidConstants.FunctionEntrySet);
            var iter = entries.Call<AndroidJavaObject>(SharedAndroidConstants.FunctionIterator);

            do {
                var entry = iter.Call<AndroidJavaObject>(SharedAndroidConstants.FunctionNext);
                var key = entry.Call<string>(SharedAndroidConstants.FunctionGetKey);
                var value = entry.Call<string>(SharedAndroidConstants.FunctionGetValue);
                ret[key] = value;
            } while (iter.Call<bool>(SharedAndroidConstants.FunctionHasNext));
            
            return ret;
        }

        public static List<string> NativeListToList(this AndroidJavaObject source)
        {
            var ret = new List<string>();
            if (source == null)
                return ret;
                
            var size = source.Call<int>(SharedAndroidConstants.FunctionSize);

            if (size == 0)
                return ret;
                
            using var iterator = source.Call<AndroidJavaObject>(SharedAndroidConstants.FunctionIterator);
            while (iterator.Call<bool>(SharedAndroidConstants.FunctionHasNext))
            {
                var nativeValue = iterator.Call<string>(SharedAndroidConstants.FunctionNext);
                ret.Add(nativeValue);
            }
            return ret;
        }

        public static AndroidJavaObject EnumerableToNativeList(this IReadOnlyCollection<string> source)
        {
            var nativeList = new AndroidJavaObject(SharedAndroidConstants.ClassArrayList);

            if (source == null)
                return nativeList;

            if (source.Count == 0)
                return nativeList;
            
            foreach (var testDeviceId in source)
            {
                var added = nativeList.Call<bool>(SharedAndroidConstants.FunctionAdd, testDeviceId);
                if (!added)
                    LogController.Log($"Failed to add test device id: {testDeviceId}", LogLevel.Error);
            }

            return nativeList;
        }

        public static Vector2 PointFToVector2(this AndroidJavaObject pointF)
        {
            if (pointF == null)
                return Vector2.zero;
            
            var x = pointF.Get<float>("x");
            var y = pointF.Get<float>("y");
            return new Vector2(x, y);
        }
        
        public static AndroidJavaObject GetNativeInt(this int value) => new(SharedAndroidConstants.ClassInteger, value);

        public static AndroidJavaObject GetNativeBool(this bool value) => new(SharedAndroidConstants.ClassBoolean, value);

        public static int NativeHashCode(this AndroidJavaObject source) => source.Call<int>(SharedAndroidConstants.FunctionHashCode);
    }
}
