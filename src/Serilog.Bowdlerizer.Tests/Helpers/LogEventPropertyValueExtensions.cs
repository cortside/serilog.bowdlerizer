using Serilog.Events;

namespace Serilog.Bowdlerizer.Tests.Helpers {
    public static class LogEventPropertyValueExtensions {
        public static object LiteralValue(this LogEventPropertyValue @this) {
            return ((ScalarValue)@this).Value;
        }
    }
}
