using System;
using Newtonsoft.Json;
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
                if ((s.StartsWith("{") && s.EndsWith("}")) || (s.StartsWith("[") && s.EndsWith("]"))) {
                    {
                        try {
                            var obj = JToken.Parse(s);
                            return true;
                        } catch (JsonReaderException jex) {
                            //Exception in parsing json
                            return false;
                        } catch (Exception ex) //some other exception
                          {
                            return false;
                        }

                    }
                }
                return false;
            }
            return false;
        }
    }
}
