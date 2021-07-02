using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Bowdlerizer.Destructurers {
    public static class JTokenDestructurer {
        public static LogEventPropertyValue GetValues(ILogEventPropertyValueFactory propertyValueFactory, JToken token) {
            LogEventPropertyValue result;
            var structureProperties = new List<LogEventProperty>();
            foreach (var o in token.Children()) {
                if (o is JProperty property) {
                    var key = property.Name;
                    object value;
                    LogEventProperty p = null;
                    if (property.Value is JValue pvalue) {
                        value = pvalue.Value;
                        p = new LogEventProperty(key, propertyValueFactory.CreatePropertyValue(value, true));
                    } else if (property.Value is JObject) {
                        var values = GetValues(propertyValueFactory, property.Value);
                        var properties = ((StructureValue)values).Properties;
                        var pv = new StructureValue(properties, "JToken");
                        p = new LogEventProperty(key, pv);
                    }
                    structureProperties.Add(p);
                }
            }

            result = new StructureValue(structureProperties, "JToken");
            return result;
        }
    }
}
