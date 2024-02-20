// <copyright file="IssuesTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
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

        #region Issue #7
        [TestMethod]
        public void Test7LdapOptions() {
            {
                var o = new LdapOptions();
                Assert.IsNotNull(o.SearchBases, "SearchBases initialised");
                Assert.AreEqual(0, o.SearchBases.Count, "SearchBases empty");
            }

            {
                var dn = "DC=visus, DC=uni-stuttgart,DC=de";
                var o = new LdapOptions();
                Assert.IsNotNull(o.SearchBases, "SearchBases initialised");
                Assert.AreEqual(0, o.SearchBases.Count, "SearchBases empty");
#pragma warning disable CS0618
                o.SearchBase = dn;
                Assert.AreEqual(1, o.SearchBases.Count, "Single SearchBase created.");
                Assert.AreSame(((ILdapOptions) o).SearchBase, o.SearchBases.Keys.First(), "First element set");
                o.IsSubtree = false;
                Assert.AreEqual(1, o.SearchBases.Count, "No additional SearchBase created.");
                Assert.AreEqual(SearchScope.Base, o.SearchBases.Values.First(), "Non-recursive set");
#pragma warning restore CS0618
            }

            {
                var dn = "DC=visus, DC=uni-stuttgart,DC=de";
                var o = new LdapOptions() {
                    SearchBases = new Dictionary<string, SearchScope>() {
                        { "DC=vis,DC=uni-stuttgart,DC=de", SearchScope.Subtree }
                    }
                };
                Assert.IsNotNull(o.SearchBases, "SearchBases initialised");
                Assert.AreEqual(1, o.SearchBases.Count, "SearchBases holds one element");
#pragma warning disable CS0618
                o.SearchBase = dn;
                Assert.AreEqual(1, o.SearchBases.Count, "Setting legacy SearchBase does not affect SearchBases");
#pragma warning restore CS0618
            }

        }
        #endregion

        #region Issue #9
        #if false
        [TestMethod]
        public void Test9() {
            var options = new LdapOptions() {
                Server = "ldap.forumsys.com",
                SearchBases = new Dictionary<string, SearchScope>() {
                    { "dc=example,dc=com", SearchScope.Subtree }
                },
                Schema = Schema.Rfc2307,
                IsRecursiveGroupMembership = true,
                Port = 389,
                IsSsl =false,
                IsNoCertificateCheck = true
            };

            var service = new LdapAuthenticationService<LdapUser>(options,
                Mock.Of<ILogger<LdapAuthenticationService<LdapUser>>>());

            var user = service.Login("uid=tesla,dc=example,dc=com", "");
            Assert.IsNotNull(user);
        }
        #endif
        #endregion

        #region Private fields
        private readonly TestSecrets _testSecrets;
        #endregion
    }
}
