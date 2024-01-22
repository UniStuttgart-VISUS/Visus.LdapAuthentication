// <copyright file="LdapOptionsTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;


namespace Visus.LdapAuthentication.Tests {

    /// <summary>
    /// Tests the <see cref="LdapSearchService"/>.
    /// </summary>
    [TestClass]
    public class LdapSearchServiceTest {

        public LdapSearchServiceTest() {
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
        public void TestGetUserByIdentity() {
            if (this._testSecrets?.LdapOptions != null) {
                var service = new LdapSearchService<LdapUser>(
                    this._testSecrets.LdapOptions,
                    Mock.Of<ILogger<LdapSearchService<LdapUser>>>());

                var user = service.GetUserByIdentity(this._testSecrets.ExistingUserIdentity);
                Assert.IsNotNull(user, "Existing user was found.");
            }
        }

        [TestMethod]
        public void TestGetUsers() {
            if (this._testSecrets?.LdapOptions != null) {
                var service = new LdapSearchService<LdapUser>(
                    this._testSecrets.LdapOptions,
                    Mock.Of<ILogger<LdapSearchService<LdapUser>>>());

                {
                    var users = service.GetUsers();
                    Assert.IsTrue(users.Any(), "Directory search returns any user.");
                }

                {
                    var att = LdapAttributeAttribute.GetLdapAttribute<LdapUser>(
                        nameof(LdapUser.AccountName),
                        this._testSecrets.LdapOptions.Schema);
                    var users = service.GetUsers($"({att.Name}={this._testSecrets.ExistingUserAccount})");
                    Assert.IsNotNull(users.Any(), "Filtered user was found.");
                }
            }
        }

        [TestMethod]
        public void TestGetDistinguishedNames() {
            if (this._testSecrets?.LdapOptions != null) {
                var service = new LdapSearchService<LdapUser>(
                    this._testSecrets.LdapOptions,
                    Mock.Of<ILogger<LdapSearchService<LdapUser>>>());

                var att = LdapAttributeAttribute.GetLdapAttribute<LdapUser>(
                    nameof(LdapUser.AccountName),
                    this._testSecrets.LdapOptions.Schema);
                var users = service.GetDistinguishedNames($"({att.Name}={this._testSecrets.ExistingUserAccount})");
                Assert.IsTrue(users.Any(), "Search returned at least one DN.");
            }
        }

        private readonly TestSecrets _testSecrets;
    }
}
