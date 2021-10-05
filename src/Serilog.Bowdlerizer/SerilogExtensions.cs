using System;
using Serilog.Bowdlerizer.Enrichers;
using Serilog.Configuration;

namespace Serilog.Bowdlerizer {
    public static class SerilogExtensions {
        public static LoggerConfiguration UsingBowdlerizer(this LoggerDestructuringConfiguration configuration, Cortside.Bowdlerizer.Bowdlerizer bowdlerizer) {
            if (configuration == null) {
                throw new ArgumentNullException(nameof(configuration));
            }

            return configuration.With(new[] { new BowdlerizerDestructuringPolicy(bowdlerizer) });
        }

        public static LoggerConfiguration WithBowdlerizer(this LoggerEnrichmentConfiguration enrich, Cortside.Bowdlerizer.Bowdlerizer bowdlerizer) {
            if (enrich == null) {
                throw new ArgumentNullException(nameof(enrich));
            }

            return enrich.With(new BowdlerizeEnricher(bowdlerizer));
        }
    }
}
