using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Serilog.Bowdlerizer.Destructurers {
    public static class ClassDestructurer {
        private readonly static ConcurrentDictionary<Type, CacheEntry> _cache = new ConcurrentDictionary<Type, CacheEntry>();
        static readonly HashSet<Type> BuiltInScalarTypes = new HashSet<Type>()
        {
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

        internal static LogEventPropertyValue GetValues(object value, ILogEventPropertyValueFactory propertyValueFactory, Cortside.Bowdlerizer.Bowdlerizer bowdlerizer) {
            var cached = _cache.GetOrAdd(value.GetType(), CreateCacheEntry);
            var pv = cached.DestructureFunc(value, propertyValueFactory);
            //if (pv is StructureValue sv) {
            //    sv.Properties.ToDictionary(x => x.Name, x => x.Value);
            //}

            return pv;
        }

        static CacheEntry CreateCacheEntry(Type type) {
            //var classDestructurer = type.GetTypeInfo().GetCustomAttribute<ITypeDestructuringAttribute>();
            //if (classDestructurer != null)
            //    return new CacheEntry((o, f) => classDestructurer.CreateLogEventPropertyValue(o, f));

            var properties = type.GetPropertiesRecursive().ToList();
            //if (properties.All(pi => pi.GetCustomAttribute<IPropertyDestructuringAttribute>() == null))
            //{
            //    return CacheEntry.Ignore;
            //}

            // make sure all non-scalar types are cached
            foreach (var property in properties) {
                if (!BuiltInScalarTypes.Contains(property.PropertyType)) {
                    _cache.GetOrAdd(property.PropertyType, CreateCacheEntry);
                }
            }

            var destructuringAttributes = properties
                .Select(pi => new { pi, Attribute = pi.GetCustomAttribute<IBowdlerizeAttribute>() })
                .Where(o => o.Attribute != null)
                .ToDictionary(o => o.pi, o => o.Attribute);

            return new CacheEntry((o, f) => MakeStructure(o, properties, destructuringAttributes, f, type));
        }

        static LogEventPropertyValue MakeStructure(object o, IEnumerable<PropertyInfo> loggedProperties, IDictionary<PropertyInfo, IBowdlerizeAttribute> destructuringAttributes, ILogEventPropertyValueFactory propertyValueFactory, Type type) {
            var structureProperties = new List<LogEventProperty>();
            foreach (var pi in loggedProperties) {
                var propValue = SafeGetPropValue(o, pi);

                if (destructuringAttributes.TryGetValue(pi, out var destructuringAttribute)) {
                    if (destructuringAttribute.TryBowdlerizeLogEventProperty(pi.Name, propValue, propertyValueFactory, out var property)) {
                        structureProperties.Add(property);
                    }
                } else {
                    structureProperties.Add(new LogEventProperty(pi.Name, propertyValueFactory.CreatePropertyValue(propValue, true)));
                }
            }

            return new StructureValue(structureProperties, type.Name);
        }

        static object SafeGetPropValue(object o, PropertyInfo pi) {
            try {
                return pi.GetValue(o);
            } catch (TargetInvocationException ex) {
                SelfLog.WriteLine("The property accessor {0} threw exception {1}", pi, ex);
                return "The property accessor threw an exception: " + ex.InnerException.GetType().Name;
            }
        }
    }
}
