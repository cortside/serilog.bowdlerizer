using System.Collections.Generic;
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

        [Fact]
        public void BowdlerizeJsonString() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .UsingBowdlerizer(bowdlerizer)
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

            log.Information("Here is {@Json}", JsonConvert.SerializeObject(model));

            var expected = QuoteExpectedString("Here is ", @"{""PersonId"":0,""MailingAddress"":{""Address1"":""***"",""Address2"":""c/o Elmer"",""City"":"""",""State"":null,""Zip"":null},""FirstName"":""***"",""EmailAddress"":""foo@bar.baz"",""LastName"":""***"",""SocialSecurityNumber"":""***"",""Suffix"":null,""PhoneNumber"":""(801) 867-5309"",""BirthDate"":""***"",""Children"":[],""Addresses"":null}");
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BowdlerizeObject() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .UsingBowdlerizer(bowdlerizer)
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


            var expected = QuoteExpectedString("Here is ", @"{""PersonId"":0,""MailingAddress"":{""Address1"":""***"",""Address2"":""c/o Elmer"",""City"":""***"",""State"":null,""Zip"":null},""FirstName"":""***"",""EmailAddress"":""foo@bar.baz"",""LastName"":""***"",""SocialSecurityNumber"":""***"",""Suffix"":null,""PhoneNumber"":""(801) 867-5309"",""BirthDate"":""***"",""Children"":[],""Addresses"":null}");
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BowdlerizeListObject() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingBowdlerizer(bowdlerizer)
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

            var expected = QuoteExpectedString("Here is ", @"[{""PersonId"":0,""MailingAddress"":{""Address1"":""***"",""Address2"":""c/o Elmer"",""City"":""***"",""State"":null,""Zip"":null},""FirstName"":""***"",""EmailAddress"":""foo@bar.baz"",""LastName"":""***"",""SocialSecurityNumber"":""***"",""Suffix"":null,""PhoneNumber"":""(801) 867-5309"",""BirthDate"":""***"",""Children"":[],""Addresses"":null},{""PersonId"":0,""MailingAddress"":null,""FirstName"":""***"",""EmailAddress"":null,""LastName"":"""",""SocialSecurityNumber"":"""",""Suffix"":null,""PhoneNumber"":null,""BirthDate"":""***"",""Children"":[],""Addresses"":null}]");
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BowdlerizeObjectWithNestedList() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .UsingBowdlerizer(bowdlerizer)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var model = new Person {
                Addresses = new List<Address> {
                    new Address() { Address1 = "123 Main", Address2 = "c/o Elmer", City = "Salt Lake City" },
                    new Address() { Address1 = "234 Main", Address2 = "c/o Candy", City = "Vernal" },
                }
            };

            log.Information("Here is {@Ignored}", model);

            var expected = QuoteExpectedString("Here is ", @"{""PersonId"":0,""MailingAddress"":null,""FirstName"":"""",""EmailAddress"":null,""LastName"":"""",""SocialSecurityNumber"":"""",""Suffix"":null,""PhoneNumber"":null,""BirthDate"":""***"",""Children"":[],""Addresses"":[{""Address1"":""***"",""Address2"":""c/o Elmer"",""City"":""***"",""State"":null,""Zip"":null},{""Address1"":""***"",""Address2"":""c/o Candy"",""City"":""***"",""State"":null,""Zip"":null}]}");
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BowdlerizeJsonListString() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .UsingBowdlerizer(bowdlerizer)
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

            var expected = QuoteExpectedString("Here is ", @"[{""PersonId"":0,""MailingAddress"":{""Address1"":""***"",""Address2"":""c/o Elmer"",""City"":""***"",""State"":null,""Zip"":null},""FirstName"":""***"",""EmailAddress"":""foo@bar.baz"",""LastName"":""***"",""SocialSecurityNumber"":""***"",""Suffix"":null,""PhoneNumber"":""(801) 867-5309"",""BirthDate"":""***"",""Children"":[],""Addresses"":null},{""PersonId"":0,""MailingAddress"":null,""FirstName"":""***"",""EmailAddress"":null,""LastName"":"""",""SocialSecurityNumber"":"""",""Suffix"":null,""PhoneNumber"":null,""BirthDate"":""***"",""Children"":[],""Addresses"":null}]");
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BowdlerizeJsonStringWithNestedList() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .UsingBowdlerizer(bowdlerizer)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var model = new Person {
                Addresses = new List<Address> {
                    new Address() { Address1 = "123 Main", Address2 = "c/o Elmer", City = "Salt Lake City" },
                    new Address() { Address1 = "234 Main", Address2 = "c/o Candy", City = "Las Vegas" },
                }
            };

            log.Information("Here is {@Ignored}", JsonConvert.SerializeObject(model));

            var expected = QuoteExpectedString("Here is ", @"{""PersonId"":0,""MailingAddress"":null,""FirstName"":"""",""EmailAddress"":null,""LastName"":"""",""SocialSecurityNumber"":"""",""Suffix"":null,""PhoneNumber"":null,""BirthDate"":""***"",""Children"":[],""Addresses"":[{""Address1"":""***"",""Address2"":""c/o Elmer"",""City"":""***"",""State"":null,""Zip"":null},{""Address1"":""***"",""Address2"":""c/o Candy"",""City"":""***"",""State"":null,""Zip"":null}]}");
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BowdlerizeXmlString() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .UsingBowdlerizer(bowdlerizer)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            const string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><InstantIDResponseEx><response><Header><Status>0</Status></Header><Result><InputEcho><Name><First>Chester</First><Last>Tester</Last></Name><Address><StreetAddress1>cvi=0;</StreetAddress1><City>salt lake</City><State>UT</State><Zip5>84106</Zip5></Address><DOB><Year>1990</Year><Month>11</Month><Day>11</Day></DOB><SSN>800120850</SSN><HomePhone>8015556666</HomePhone><UseDOBFilter>1</UseDOBFilter><DOBRadius>1</DOBRadius><Passport><Number></Number><ExpirationDate/><Country></Country><MachineReadableLine1></MachineReadableLine1><MachineReadableLine2></MachineReadableLine2></Passport><Channel></Channel><OwnOrRent></OwnOrRent></InputEcho></Result></response></InstantIDResponseEx>";
            log.Information("Here is {Xml}", xml);

            var expected = QuoteExpectedString("Here is ", "<InstantIDResponseEx><response><Header><Status>0</Status></Header><Result><InputEcho><Name><First>***</First><Last>***</Last></Name><Address><StreetAddress1>cvi=0;</StreetAddress1><City>salt lake</City><State>UT</State><Zip5>84106</Zip5></Address><DOB><Year>***</Year><Month>***</Month><Day>***</Day></DOB><SSN>***</SSN><HomePhone>8015556666</HomePhone><UseDOBFilter>1</UseDOBFilter><DOBRadius>1</DOBRadius><Passport><Number></Number><ExpirationDate /><Country></Country><MachineReadableLine1></MachineReadableLine1><MachineReadableLine2></MachineReadableLine2></Passport><Channel></Channel><OwnOrRent></OwnOrRent></InputEcho></Result></response></InstantIDResponseEx>");
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BowdlerizeScalarType() {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .UsingBowdlerizer(bowdlerizer)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information("Here is {SomeDate}", 123.45M);

            const string expected = "Here is 123.45";
            var actual = evt.RenderMessage();
            Assert.Equal(expected, actual);
        }
    }
}
