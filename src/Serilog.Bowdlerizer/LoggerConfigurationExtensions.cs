using System;
using Serilog.Bowdlerizer.Destructurers.Policies;
using Serilog.Bowdlerizer.Enrichers;
using Serilog.Configuration;

namespace Serilog.Bowdlerizer {
    public static class LoggerConfigurationExtensions {
        public static LoggerConfiguration UsingBowdlerizer(this LoggerConfiguration configuration, Cortside.Bowdlerizer.Bowdlerizer bowdlerizer) {
            if (configuration == null) {
                throw new ArgumentNullException(nameof(configuration));
            }

            configuration.Destructure.UsingBowdlerizer(bowdlerizer);
            configuration.Enrich.WithBowdlerizer(bowdlerizer);

            return configuration;
        }

        public static LoggerConfiguration UsingBowdlerizer(this LoggerDestructuringConfiguration configuration, Cortside.Bowdlerizer.Bowdlerizer bowdlerizer) {
            if (configuration == null) {
                throw new ArgumentNullException(nameof(configuration));
            }

            return configuration.With(new BowdlerizerDestructuringPolicy());
        }

        public static LoggerConfiguration WithBowdlerizer(this LoggerEnrichmentConfiguration enrich, Cortside.Bowdlerizer.Bowdlerizer bowdlerizer) {
            if (enrich == null) {
                throw new ArgumentNullException(nameof(enrich));
            }

            return enrich.With(new BowdlerizeEnricher(bowdlerizer));
        }
    }
}
