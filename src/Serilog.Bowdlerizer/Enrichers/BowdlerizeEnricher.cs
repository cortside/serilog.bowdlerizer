using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Bowdlerizer.Destructurers;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Bowdlerizer.Enrichers {
    public class BowdlerizeEnricher : ILogEventEnricher {
        private readonly Cortside.Bowdlerizer.Bowdlerizer bowdlerizer;
        private readonly FieldInfo logEventValueProperty;
        private static ILogEventPropertyValueFactory propertyValueConverter;

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
                    if (JsonStringDestructurer.IsJsonString(scalar.Value)) {
                        GetPropertyValueConverter(propertyFactory);

                        var s = scalar.Value as string;
                        LogEventPropertyValue v = null;
                        var token = JToken.Parse(s);
                        if (token.Type == JTokenType.Array) {
                            var propsList = new List<LogEventPropertyValue>();
                            foreach (var item in token.Children()) {
                                _ = bowdlerizer.BowdlerizeJToken(item);
                                var values = JTokenDestructurer.GetValues(propertyValueConverter, item);
                                var properties = ((StructureValue)values).Properties;
                                var pv = new StructureValue(properties, "JToken");
                                propsList.Add(pv);
                            }
                            v = new SequenceValue(propsList);
                        } else {
                            _ = bowdlerizer.BowdlerizeJToken(token);
                            v = JTokenDestructurer.GetValues(propertyValueConverter, token) as StructureValue;
                        }
                        var key = property.Key;
                        changed.Add(key, v);
                    } else if (XmlStringDestructurer.IsXmlString(scalar.Value)) {
                        GetPropertyValueConverter(propertyFactory);

                        var s = scalar.Value as string;
                        var doc = new XmlDocument();
                        doc.LoadXml(s);
                        var json = JsonConvert.SerializeXmlNode(doc);
                        var token = JToken.Parse(json);
                        _ = bowdlerizer.BowdlerizeJToken(token);

                        XNode node = JsonConvert.DeserializeXNode(token.ToString(Newtonsoft.Json.Formatting.None));
                        var v = new ScalarValue(node.ToString(SaveOptions.DisableFormatting));

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

        private static void GetPropertyValueConverter(ILogEventPropertyFactory propertyFactory) {
            if (propertyValueConverter == null) {
                var fields = propertyFactory.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                var o = fields.SingleOrDefault(f => f.Name == "_propertyValueConverter");
                propertyValueConverter = o.GetValue(propertyFactory) as ILogEventPropertyValueFactory;
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
                        //p = svx.Properties.FirstOrDefault(x => x != null && property == x.Name + "[*]");
                        //if (p != null) {
                        //    var hello = "hello";
                        //}
                        value = null;
                        return false;
                    }
                }
            }
            value = root;
            return true;
        }
    }
}
