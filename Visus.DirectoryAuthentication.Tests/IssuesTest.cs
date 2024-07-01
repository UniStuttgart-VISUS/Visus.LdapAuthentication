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
using System.Linq;

namespace Visus.DirectoryAuthentication.Tests {

    /// <summary>
    /// Tests for issues from GitHub.
    /// </summary>
    [TestClass]
    public sealed class IssuesTest {

        #region Public constructors
        public IssuesTest() {
            new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build()
                .Bind(this._testSecrets = new());
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

            var options = Options.Create(options);
            var service = new LdapAuthenticationService<LdapUser>(
                new LdapUserMapper<LdapUser>(options),
                options,
                Mock.Of<ILogger<LdapAuthenticationService<LdapUser>>>());

            var user = service.Login("uid=tesla,dc=example,dc=com", "");
            Assert.IsNotNull(user);
        }
        #endif
        #endregion

        #region Issue #10
        [TestMethod]
        public void Test10() {
            if (this._testSecrets?.LdapOptions != null) {
                var options = Options.Create(this._testSecrets.LdapOptions);
                var claims = new ClaimsBuilder<LdapUser, LdapGroup>(
                    Mock.Of<ILogger<ClaimsBuilder<LdapUser, LdapGroup>>>());
                var mapper = new LdapMapper<LdapUser, LdapGroup>(options, claims);
                var service = new LdapAuthenticationService<LdapUser, LdapGroup>(
                    mapper,
                    options,
                    Mock.Of<ILogger<LdapAuthenticationService<LdapUser, LdapGroup>>>());

                Assert.ThrowsException<LdapException>(() => {
                    var user = service.Login(this._testSecrets.LdapOptions.User,
                        this._testSecrets.LdapOptions.Password + " is wrong");
                });
            }
        }
        #endregion

        #region Issue #12
        [TestMethod]
        public void Test12() {
            if (this._testSecrets?.LdapOptions != null) {
                var configuration = new ConfigurationBuilder()
                    .AddUserSecrets<TestSecrets>()
                    .Build();
                var secrets = new TestSecrets();
                configuration.Bind(secrets);
                secrets.LdapOptions.PageSize = 0;

                var options = Options.Create(secrets.LdapOptions);
                var claims = new ClaimsBuilder<LdapUser, LdapGroup>(
                    Mock.Of<ILogger<ClaimsBuilder<LdapUser, LdapGroup>>>());
                var mapper = new LdapMapper<LdapUser, LdapGroup>(options, claims);
                var service = new LdapSearchService<LdapUser, LdapGroup>(
                    mapper,
                    options,
                    Mock.Of<ILogger<LdapSearchService<LdapUser, LdapGroup>>>());

                var users = service.GetUsers($"(sAMAccountName={this._testSecrets.ExistingUserAccount})");
                Assert.IsTrue(users.Any(), "At least something returned");
            }
        }
        #endregion

        #region Private fields
        private readonly TestSecrets _testSecrets;
        #endregion
    }
}
