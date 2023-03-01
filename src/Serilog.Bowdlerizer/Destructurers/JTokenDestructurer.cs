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
                    } else if (property.Value.Type == JTokenType.Array) {
                        var propsList = new List<LogEventPropertyValue>();
                        foreach (var item in property.Value.Children()) {
                            var values = GetValues(propertyValueFactory, item);
                            var properties = ((StructureValue)values).Properties;
                            var pv = new StructureValue(properties);
                            propsList.Add(pv);
                        }
                        var seq = new SequenceValue(propsList);
                        p = new LogEventProperty(key, seq);

                    } else {
                        var values = GetValues(propertyValueFactory, property.Value);
                        var properties = ((StructureValue)values).Properties;
                        var pv = new StructureValue(properties);
                        p = new LogEventProperty(key, pv);
                    }
                    structureProperties.Add(p);
                }
            }

            result = new StructureValue(structureProperties);
            return result;
        }
    }
}
