using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Bowdlerizer.Destructurers {
    public static class XmlStringDestructurer {
        public static LogEventPropertyValue GetValues(ILogEventPropertyValueFactory propertyValueFactory, string s) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(s);
            var json = JsonConvert.SerializeXmlNode(doc);
            var token = JToken.Parse(json);

            return JTokenDestructurer.GetValues(propertyValueFactory, token);
        }

        public static bool IsXmlString(object value) {
            if (value is string s) {
                s = s.Trim();
                // TODO: this is very poor/simple check
                if (s.StartsWith("<?xml") && s.EndsWith(">")) {
                    return true;
                }
            }
            return false;
        }
    }
}
