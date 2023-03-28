using System;
using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Bowdlerizer.Destructurers.Policies {
    public class BowdlerizerDestructuringPolicy : IDestructuringPolicy {
        private static readonly HashSet<Type> BuiltInScalarTypes = new HashSet<Type>() {
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

        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result) {
            if (BuiltInScalarTypes.Contains(value.GetType())) {
                result = null;
                return false;
            }

            result = ClassDestructurer.GetValues(value);
            return true;
        }
    }
}
