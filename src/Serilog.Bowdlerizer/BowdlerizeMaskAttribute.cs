using System;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Bowdlerizer {
    [AttributeUsage(AttributeTargets.Property)]
    public class BowdlerizeMaskAttribute : Attribute, IBowdlerizeAttribute {
        const string DefaultMask = "***";

        public string Mask { get; set; } = DefaultMask;
        public int ShowFirst { get; set; }
        public int ShowLast { get; set; }
        public bool PreserveLength { get; set; }

        /// <summary>
        /// Check to see if custom Text has been provided.
        /// If true PreserveLength is ignored.
        /// </summary>
        /// <returns></returns>
        internal bool IsDefaultMask() {
            return Mask == DefaultMask;
        }

        internal object ObfuscateValue(object propValue) {
            var val = propValue as string;

            if (string.IsNullOrEmpty(val)) {
                return val;
            }

            if (ShowFirst == 0 && ShowLast == 0) {
                if (PreserveLength) {
                    return new string(Mask[0], val.Length);
                }

                return Mask;
            }

            if (ShowFirst > 0 && ShowLast == 0) {
                var first = val.Substring(0, Math.Min(ShowFirst, val.Length));

                if (!PreserveLength || !IsDefaultMask()) {
                    return first + Mask;
                }

                var mask = "";
                if (ShowFirst <= val.Length) {
                    mask = new string(Mask[0], val.Length - ShowFirst);
                }

                return first + mask;

            }

            if (ShowFirst == 0 && ShowLast > 0) {
                var last = ShowLast > val.Length ? val : val.Substring(val.Length - ShowLast);

                if (!PreserveLength || !IsDefaultMask()) {
                    return Mask + last;
                }

                var mask = "";
                if (ShowLast <= val.Length) {
                    mask = new string(Mask[0], val.Length - ShowLast);
                }

                return mask + last;
            }

            if (ShowFirst > 0 && ShowLast > 0) {
                if (ShowFirst + ShowLast >= val.Length) {
                    return val;
                }

                var first = val.Substring(0, ShowFirst);
                var last = val.Substring(val.Length - ShowLast);

                return first + Mask + last;
            }

            return propValue;
        }

        public bool TryBowdlerizeLogEventProperty(string name, object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventProperty property) {
            property = new LogEventProperty(name, new ScalarValue(ObfuscateValue(value)));
            return true;
        }
    }
}
