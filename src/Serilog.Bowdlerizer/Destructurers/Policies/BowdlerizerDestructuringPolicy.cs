using Serilog.Core;
using Serilog.Events;

namespace Serilog.Bowdlerizer.Destructurers.Policies {
    public class BowdlerizerDestructuringPolicy : IDestructuringPolicy {
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result) {
            if (ClassDestructurer.BuiltInScalarTypes.Contains(value.GetType())) {
                result = null;
                return false;
            }

            result = ClassDestructurer.GetValues(propertyValueFactory, value);
            return true;
        }
    }
}
