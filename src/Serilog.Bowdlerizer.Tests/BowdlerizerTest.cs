using System.Collections.Generic;
using System.Linq;
using Cortside.Bowdlerizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Bowdlerizer.Tests.Helpers;
using Serilog.Bowdlerizer.Tests.Models;
using Serilog.Events;
using Xunit;

namespace Serilog.Bowdlerizer.Tests {
    public class BowdlerizerTest {
        private readonly Cortside.Bowdlerizer.Bowdlerizer bowdlerizer;

        public BowdlerizerTest() {
            bowdlerizer = new Cortside.Bowdlerizer.Bowdlerizer(new List<BowdlerizerRule> {
                new BowdlerizerRule() { Path = "$..['SearchBy.Name.First']", Strategy = new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..['SearchBy.Name.Last']", Strategy = new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..['SearchBy.SSN']", Strategy = new BowdlerizerTailStrategy(0) },
                new BowdlerizerRule() { Path = "$..Name.First", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..Name.Last", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..FirstName", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..LastName", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..BirthDate", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..SocialSecurityNumber", Strategy=new BowdlerizerTailStrategy(0) },
                new BowdlerizerRule() { Path = "$..MailingAddress.Address1", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..MailingAddress.City", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..SSN", Strategy=new BowdlerizerTailStrategy(0) },
                new BowdlerizerRule() { Path = "$..DOB.Year", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..DOB.Month", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..name-first", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..name-last", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..ssn", Strategy=new BowdlerizerTailStrategy(0) },
            });
        }

        [Fact]
        public void DestructureJToken() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingBowdlerizer(bowdlerizer)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var model = new Person {
                FirstName = "John",
                LastName = "Doe",
                SocialSecurityNumber = "123456789",
                EmailAddress = "foo@bar.baz",
                PhoneNumber = "(801) 867-5309",
                MailingAddress = new Address() { Address1 = "123 Main", Address2 = "c/o Elmer" }
            };

            log.Information("Here is {@json}", JToken.FromObject(model));

            var sv = (StructureValue)evt.Properties["json"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.NotEqual(model.BirthDate.ToString(), props["BirthDate"].LiteralValue());
            Assert.Equal(model.EmailAddress, props["EmailAddress"].LiteralValue());

            var expected = "Here is JToken { PersonId: 0, MailingAddress: JToken { Address1: \"***\", Address2: \"c/o Elmer\", City: \"\", State: null, Zip: null }, FirstName: \"***\", EmailAddress: \"foo@bar.baz\", LastName: \"***\", SocialSecurityNumber: \"***\", Suffix: null, PhoneNumber: \"(801) 867-5309\", BirthDate: \"***\" }";
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BowdlerizeObject() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingBowdlerizer(bowdlerizer)
                .Enrich.WithBowdlerizer(bowdlerizer)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var model = new Person {
                FirstName = "John",
                LastName = "Doe",
                SocialSecurityNumber = "123456789",
                EmailAddress = "foo@bar.baz",
                PhoneNumber = "(801) 867-5309",
                MailingAddress = new Address() { Address1 = "123 Main", Address2 = "c/o Elmer", City = "Salt Lake City" }
            };

            log.Information("Here is {@Ignored}", model);

            var sv = (StructureValue)evt.Properties["Ignored"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            var property = ((StructureValue)sv.Properties.FirstOrDefault(x => x.Name == "MailingAddress").Value).Properties.FirstOrDefault(x => x.Name == "City");

            var scalar = ((StructureValue)props["MailingAddress"]).Properties.ToDictionary(p => p.Name, p => p.Value)["City"] as ScalarValue;
            Assert.Equal("***", scalar.LiteralValue());

            Assert.NotEqual(model.BirthDate.ToString(), props["BirthDate"].LiteralValue());
            Assert.Equal(model.EmailAddress, props["EmailAddress"].LiteralValue());

            var expected = "Here is Person { PersonId: 0, MailingAddress: Address { Address1: \"***\", Address2: \"c/o Elmer\", City: \"***\", State: null, Zip: null }, FirstName: \"***\", EmailAddress: \"foo@bar.baz\", LastName: \"***\", SocialSecurityNumber: \"***\", Suffix: null, PhoneNumber: \"(801) 867-5309\", BirthDate: \"***\" }";
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BowdlerizeJsonString() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingBowdlerizer(bowdlerizer)
                .Enrich.WithBowdlerizer(bowdlerizer)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var model = new Person {
                FirstName = "John",
                LastName = "Doe",
                SocialSecurityNumber = "123456789",
                EmailAddress = "foo@bar.baz",
                PhoneNumber = "(801) 867-5309",
                MailingAddress = new Address() { Address1 = "123 Main", Address2 = "c/o Elmer", City = "Salt Lake City" }
            };

            log.Information("Here is {@Ignored}", JsonConvert.SerializeObject(model));

            var sv = (StructureValue)evt.Properties["Ignored"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            var property = ((StructureValue)sv.Properties.FirstOrDefault(x => x.Name == "MailingAddress").Value).Properties.FirstOrDefault(x => x.Name == "City");

            var scalar = ((StructureValue)props["MailingAddress"]).Properties.ToDictionary(p => p.Name, p => p.Value)["City"] as ScalarValue;
            Assert.Equal("***", scalar.LiteralValue());

            Assert.NotEqual(model.BirthDate.ToString(), props["BirthDate"].LiteralValue());
            Assert.Equal(model.EmailAddress, props["EmailAddress"].LiteralValue());

            var expected = "Here is JToken { PersonId: 0, MailingAddress: JToken { Address1: \"***\", Address2: \"c/o Elmer\", City: \"***\", State: null, Zip: null }, FirstName: \"***\", EmailAddress: \"foo@bar.baz\", LastName: \"***\", SocialSecurityNumber: \"***\", Suffix: null, PhoneNumber: \"(801) 867-5309\", BirthDate: \"***\" }";
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }
    }
}
