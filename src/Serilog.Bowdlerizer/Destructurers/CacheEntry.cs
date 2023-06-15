using System;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Bowdlerizer.Destructurers {
    struct CacheEntry {
        public CacheEntry(Func<object, ILogEventPropertyValueFactory, LogEventPropertyValue> destructureFunc) {
            CanDestructure = true;
            DestructureFunc = destructureFunc ?? throw new ArgumentNullException(nameof(destructureFunc));
        }

        private CacheEntry(bool canDestructure, Func<object, ILogEventPropertyValueFactory, LogEventPropertyValue> destructureFunc) {
            CanDestructure = canDestructure;
            DestructureFunc = destructureFunc ?? throw new ArgumentNullException(nameof(destructureFunc));
        }

        public bool CanDestructure { get; }

        public Func<object, ILogEventPropertyValueFactory, LogEventPropertyValue> DestructureFunc { get; }

        public static CacheEntry Ignore { get; } = new CacheEntry(false, (o, f) => null);
    }
}
