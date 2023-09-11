// <copyright file="IssuesTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;


namespace Visus.LdapAuthentication.Tests {

    /// <summary>
    /// Tests for GitHub issues.
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


        #region Issue #1
        public sealed class CustomLdapUser : LdapUserBase {

            [LdapAttribute(Schema.ActiveDirectory, "objectCategory")]
            [Claim(ClaimTypes.Name)]
            public override string DisplayName => base.DisplayName;
        }

        [TestMethod]
        public void Test1() {
            if (this._testSecrets?.LdapOptions != null) {
                var service = new LdapSearchService<CustomLdapUser>(
                    this._testSecrets.LdapOptions,
                    Mock.Of<ILogger<LdapSearchService<CustomLdapUser>>>());

                var user = service.GetUserByIdentity(this._testSecrets.ExistingUserIdentity);
                Assert.IsNotNull(user, "Existing user was found.");
                Assert.AreEqual("CN=Person,CN=Schema,CN=Configuration,DC=visus,DC=uni-stuttgart,DC=de", user.DisplayName);
            }
        }
        #endregion

        #region Private fields
        private readonly TestSecrets _testSecrets;
        #endregion
    }
}
