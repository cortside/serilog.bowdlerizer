using Serilog.Core;
using Serilog.Events;

namespace Serilog.Bowdlerizer {
    public interface IBowdlerizeAttribute {
        bool TryBowdlerizeLogEventProperty(string name, object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventProperty property);
    }
}
