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
using Visus.LdapAuthentication.Tests;
using Visus.LdapAuthentication;


namespace Visus.DirectoryAuthentication.Tests {

    [TestClass]
    public sealed class ServiceCollectionTest {

        [TestMethod]
        public void TestInternalDefaults() {
            var configuration = TestExtensions.CreateConfiguration();

            var collection = new ServiceCollection().AddMockLoggers();
            collection.AddLdapAuthentication(o => {
                var section = configuration.GetSection("LdapOptions");
                section.Bind(o);

                o.Servers = ["127.0.0.1"];
                o.SearchBases = new Dictionary<string, SearchScope> {
                    { "DC=domain", SearchScope.Base }
                };
                o.Schema = Schema.ActiveDirectory;
            });

            var provider = collection.BuildServiceProvider();

            {
                var options = provider.GetService<IOptions<LdapOptions>>();
                Assert.IsNotNull(options?.Value, "Options resolved");
            }

            {
                var service = provider.GetService<ILdapAttributeMap<LdapUser>>();
                Assert.IsNotNull(service, "User attribute map resolved");
            }

            {
                var service = provider.GetService<ILdapAttributeMap<LdapGroup>>();
                Assert.IsNotNull(service, "Group attribute map resolved");
            }

            {
                var service = provider.GetService<ILdapMapper<SearchResultEntry, LdapUser, LdapGroup>>();
                Assert.IsNotNull(service, "Mapper resolved");
            }

            {
                var service = provider.GetService<IUserClaimsMap>();
                Assert.IsNotNull(service, "User claims map resolved");
            }

            {
                var service = provider.GetService<IGroupClaimsMap>();
                Assert.IsNotNull(service, "Group claims map resolved");
            }

            {
                var service = provider.GetService<IClaimsBuilder<LdapUser, LdapGroup>>();
                Assert.IsNotNull(service, "Claims builder resolved");
            }

            {
                var service = provider.GetService<IClaimsMapper<SearchResultEntry>>();
                Assert.IsNotNull(service, "Claims mapper resolved");
            }
        }

        [TestMethod]
        public void TestPublicDefaults() {
            var configuration = TestExtensions.CreateConfiguration();

            var collection = new ServiceCollection().AddMockLoggers();
            collection.AddLdapAuthentication(o => {
                var section = configuration.GetSection("LdapOptions");
                section.Bind(o);

                o.Servers = ["127.0.0.1"];
                o.SearchBases = new Dictionary<string, SearchScope> {
                    { "DC=domain", SearchScope.Base }
                };
                o.Schema = Schema.ActiveDirectory;
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

            if (this._testSecrets.CanRun) {
                var service = provider.GetService<ILdapSearchService<LdapUser, LdapGroup>>();
                Assert.IsNotNull(service, "Search service resolved");
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

        private readonly TestSecrets _testSecrets = TestExtensions.CreateSecrets();
    }
}

