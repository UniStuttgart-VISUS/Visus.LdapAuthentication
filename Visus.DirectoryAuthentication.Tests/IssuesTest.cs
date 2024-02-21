// <copyright file="IssuesTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;


namespace Visus.DirectoryAuthentication.Tests {

    /// <summary>
    /// Tests for issues from GitHub.
    /// </summary>
    [TestClass]
    public sealed class IssuesTest {

        #region Public constructors
        public IssuesTest() {
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
        #endregion

        #region Issue #9
        #if false
        [TestMethod]
        public void Test9() {
            var options = new LdapOptions() {
                AuthenticationType = AuthType.Basic,
                Server = "ldap.forumsys.com",
                SearchBase = new Dictionary<string, SearchScope>() {
                    { "dc=example,dc=com", SearchScope.Subtree }
                },
                Schema = Schema.Rfc2307,
                IsRecursiveGroupMembership = true,
                Port = 389,
                IsSsl = false,
                IsNoCertificateCheck = true
            };

            var service = new LdapAuthenticationService<LdapUser>(
                Options.Create(this._testSecrets.LdapOptions),
                Mock.Of<ILogger<LdapAuthenticationService<LdapUser>>>());

            var user = service.Login("uid=tesla,dc=example,dc=com", "");
            Assert.IsNotNull(user);
        }
        #endif
        #endregion

        #region Issue #10
        [TestMethod]
        public void Test10() {
            if (this._testSecrets != null) {
                var service = new LdapAuthenticationService<LdapUser>(
                    Options.Create(this._testSecrets.LdapOptions),
                    Mock.Of<ILogger<LdapAuthenticationService<LdapUser>>>());

                Assert.ThrowsException<LdapException>(() => {
                    var user = service.Login(this._testSecrets.LdapOptions.User,
                        this._testSecrets.LdapOptions.Password + " is wrong");
                });
            }
        }
        #endregion

        #region Private fields
        private readonly TestSecrets _testSecrets;
        #endregion
    }
}
