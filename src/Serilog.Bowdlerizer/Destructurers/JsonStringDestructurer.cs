using Newtonsoft.Json.Linq;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Bowdlerizer.Destructurers {
    public static class JsonStringDestructurer {
        public static LogEventPropertyValue GetValues(ILogEventPropertyValueFactory propertyValueFactory, string s) {
            var token = JToken.Parse(s);
            return JTokenDestructurer.GetValues(propertyValueFactory, token);
        }

        public static bool IsJsonString(object value) {
            if (value is string s) {
                s = s.Trim();
                // TODO: this is very poor/simple check
                if (s.StartsWith("{") && s.EndsWith("}")) {
                    return true;
                }
            }
            return false;
        }

        //private static Dictionary<string, object> DeserializeToDictionary(string jo) {
        //    var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(jo);
        //    var values2 = new Dictionary<string, object>();
        //    foreach (KeyValuePair<string, object> d in values) {
        //        if (d.Value is JObject) {
        //            values2.Add(d.Key, DeserializeToDictionary(d.Value.ToString()));
        //        } else {
        //            values2.Add(d.Key, d.Value);
        //        }
        //    }
        //    return values2;
        //}
    }
}
