// <copyright file="LdapAuthenticationServiceTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021  -2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Novell.Directory.Ldap;
using Visus.LdapAuthentication.Services;


namespace Visus.LdapAuthentication.Tests
{

    /// <summary>
    /// Tests for the <see cref="LdapAuthenticationService"/>.
    /// </summary>
    [TestClass]
    public sealed class LdapAuthenticationServiceTest {

        public LdapAuthenticationServiceTest() {
            try {
                var configuration = new ConfigurationBuilder()
                    .AddUserSecrets<TestSecrets>()
                    .Build();
                this._testSecrets = new TestSecrets();
                configuration.Bind(this._testSecrets);

            } catch {
                this._testSecrets = null;
            }
        }

        [TestMethod]
        public void TestLogin() {
            if (this._testSecrets?.LdapOptions != null) {
                var options = Options.Create(this._testSecrets.LdapOptions);
                var service = new LdapAuthenticationService<LdapUser>(
                    new LdapUserMapper<LdapUser>(options),
                    options,
                    Mock.Of<ILogger<LdapAuthenticationService<LdapUser>>>());

                {
                    var user = service.Login(this._testSecrets.LdapOptions.User,
                        this._testSecrets.LdapOptions.Password);
                    Assert.IsNotNull(user, "Login succeeded.");
                }

                Assert.ThrowsException<LdapException>(() => {
                    var user = service.Login(this._testSecrets.LdapOptions.User,
                        this._testSecrets.LdapOptions.Password + " is wrong");
                });
            }
        }

        private readonly TestSecrets _testSecrets;
    }
}
