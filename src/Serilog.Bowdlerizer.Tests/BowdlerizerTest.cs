using System.Collections.Generic;
using System.Linq;
using Cortside.Bowdlerizer;
using Newtonsoft.Json;
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
                new BowdlerizerRule() { Path = "$..['SearchBy.SSN']", Strategy = new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..Name.First", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..Name.Last", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..FirstName", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..LastName", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..BirthDate", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..SocialSecurityNumber", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..MailingAddress.Address1", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..MailingAddress.City", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..Addresses[*].Address1", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..Addresses[*].City", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..SSN", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..DOB.Year", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..DOB.Month", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..DOB.Day", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..name-first", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..name-last", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..ssn", Strategy=new BowdlerizerHeadStrategy(0) },
                new BowdlerizerRule() { Path = "$..Address1", Strategy=new BowdlerizerHeadStrategy(0) },
            });
        }

        private string QuoteExpectedString(string prefix, string s) {
            var expected = s;
            expected = expected.Replace("\"", "\\\"");
            expected = $"{prefix}\"{expected}\"";

            return expected;
        }

        //[Fact]
        //public void DestructureJToken() {
        //    LogEvent evt = null;

        //    var log = new LoggerConfiguration()
        //        .Enrich.WithBowdlerizer(bowdlerizer)
        //        .WriteTo.Sink(new DelegatingSink(e => evt = e))
        //        .CreateLogger();

        //    var model = new Person {
        //        FirstName = "John",
        //        LastName = "Doe",
        //        SocialSecurityNumber = "123456789",
        //        EmailAddress = "foo@bar.baz",
        //        PhoneNumber = "(801) 867-5309",
        //        MailingAddress = new Address() { Address1 = "123 Main", Address2 = "c/o Elmer" }
        //    };

        //    log.Information("Here is {@json}", JToken.FromObject(model));

        //    var sv = (ScalarValue)evt.Properties["json"];
        //    //var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        //    //Assert.NotEqual(model.BirthDate.ToString(), props["BirthDate"].LiteralValue());
        //    //Assert.Equal(model.EmailAddress, props["EmailAddress"].LiteralValue());

        //    var expected = QuoteExpectedString("Here is ", @"{""PersonId"":0,""MailingAddress"":{""Address1"":""***"",""Address2"":""c/o Elmer"",""City"":"""",""State"":null,""Zip"":null},""FirstName"":""***"",""EmailAddress"":""foo@bar.baz"",""LastName"":""***"",""SocialSecurityNumber"":""***"",""Suffix"":null,""PhoneNumber"":""(801) 867-5309"",""BirthDate"":""***"",""Children"":[],""Addresses"":null}");
        //    //var expected = QuoteExpectedString("Here is ", @"{""PersonId"":0,""MailingAddress"":{""Address1"":""***"",""Address2"":""c/o Elmer"",""City"":"""",""State"":null,""Zip"":null},""FirstName"":""***"",""EmailAddress"":""foo@bar.baz"",""LastName"":""***"",""SocialSecurityNumber"":""***"",""Suffix: null, PhoneNumber: \"(801) 867-5309\", BirthDate: \"***\", Children: [], Addresses: null }");
        //    var actual = evt.RenderMessage();
        //    Assert.Equal(expected, actual);
        //}

        [Fact]
        public void DestructureJson() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Enrich.WithBowdlerizer(bowdlerizer)
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

            log.Information("Here is {@json}", JsonConvert.SerializeObject(model));

            var sv = (ScalarValue)evt.Properties["json"];

            var expected = QuoteExpectedString("Here is ", @"{""PersonId"":0,""MailingAddress"":{""Address1"":""***"",""Address2"":""c/o Elmer"",""City"":"""",""State"":null,""Zip"":null},""FirstName"":""***"",""EmailAddress"":""foo@bar.baz"",""LastName"":""***"",""SocialSecurityNumber"":""***"",""Suffix"":null,""PhoneNumber"":""(801) 867-5309"",""BirthDate"":""***"",""Children"":[],""Addresses"":null}");
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BowdlerizeObject() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Enrich.WithBowdlerizer(bowdlerizer)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var model = new Person {
                FirstName = "John",
                LastName = "Doe",
                SocialSecurityNumber = "123456789",
                EmailAddress = "foo@bar.baz",
                PhoneNumber = "(801) 867-5309",
                MailingAddress = new Address() { Address1 = "123 Main", Address2 = "c/o Elmer", City = "Salt Lake City" },
            };

            log.Information("Here is {@Ignored}", model);

            var sv = (StructureValue)evt.Properties["Ignored"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            var scalar = ((StructureValue)props["MailingAddress"]).Properties.ToDictionary(p => p.Name, p => p.Value)["City"] as ScalarValue;
            Assert.Equal("***", scalar.LiteralValue());

            Assert.NotEqual(model.BirthDate.ToString(), props["BirthDate"].LiteralValue());
            Assert.Equal(model.EmailAddress, props["EmailAddress"].LiteralValue());

            var expected = "Here is Person { PersonId: 0, MailingAddress: Address { Address1: \"***\", Address2: \"c/o Elmer\", City: \"***\", State: null, Zip: null }, FirstName: \"***\", EmailAddress: \"foo@bar.baz\", LastName: \"***\", SocialSecurityNumber: \"***\", Suffix: null, PhoneNumber: \"(801) 867-5309\", BirthDate: \"***\", Children: [], Addresses: null }";
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }

        [Fact(Skip = "Focus on JSON strings first")]
        public void BowdlerizeListObject() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Enrich.WithBowdlerizer(bowdlerizer)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var model = new List<Person>{
                new Person {
                    FirstName = "John",
                    LastName = "Doe",
                    SocialSecurityNumber = "123456789",
                    EmailAddress = "foo@bar.baz",
                    PhoneNumber = "(801) 867-5309",
                    MailingAddress = new Address() { Address1 = "123 Main", Address2 = "c/o Elmer", City = "Salt Lake City" },
                },
                new Person {
                    FirstName = "Jane"
                }
            };

            log.Information("Here is {@Ignored}", model);

            var sv = (SequenceValue)evt.Properties["Ignored"];

            var expected = "Here is [Person { PersonId: 0, MailingAddress: Address { Address1: \"***\", Address2: \"c/o Elmer\", City: \"***\", State: null, Zip: null }, FirstName: \"***\", EmailAddress: \"foo@bar.baz\", LastName: \"***\", SocialSecurityNumber: \"****\", Suffix: null, PhoneNumber: \"(801) 867-5309\", BirthDate: \"****\", Addresses: null }, Person { PersonId: 0, MailingAddress: null, FirstName: \"***\", EmailAddress: null, LastName: null, SocialSecurityNumber: null, Suffix: null, PhoneNumber: null, BirthDate: 01/01/0001 00:00:00, Addresses: null }]";
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }

        [Fact(Skip = "Focus on JSON strings first")]
        public void BowdlerizeObjectWithNestedList() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Enrich.WithBowdlerizer(bowdlerizer)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var model = new Person {
                Addresses = new List<Address> {
                    new Address() { Address1 = "123 Main", Address2 = "c/o Elmer", City = "Salt Lake City" },
                    new Address() { Address1 = "234 Main", Address2 = "c/o Elmer", City = "Vernal" },
                }
            };

            log.Information("Here is {@Ignored}", model);

            var sv = (StructureValue)evt.Properties["Ignored"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            var expected = "Here is Person { PersonId: 0, MailingAddress: \"***\", FirstName: \"***\", EmailAddress: null, LastName: \"***\", SocialSecurityNumber: \"***\", Suffix: null, PhoneNumber: null, BirthDate: \"***\", Addresses: [Address { Address1: \"***\", Address2: \"c/o Elmer\", City: \"***\", State: null, Zip: null }, Address { Address1: \"***\", Address2: \"c/o Elmer\", City: \"***\", State: null, Zip: null }] }";
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }

        //[Fact]
        //public void BowdlerizeJsonString() {
        //    LogEvent evt = null;

        //    var log = new LoggerConfiguration()
        //        .Enrich.WithBowdlerizer(bowdlerizer)
        //        .WriteTo.Sink(new DelegatingSink(e => evt = e))
        //        .CreateLogger();

        //    var model = new Person {
        //        FirstName = "John",
        //        LastName = "Doe",
        //        SocialSecurityNumber = "123456789",
        //        EmailAddress = "foo@bar.baz",
        //        PhoneNumber = "(801) 867-5309",
        //        MailingAddress = new Address() { Address1 = "123 Main", Address2 = "c/o Elmer", City = "Salt Lake City" }
        //    };

        //    log.Information("Here is {@Ignored}", JsonConvert.SerializeObject(model));

        //    var sv = (ScalarValue)evt.Properties["Ignored"];
        //    //var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        //    //var property = ((StructureValue)sv.Properties.FirstOrDefault(x => x.Name == "MailingAddress").Value).Properties.FirstOrDefault(x => x.Name == "City");

        //    //var scalar = ((StructureValue)props["MailingAddress"]).Properties.ToDictionary(p => p.Name, p => p.Value)["City"] as ScalarValue;
        //    //Assert.Equal("***", scalar.LiteralValue());

        //    //Assert.NotEqual(model.BirthDate.ToString(), props["BirthDate"].LiteralValue());
        //    //Assert.Equal(model.EmailAddress, props["EmailAddress"].LiteralValue());

        //    var expected = "Here is { PersonId: 0, MailingAddress: { Address1: \"***\", Address2: \"c/o Elmer\", City: \"***\", State: null, Zip: null }, FirstName: \"***\", EmailAddress: \"foo@bar.baz\", LastName: \"***\", SocialSecurityNumber: \"***\", Suffix: null, PhoneNumber: \"(801) 867-5309\", BirthDate: \"***\", Children: [], Addresses: null }";
        //    var actual = evt.RenderMessage();
        //    Assert.Equal(expected, actual);
        //}

        [Fact]
        public void BowdlerizeJsonListString() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Enrich.WithBowdlerizer(bowdlerizer)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var model = new List<Person> { new Person {
                    FirstName = "John",
                    LastName = "Doe",
                    SocialSecurityNumber = "123456789",
                    EmailAddress = "foo@bar.baz",
                    PhoneNumber = "(801) 867-5309",
                    MailingAddress = new Address() { Address1 = "123 Main", Address2 = "c/o Elmer", City = "Salt Lake City" }
                },
                new Person{ FirstName = "blergify"}
            };

            log.Information("Here is {@Ignored}", JsonConvert.SerializeObject(model));

            var sv = (ScalarValue)evt.Properties["Ignored"];

            var expected = QuoteExpectedString("Here is ", @"[{""PersonId"":0,""MailingAddress"":{""Address1"":""***"",""Address2"":""c/o Elmer"",""City"":""***"",""State"":null,""Zip"":null},""FirstName"":""***"",""EmailAddress"":""foo@bar.baz"",""LastName"":""***"",""SocialSecurityNumber"":""***"",""Suffix"":null,""PhoneNumber"":""(801) 867-5309"",""BirthDate"":""***"",""Children"":[],""Addresses"":null},{""PersonId"":0,""MailingAddress"":null,""FirstName"":""***"",""EmailAddress"":null,""LastName"":"""",""SocialSecurityNumber"":"""",""Suffix"":null,""PhoneNumber"":null,""BirthDate"":""***"",""Children"":[],""Addresses"":null}]");
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BowdlerizeJsonStringWithNestedList() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Enrich.WithBowdlerizer(bowdlerizer)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var model = new Person {
                Addresses = new List<Address> {
                    new Address() { Address1 = "123 Main", Address2 = "c/o Elmer", City = "Salt Lake City" },
                    new Address() { Address1 = "234 Main", Address2 = "c/o Candy", City = "Las Vegas" },
                }
            };

            log.Information("Here is {@Ignored}", JsonConvert.SerializeObject(model));

            var sv = (ScalarValue)evt.Properties["Ignored"];
            //var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);


            var expected = QuoteExpectedString("Here is ", @"{""PersonId"":0,""MailingAddress"":null,""FirstName"":"""",""EmailAddress"":null,""LastName"":"""",""SocialSecurityNumber"":"""",""Suffix"":null,""PhoneNumber"":null,""BirthDate"":""***"",""Children"":[],""Addresses"":[{""Address1"":""***"",""Address2"":""c/o Elmer"",""City"":""***"",""State"":null,""Zip"":null},{""Address1"":""***"",""Address2"":""c/o Candy"",""City"":""***"",""State"":null,""Zip"":null}]}");
            //var expected = "Here is { PersonId: 0, MailingAddress: \"***\", FirstName: \"***\", EmailAddress: null, LastName: \"***\", SocialSecurityNumber: \"***\", Suffix: null, PhoneNumber: null, BirthDate: \"***\", Children: [], Addresses: [{ Address1: \"***\", Address2: \"c/o Elmer\", City: \"***\", State: null, Zip: null }, { Address1: \"***\", Address2: \"c/o Candy\", City: \"***\", State: null, Zip: null }] }";
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BowdlerizeXmlString() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Enrich.WithBowdlerizer(bowdlerizer)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><InstantIDResponseEx><response><Header><Status>0</Status></Header><Result><InputEcho><Name><First>Chester</First><Last>Tester</Last></Name><Address><StreetAddress1>cvi=0;</StreetAddress1><City>salt lake</City><State>UT</State><Zip5>84106</Zip5></Address><DOB><Year>1990</Year><Month>11</Month><Day>11</Day></DOB><SSN>800120850</SSN><HomePhone>8015556666</HomePhone><UseDOBFilter>1</UseDOBFilter><DOBRadius>1</DOBRadius><Passport><Number></Number><ExpirationDate/><Country></Country><MachineReadableLine1></MachineReadableLine1><MachineReadableLine2></MachineReadableLine2></Passport><Channel></Channel><OwnOrRent></OwnOrRent></InputEcho></Result></response></InstantIDResponseEx>";
            log.Information("Here is {xml}", xml);

            var expected = QuoteExpectedString("Here is ", "<InstantIDResponseEx><response><Header><Status>0</Status></Header><Result><InputEcho><Name><First>***</First><Last>***</Last></Name><Address><StreetAddress1>cvi=0;</StreetAddress1><City>salt lake</City><State>UT</State><Zip5>84106</Zip5></Address><DOB><Year>***</Year><Month>***</Month><Day>***</Day></DOB><SSN>***</SSN><HomePhone>8015556666</HomePhone><UseDOBFilter>1</UseDOBFilter><DOBRadius>1</DOBRadius><Passport><Number></Number><ExpirationDate /><Country></Country><MachineReadableLine1></MachineReadableLine1><MachineReadableLine2></MachineReadableLine2></Passport><Channel></Channel><OwnOrRent></OwnOrRent></InputEcho></Result></response></InstantIDResponseEx>");
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }

        [Fact(Skip = "Fails due to JSON parsing different than object")]
        public void BowdlerizePersonWithNestedList() {
            var personWithKids = new Person {
                FirstName = "tfName",
                LastName = "tlName",
                EmailAddress = "testemail@email.com",
                SocialSecurityNumber = "123456",
                Addresses = new List<Address> {
                    new Address() { Address1 = "123 Main", Address2 = "c/o Elmer", City = "Salt Lake City" },
                    new Address() { Address1 = "234 Main", Address2 = "c/o Candy", City = "Las Vegas" },
                }
            };
            personWithKids.Children.Add(new Person() {
                FirstName = "ChildFName",
                LastName = "ChildLName",
                EmailAddress = "testchildemail@email.com",
                SocialSecurityNumber = "123456",
                Children = new List<Person> { new Person() {
                    FirstName = "GrandChildFName",
                    LastName = "GrandChildLName",
                    EmailAddress = "testgchildemail@amil.com",
                    SocialSecurityNumber = "1234567"
                }
                },
                Addresses = new List<Address> {
                    new Address() { Address1 = "123 Main", Address2 = "c/o Elmer", City = "Salt Lake City" },
                    new Address() { Address1 = "234 Main", Address2 = "c/o Candy", City = "Las Vegas" },
                }
            });
            LogEvent evt = null;
            var log = new LoggerConfiguration()
            .Enrich.WithBowdlerizer(bowdlerizer)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();
            log.Information("Here is PERSON JSON: {@jsonWithKids}", JsonConvert.SerializeObject(personWithKids));
            var sv = (StructureValue)evt.Properties["jsonWithKids"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);
            var actualJSON = evt.RenderMessage();
            LogEvent evt2 = null;
            var logObject = new LoggerConfiguration()
            .Enrich.WithBowdlerizer(bowdlerizer)
            .WriteTo.Sink(new DelegatingSink(e => evt2 = e))
            .CreateLogger();
            logObject.Information("Here is PERSON OBJ: {@personWithKids}", personWithKids);
            var sv2 = (StructureValue)evt2.Properties["personWithKids"];
            var props2 = sv2.Properties.ToDictionary(p => p.Name, p => p.Value);
            var actualObject = evt2.RenderMessage();

            var expectedJSON = "Here is PERSON JSON: { PersonId: 0, MailingAddress: \"***\", FirstName: \"***\", EmailAddress: \"testemail@email.com\", LastName: \"***\", SocialSecurityNumber: \"***\", Suffix: null, PhoneNumber: null, BirthDate: \"***\", Children: [{ PersonId: 0, MailingAddress: null, FirstName: \"***\", EmailAddress: \"testchildemail@email.com\", LastName: \"***\", SocialSecurityNumber: \"***\", Suffix: null, PhoneNumber: null, BirthDate: \"***\", Children: [{ PersonId: 0, MailingAddress: null, FirstName: \"***\", EmailAddress: \"testgchildemail@amil.com\", LastName: \"***\", SocialSecurityNumber: \"***\", Suffix: null, PhoneNumber: null, BirthDate: \"***\", Children: [], Addresses: null }], Addresses: [{ Address1: \"***\", Address2: \"c/o Elmer\", City: \"***\", State: null, Zip: null }, { Address1: \"***\", Address2: \"c/o Candy\", City: \"***\", State: null, Zip: null }] }], Addresses: [{ Address1: \"***\", Address2: \"c/o Elmer\", City: \"***\", State: null, Zip: null }, { Address1: \"***\", Address2: \"c/o Candy\", City: \"***\", State: null, Zip: null }] }";
            var expectedObject = "Here is PERSON OBJ: Person { PersonId: 0, MailingAddress: \"***\", FirstName: \"***\", EmailAddress: \"testemail@email.com\", LastName: \"***\", SocialSecurityNumber: \"***\", Suffix: null, PhoneNumber: null, BirthDate: \"***\", Children: [Person { PersonId: 0, MailingAddress: null, FirstName: \"***\", EmailAddress: \"testchildemail@email.com\", LastName: \"***\", SocialSecurityNumber: \"***\", Suffix: null, PhoneNumber: null, BirthDate: \"***\", Children: [Person { PersonId: 0, MailingAddress: null, FirstName: \"***\", EmailAddress: \"testgchildemail@amil.com\", LastName: \"***\", SocialSecurityNumber: \"***\", Suffix: null, PhoneNumber: null, BirthDate: \"***\", Children: [], Addresses: null }], Addresses: [Address { Address1: \"***\", Address2: \"c/o Elmer\", City: \"***\", State: null, Zip: null }, Address { Address1: \"***\", Address2: \"c/o Candy\", City: \"***\", State: null, Zip: null }] }], Addresses: [Address { Address1: \"***\", Address2: \"c/o Elmer\", City: \"***\", State: null, Zip: null }, Address { Address1: \"***\", Address2: \"c/o Candy\", City: \"Las Vegas\", State: null, Zip: null }] }";

            //this passes and *** out all names etc in nested objects
            Assert.Equal(expectedJSON, actualJSON);

            //currently this does not pass as nested objects are not set to *** 
            Assert.Equal(expectedObject, actualObject);
        }
    }
}
