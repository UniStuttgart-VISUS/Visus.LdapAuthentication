// <copyright file="LdapAuthenticationServiceTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Novell.Directory.Ldap;


namespace Visus.LdapAuthentication.Tests {

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
            if (this._testSecrets != null) {
                var service = new LdapAuthenticationService<LdapUser>(
                    this._testSecrets.LdapOptions,
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
