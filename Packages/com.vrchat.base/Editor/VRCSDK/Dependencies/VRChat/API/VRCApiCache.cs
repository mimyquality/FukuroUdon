using System;
using System.Collections.Generic;
using System.Net.Http;
using UnityEditor;

namespace VRC.SDKBase.Editor.Api
{
    [InitializeOnLoad]
    internal static class VRCApiCache
    {
        private static long DEFAULT_CACHE_TIME;
        private static Dictionary<string, (DateTime timestamp, object data)> _cache;

        static VRCApiCache()
        {
            _cache = new Dictionary<string, (DateTime timestamp, object data)>();
            DEFAULT_CACHE_TIME = 1000 * 60 * 1; // 1 minute
        }

        public static T Add<T>(string key, T value)
        {
            _cache[key] = (DateTime.UtcNow, value);
            return value;
        }

        public static T Get<T>(string key, out bool cached)
        {
            if (_cache.TryGetValue(key, out var value))
            {
                if (DateTime.UtcNow.Subtract(value.timestamp).TotalMilliseconds < DEFAULT_CACHE_TIME)
                {
                    cached = true;
                    return (T) value.data;
                }

                cached = false;
                return default;
            }

            cached = false;
            return default;
        }

        public static void Invalidate(string key)
        {
            _cache.Remove(key);
        }

        public static void Clear()
        {
            _cache = new Dictionary<string, (DateTime timestamp, object data)>();
        }
    }
}

