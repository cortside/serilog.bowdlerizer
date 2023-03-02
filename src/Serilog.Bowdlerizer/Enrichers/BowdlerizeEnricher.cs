using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Bowdlerizer.Enrichers {
    public class BowdlerizeEnricher : ILogEventEnricher {
        private readonly Cortside.Bowdlerizer.Bowdlerizer bowdlerizer;
        private readonly FieldInfo logEventValueProperty;

        public BowdlerizeEnricher(Cortside.Bowdlerizer.Bowdlerizer bowdlerizer) {
            this.bowdlerizer = bowdlerizer;

            var fields = typeof(LogEventProperty).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            logEventValueProperty = fields.SingleOrDefault(f => f.Name.Contains("Value"));
        }

        /// <summary>
        /// Enrich the log event.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) {
            var changed = new Dictionary<string, LogEventPropertyValue>();
            foreach (var property in logEvent.Properties) {
                if (property.Value is ScalarValue scalar) {
                    if (IsJsonString(scalar.Value)) {
                        var s = bowdlerizer.BowdlerizeJson(scalar.Value.ToString());
                        var v = new ScalarValue(s);
                        var key = property.Key;
                        changed.Add(key, v);
                    } else if (IsXmlString(scalar.Value)) {
                        var s = bowdlerizer.BowdlerizeXml(scalar.Value as string);
                        var v = new ScalarValue(s);
                        var key = property.Key;
                        changed.Add(key, v);
                    }
                }
            }

            foreach (var change in changed) {
                logEvent.AddOrUpdateProperty(new LogEventProperty(change.Key, change.Value));
            }

            Bowdlerize(logEvent);
        }

        private void Bowdlerize(LogEvent logEvent) {
            var paths = bowdlerizer.Paths();
            foreach (var s in paths) {
                foreach (var property in logEvent.Properties) {
                    if (property.Value is StructureValue sv) {
                        Bowdlerize(sv, s);
                    }
                }
            }
        }

        private void Bowdlerize(StructureValue sv, string s) {
            if (TryGetProperty(sv, s, out LogEventProperty result)) {
                SetValue(result);
            }
        }

        private void SetValue(LogEventProperty p) {
            if (p != null) {
                logEventValueProperty.SetValue(p, new ScalarValue("***"));
            }
        }

        private static bool TryGetProperty(StructureValue root, string path, out LogEventProperty value) {
            path = path.Replace("$..", "");
            var properties = path.Split('.');

            return TryGetProperty(root, properties, out value);
        }

        private static bool TryGetProperty(LogEventPropertyValue sv, IEnumerable<string> properties, out LogEventProperty value) {
            LogEventProperty root = new LogEventProperty("root", sv);
            foreach (var property in properties) {
                if (root.Value is StructureValue svx) {
                    var p = svx.Properties.FirstOrDefault(x => x != null && x.Name == property);
                    if (p != null) {
                        root = p;
                    } else {
                        value = null;
                        return false;
                    }
                }
            }
            value = root;
            return true;
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

        public static bool IsXmlString(object value) {
            if (value is string s) {
                s = s.Trim();
                if (s.StartsWith("<") && s.EndsWith(">")) {
                    return true;
                }
            }
            return false;
        }
    }
}
