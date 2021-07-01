using System;

namespace Serilog.Bowdlerizer.Tests.Models {
    public class Person {

        public Person() {
            BirthDate = new DateTime();
        }

        public int PersonId { get; set; }
        public Address MailingAddress { get; set; }
        [BowdlerizeMask]
        public string FirstName { get; set; }
        public string EmailAddress { get; set; }
        [BowdlerizeMask]
        public string LastName { get; set; }
        [BowdlerizeMask(Mask = "REDACTED")]
        public string SocialSecurityNumber { get; set; }
        public string Suffix { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}
