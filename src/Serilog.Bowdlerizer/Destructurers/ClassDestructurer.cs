using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Bowdlerizer.Destructurers {
    public static class ClassDestructurer {
        private static readonly ConcurrentDictionary<Type, CacheEntry> _cache = new ConcurrentDictionary<Type, CacheEntry>();

        internal static readonly HashSet<Type> BuiltInScalarTypes = new HashSet<Type>() {
            typeof(bool),
            typeof(char),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(string),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(Uri)
        };

        internal static LogEventPropertyValue GetValues(ILogEventPropertyValueFactory propertyValueFactory,
            object value) {
            var cached = _cache.GetOrAdd(value.GetType(), CreateCacheEntry);
            return cached.DestructureFunc(value, propertyValueFactory);
        }

        static CacheEntry CreateCacheEntry(Type type) {
            // make sure all non-scalar types are cached
            foreach (var property in type.GetPropertiesRecursive()) {
                if (!BuiltInScalarTypes.Contains(property.PropertyType)) {
                    _cache.GetOrAdd(property.PropertyType, CreateCacheEntry);
                }
            }

            return new CacheEntry((o, f) => MakeStructure(o));
        }

        static LogEventPropertyValue MakeStructure(object o) {
            var json = JsonConvert.SerializeObject(o);
            return new ScalarValue(json);
        }
    }
}
