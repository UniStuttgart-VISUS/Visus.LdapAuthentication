// <copyright file="LdapOptionsTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2022 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

#if false
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;


namespace Visus.DirectoryAuthentication.Tests {

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

        private readonly TestSecrets _testSecrets;
    }
}
#endif
