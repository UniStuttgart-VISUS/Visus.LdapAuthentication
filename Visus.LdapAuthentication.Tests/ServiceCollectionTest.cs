// <copyright file="ServiceCollectionTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Visus.Ldap.Claims;
using Visus.Ldap.Mapping;
using Visus.Ldap;
using Visus.LdapAuthentication.Configuration;
using Novell.Directory.Ldap;
using System.Collections.Generic;
using System.Linq;


namespace Visus.LdapAuthentication.Tests {

    [TestClass]
    public sealed class ServiceCollectionTest {

        [TestMethod]
        public void TestInternalDefaults() {
            var configuration = TestExtensions.CreateConfiguration();

            var collection = new ServiceCollection().AddMockLoggers();
            collection.AddLdapAuthentication(o => {
                var section = configuration.GetSection("LdapOptions");
                section.Bind(o);
            });

            var provider = collection.BuildServiceProvider();

            if (this._testSecrets.CanRun) {
                var options = provider.GetService<IOptions<LdapOptions>>();
                Assert.IsNotNull(options?.Value, "Options resolved");
            }

            if (this._testSecrets.CanRun) {
                var service = provider.GetService<ILdapAttributeMap<LdapUser>>();
                Assert.IsNotNull(service, "User attribute map resolved");
            }

            if (this._testSecrets.CanRun) {
                var service = provider.GetService<ILdapAttributeMap<LdapGroup>>();
                Assert.IsNotNull(service, "Group attribute map resolved");
            }

            if (this._testSecrets.CanRun) {
                var service = provider.GetService<ILdapMapper<LdapEntry, LdapUser, LdapGroup>>();
                Assert.IsNotNull(service, "Mapper resolved");
            }

            if (this._testSecrets.CanRun) {
                var service = provider.GetService<IUserClaimsMap>();
                Assert.IsNotNull(service, "User claims map resolved");
            }

            if (this._testSecrets.CanRun) {
                var service = provider.GetService<IGroupClaimsMap>();
                Assert.IsNotNull(service, "Group claims map resolved");
            }

            if (this._testSecrets.CanRun) {
                var service = provider.GetService<IClaimsBuilder<LdapUser, LdapGroup>>();
                Assert.IsNotNull(service, "Claims builder resolved");
            }

            if (this._testSecrets.CanRun) {
                var service = provider.GetService<IClaimsMapper<LdapEntry>>();
                Assert.IsNotNull(service, "Claims mapper resolved");
            }
        }

        [TestMethod]
        public void TestPublicDefaults() {
            if (this._testSecrets.CanRun) {
                var configuration = TestExtensions.CreateConfiguration();

                var collection = new ServiceCollection().AddMockLoggers();
                collection.AddLdapAuthentication(o => {
                    var section = configuration.GetSection("LdapOptions");
                    section.Bind(o);
                });

                var provider = collection.BuildServiceProvider();

                {
                    var service = provider.GetService<ILdapConnectionService>();
                    Assert.IsNotNull(service, "Connection service resolved");
                }

                {
                    var service = provider.GetService<ILdapAuthenticationService<LdapUser>>();
                    Assert.IsNotNull(service, "Authentication service resolved");
                }

                {
                    var service = provider.GetService<ILdapSearchService<LdapUser, LdapGroup>>();
                    Assert.IsNotNull(service, "Search service resolved");
                }

                {
                    var service = provider.GetService<ILdapCache>();
                    Assert.IsNotNull(service, "Cache resolved");
                }
            }
        }

        [TestMethod]
        public void TestValidation() {
            Assert.ThrowsException<OptionsValidationException>(() => {
                var configuration = TestExtensions.CreateConfiguration();

                var collection = new ServiceCollection().AddMockLoggers();
                collection.AddLdapAuthentication(o => { });

                var provider = collection.BuildServiceProvider();

                var service = provider.GetService<ILdapConnectionService>();
            });
        }


        [TestMethod]
        public void TestFluentLdapAttributeMap() {
            var configuration = TestExtensions.CreateConfiguration();

            var collection = new ServiceCollection().AddMockLoggers();
            collection.AddLdapAuthentication(o => {
                var section = configuration.GetSection("LdapOptions");
                section.Bind(o);
            }, (u, _) => {
                u.MapProperty(nameof(LdapUser.Identity)).ToAttribute("objectSid");

            }, (g, _) => {
                g.MapProperty(nameof(LdapGroup.Identity)).ToAttribute("objectSid");
            });

            var provider = collection.BuildServiceProvider();

            if (this._testSecrets.CanRun) {
                var service = provider.GetService<ILdapAttributeMap<LdapUser>>();
                Assert.IsNotNull(service, "User attribute map resolved");
                Assert.AreEqual(1, service.Attributes.Count());
                Assert.IsTrue(service.Attributes.Any(a => a.Name == "objectSid"));
            }

            if (this._testSecrets.CanRun) {
                var service = provider.GetService<ILdapAttributeMap<LdapGroup>>();
                Assert.IsNotNull(service, "Group attribute map resolved");
                Assert.AreEqual(1, service.Attributes.Count());
                Assert.IsTrue(service.Attributes.Any(a => a.Name == "objectSid"));
            }
        }

        [TestMethod]
        public void TestFluentClaimsMap() {
            var configuration = TestExtensions.CreateConfiguration();

            var collection = new ServiceCollection().AddMockLoggers();
            collection.AddLdapAuthentication(o => {
                var section = configuration.GetSection("LdapOptions");
                section.Bind(o);
            }, null, null, (u, _) => {
                u.MapProperty(nameof(LdapUser.Identity)).ToClaim("id");

            }, (g, _) => {
                g.MapProperty(nameof(LdapGroup.Identity)).ToClaim("id");
            });

            var provider = collection.BuildServiceProvider();

            var attribute = new LdapAttributeAttribute(Schema.ActiveDirectory, "objectSid") {
                Converter = typeof(SidConverter)
            };

            if (this._testSecrets.CanRun) {
                var service = provider.GetService<IUserClaimsMap>();
                Assert.IsNotNull(service, "User claims map resolved");
                Assert.AreEqual("id", service[attribute].Single().Name);
            }

            if (this._testSecrets.CanRun) {
                var service = provider.GetService<IGroupClaimsMap>();
                Assert.IsNotNull(service, "Group claims map resolved");
                Assert.AreEqual("id", service[attribute].Single().Name);
            }
        }

        private readonly TestSecrets _testSecrets = TestExtensions.CreateSecrets();
    }
}

