// <copyright file="LdapAuthenticationServiceTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021  -2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Visus.Ldap;


namespace Visus.LdapAuthentication.Tests {

    /// <summary>
    /// Tests for the <see cref="LdapAuthenticationService"/>.
    /// </summary>
    [TestClass]
    public sealed class LdapAuthenticationServiceTest {

        public LdapAuthenticationServiceTest() {
            var configuration = TestExtensions.CreateConfiguration();
            var collection = new ServiceCollection().AddMockLoggers();
            collection.AddLdapAuthentication(o => {
                var section = configuration.GetSection("LdapOptions");
                section.Bind(o);
            });
            this._services = collection.BuildServiceProvider();
        }

        [TestMethod]
        public void TestLoginUser() {
            if (this._testSecrets?.LdapOptions != null) {
                var service = this._services.GetService<ILdapAuthenticationService<LdapUser>>();
                Assert.IsNotNull(service);

                var user = service.LoginUser(
                    this._testSecrets.LdapOptions.User,
                    this._testSecrets.LdapOptions.Password);
                Assert.IsNotNull(user);
                Assert.IsNotNull(user.Groups);
                Assert.IsTrue(user.Groups.Any());
                Assert.IsTrue(user.Groups.Count() >= 1);
                Assert.IsFalse(user.Groups.Any(g => g == null));
            }
        }

        private readonly ServiceProvider _services;
        private readonly TestSecrets _testSecrets = TestExtensions.CreateSecrets();
    }
}
