using Newtonsoft.Json.Linq;
using Serilog.Bowdlerizer.Destructurers;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Bowdlerizer {
    class BowdlerizerDestructuringPolicy : IDestructuringPolicy {
        private readonly Cortside.Bowdlerizer.Bowdlerizer bowdlerizer;

        public BowdlerizerDestructuringPolicy(Cortside.Bowdlerizer.Bowdlerizer bowdlerizer) {
            this.bowdlerizer = bowdlerizer;
        }

        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result) {
            if (value is JToken token) {
                _ = bowdlerizer.BowdlerizeJToken(token);
                result = JTokenDestructurer.GetValues(propertyValueFactory, token);
                return true;
            } else if (JsonStringDestructurer.IsJsonString(value)) {
                var s = bowdlerizer.BowdlerizeJson(value as string);
                result = JsonStringDestructurer.GetValues(propertyValueFactory, s);
                return true;
            } else if (XmlStringDestructurer.IsXmlString(value)) {
                var s = bowdlerizer.BowdlerizeXml(value as string);
                result = JsonStringDestructurer.GetValues(propertyValueFactory, s);
                return true;
            }
            //else {
            //    result = ClassDestructurer.GetValues(value, propertyValueFactory, bowdlerizer);
            //    //return cached.CanDestructure;
            //    return true;
            //}

            result = null;
            return false;
        }
    }
}
