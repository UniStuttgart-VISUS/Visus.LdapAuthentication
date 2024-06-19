// <copyright file="LdapIdentityUserTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Visus.DirectoryAuthentication;


namespace Visus.DirectoryIdentity.Tests {

    /// <summary>
    /// Tests of the <see cref="LdapIdentityUser"/>.
    /// </summary>
    [TestClass]
    public class LdapIdentityUserTest {

        #region Public constructors
        public LdapIdentityUserTest() {
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

        [TestMethod]
        public void TestRetrievalOfExtendedAttributes() {
            if (this._testSecrets != null) {
                var service = new LdapSearchService<LdapIdentityUser>(
                    new LdapUserMapper<LdapIdentityUser>(Options.Create(this._testSecrets.LdapOptions)),
                    Options.Create(this._testSecrets.LdapOptions),
                    Mock.Of<ILogger<LdapSearchService<LdapIdentityUser>>>());

                var user = service.GetUserByIdentity(this._testSecrets.ExistingUserIdentity);
                Assert.IsNotNull(user, "Existing user was found");
                Assert.IsFalse(user.IsLockoutEnabled, "User is not locked");
                Assert.IsNotNull(user.LastAccessFailed, "Password failed some time");
            }
        }

        #region Private fields
        private readonly TestSecrets _testSecrets;
        #endregion
    }
}
