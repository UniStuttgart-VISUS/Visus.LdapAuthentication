// <copyright file="ClaimMapBuilderTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Security.Claims;
using Visus.DirectoryAuthentication.Claims;
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryAuthentication.Mapping;
using Visus.Ldap;
using Visus.Ldap.Claims;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Tests {

    [TestClass]
    public sealed class ClaimMapBuilderTest {
        [TestMethod]
        public void TestGroupClaims() {
            IClaimsMap map = new ClaimsMap<LdapGroup>((builder, _) => {
                builder.MapProperty(nameof(LdapGroup.AccountName))
                    .ToClaims(ClaimTypes.WindowsAccountName, ClaimTypes.Role);

                builder.MapAttribute("objectSid")
                    .WithConverter<SidConverter>()
                    .ToClaim(ClaimTypes.Sid);
            }, new LdapOptions() {
                Schema = Schema.ActiveDirectory
            });

            Assert.IsNotNull(map);
            Assert.IsNotNull(map.AttributeNames);
            Assert.IsTrue(map.AttributeNames.Contains("sAMAccountName"));
            Assert.IsTrue(map.AttributeNames.Contains("objectSid"));

            {
                var att = new LdapAttributeAttribute(Schema.ActiveDirectory, "sAMAccountName");
                var claims = map[att];
                Assert.IsNotNull(claims);
                Assert.IsTrue(claims.Any(c => c.Name ==ClaimTypes.WindowsAccountName));
                Assert.IsTrue(claims.Any(c => c.Name ==ClaimTypes.Role));
            }

            {
                var att = new LdapAttributeAttribute(Schema.ActiveDirectory, "objectSid") {
                    Converter = typeof(SidConverter)
                };
                var claims = map[att];
                Assert.IsNotNull(claims);
                Assert.IsNotNull(claims.SingleOrDefault(c => c.Name == ClaimTypes.Sid));
            }
        }

        [TestMethod]
        public void TestUserClaims() {
            IClaimsMap map = new ClaimsMap<LdapUser>((builder, _) => {
                builder.MapProperty(nameof(LdapUser.AccountName))
                    .ToClaims(ClaimTypes.WindowsAccountName, ClaimTypes.NameIdentifier);

                builder.MapPropertyToAnnotatedClaims(nameof(LdapUser.ChristianName));

                builder.MapAttribute(new LdapAttributeAttribute(Schema.ActiveDirectory, "mail"))
                    .ToClaim(ClaimTypes.Email);

                builder.MapAttribute(new LdapAttributeAttribute(Schema.ActiveDirectory, "objectSid") {
                    Converter = typeof(SidConverter)
                }).ToClaims([ClaimTypes.Sid, ClaimTypes.PrimarySid]);

                builder.MapProperty(nameof(LdapUser.Surname))
                    .ToClaim(ClaimTypes.Surname);
            }, new LdapOptions() {
                Schema = Schema.ActiveDirectory
            });

            Assert.IsNotNull(map);
            Assert.IsNotNull(map.AttributeNames);
            Assert.IsTrue(map.AttributeNames.Contains("sAMAccountName"));
            Assert.IsTrue(map.AttributeNames.Contains("givenName"));
            Assert.IsTrue(map.AttributeNames.Contains("objectSid"));
            Assert.IsTrue(map.AttributeNames.Contains("mail"));
            Assert.IsTrue(map.AttributeNames.Contains("sn"));

            {
                var att = new LdapAttributeAttribute(Schema.ActiveDirectory, "sAMAccountName");
                var claims = map[att];
                Assert.IsNotNull(claims);
                Assert.IsTrue(claims.Any(c => c.Name == ClaimTypes.WindowsAccountName));
                Assert.IsTrue(claims.Any(c => c.Name == ClaimTypes.NameIdentifier));
            }

            {
                var att = new LdapAttributeAttribute(Schema.ActiveDirectory, "givenName");
                var claims = map[att];
                Assert.IsNotNull(claims);
                Assert.IsNotNull(claims.SingleOrDefault(c => c.Name == ClaimTypes.GivenName));
            }

            {
                var att = new LdapAttributeAttribute(Schema.ActiveDirectory, "mail");
                var claims = map[att];
                Assert.IsNotNull(claims);
                Assert.IsNotNull(claims.SingleOrDefault(c => c.Name == ClaimTypes.Email));
            }

            {
                var att = new LdapAttributeAttribute(Schema.ActiveDirectory, "objectSid") {
                    Converter = typeof(SidConverter)
                };
                var claims = map[att];
                Assert.IsNotNull(claims);
                Assert.IsTrue(claims.Any(c => c.Name == ClaimTypes.Sid));
                Assert.IsTrue(claims.Any(c => c.Name == ClaimTypes.PrimarySid));
            }

            {
                var att = new LdapAttributeAttribute(Schema.ActiveDirectory, "sn");
                var claims = map[att];
                Assert.IsNotNull(claims);
                Assert.IsNotNull(claims.SingleOrDefault(c => c.Name == ClaimTypes.Surname));
            }
        }

        [TestMethod]
        public void TestPreventInvalidProperty() {
            _ = new ClaimsMap<LdapUser>((builder, _) => {
                Assert.ThrowsException<ArgumentNullException>(() => builder.MapProperty(null!));
                Assert.ThrowsException<ArgumentNullException>(() => builder.MapProperty(null!));
                Assert.ThrowsException<ArgumentException>(() => builder.MapProperty("hurz"));
                Assert.ThrowsException<ArgumentException>(() => builder.MapProperty("hurz"));
            }, new LdapOptions() {
                Schema = Schema.ActiveDirectory
            });
        }

        [TestMethod]
        public void TestPreventInvalidAttribute() {
            _ = new ClaimsMap<LdapUser>((builder, _) => {
                Assert.ThrowsException<ArgumentNullException>(() => builder.MapAttribute((LdapAttributeAttribute) null!));
                Assert.ThrowsException<ArgumentNullException>(() => builder.MapAttribute((string) null!));
            }, new LdapOptions() {
                Schema = Schema.ActiveDirectory
            });
        }

        [TestMethod]
        public void TestPreventSchemaMismatch() {
            _ = new ClaimsMap<LdapUser>((builder, _) => {
                 Assert.ThrowsException<ArgumentException>(() => builder.MapAttribute(new LdapAttributeAttribute("hurz", "sAMAccountName")));
            }, new LdapOptions() {
                Schema = Schema.ActiveDirectory
            });
        }
    }
}
