// <copyright file="LdapConnectionServiceTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Visus.LdapAuthentication.Configuration;


namespace Visus.LdapAuthentication.Tests {

    [TestClass]
    public sealed class LdapConnectionServiceTest {

        [TestMethod]
        public void TestFailover() {
            if (this._testSecrets.CanRun) {
                var configuration = TestExtensions.CreateConfiguration();
                var collection = new ServiceCollection().AddMockLoggers();
                collection.AddLdapAuthentication(o => {
                    var section = configuration.GetSection("LdapOptions");
                    section.Bind(o);

                    Assert.IsTrue(o.Servers.Count() >= 2, "This test requires two servers.");
                    Assert.AreNotEqual(o.Servers.First(), o.Servers.Skip(1).First(), "The two servers must be distinct for this test.");
                    o.ServerSelectionPolicy = ServerSelectionPolicy.Failover;
                });
                var services = collection.BuildServiceProvider();

                var service = services.GetService<ILdapConnectionService>();
                Assert.IsNotNull(service);

                var connection1 = service.Connect();
                var connection2 = service.Connect();
                Assert.AreEqual(connection1.Host, connection2.Host);
            }
        }

        [TestMethod]
        public void TestRoundRobin() {
            if (this._testSecrets.CanRun) {
                var configuration = TestExtensions.CreateConfiguration();
                var collection = new ServiceCollection().AddMockLoggers();
                collection.AddLdapAuthentication(o => {
                    var section = configuration.GetSection("LdapOptions");
                    section.Bind(o);

                    o.Servers = ["127.0.0.1", "127.0.0.2"];
                    o.SearchBases = new Dictionary<string, SearchScope> {
                    { "DC=domain", SearchScope.Base }
                };

                    Assert.IsTrue(o.Servers.Count() >= 2, "This test requires two servers.");
                    Assert.AreNotEqual(o.Servers.First(), o.Servers.Skip(1).First(), "The two servers must be distinct for this test.");
                    o.ServerSelectionPolicy = ServerSelectionPolicy.RoundRobin;
                });
                var services = collection.BuildServiceProvider();

                var service = services.GetService<ILdapConnectionService>();
                Assert.IsNotNull(service);

                var connection1 = service.Connect();
                var connection2 = service.Connect();
                Assert.AreNotEqual(connection1.Host, connection2.Host);
            }
        }

        private readonly TestSecrets _testSecrets = TestExtensions.CreateSecrets();
    }
}
