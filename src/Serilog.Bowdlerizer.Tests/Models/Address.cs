using Serilog.Bowdlerizer.Attributes;

namespace Serilog.Bowdlerizer.Tests.Models {
    public class Address {
        [BowdlerizeMask]
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
}
