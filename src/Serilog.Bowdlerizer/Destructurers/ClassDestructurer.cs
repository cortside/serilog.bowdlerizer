using Newtonsoft.Json;
using Serilog.Events;

namespace Serilog.Bowdlerizer.Destructurers {
    public static class ClassDestructurer {
        internal static LogEventPropertyValue GetValues(object value) {
            var json = JsonConvert.SerializeObject(value);
            return new ScalarValue(json);
        }
    }
}
