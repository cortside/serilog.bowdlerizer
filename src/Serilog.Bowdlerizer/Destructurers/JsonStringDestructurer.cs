using System;
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
            if (!(value is string s)) {
                return false;
            }

            s = s.Trim();
            if ((!s.StartsWith("{") || !s.EndsWith("}")) && (!s.StartsWith("[") || !s.EndsWith("]"))) {
                return false;
            }

            try {
                _ = JToken.Parse(s);
                return true;
            } catch (Exception) {
                return false;
            }
        }
    }
}
