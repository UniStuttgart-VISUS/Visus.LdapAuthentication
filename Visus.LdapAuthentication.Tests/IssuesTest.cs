// <copyright file="IssuesTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
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

        #region Issue #7
        [TestMethod]
        public void Test7SearchBase() {
            {
                var b = new SearchBase();
                Assert.IsNotNull(b.DistinguishedName, "DN is not null");
                Assert.AreEqual(SearchScope.Sub, b.Scope, "Default scope is sub");
            }

            {
                var b = new SearchBase(SearchScope.Base);
                Assert.IsNotNull(b.DistinguishedName, "DN is not null");
                Assert.AreEqual(SearchScope.Base, b.Scope, "Scope set");
            }

            {
                var expected = "DC=visus, DC=uni-stuttgart, DC=de";
                var b = new SearchBase(expected);
                Assert.AreEqual(expected, b.DistinguishedName, "DN set");
                Assert.AreEqual(SearchScope.Sub, b.Scope, "Default scope is sub");
            }

            {
                var expected = "DC=visus, DC=uni-stuttgart, DC=de";
                var b = new SearchBase(expected, SearchScope.Base);
                Assert.AreEqual(expected, b.DistinguishedName, "DN set");
                Assert.AreEqual(SearchScope.Base, b.Scope, "Scope set");
            }
        }

        [TestMethod]
        public void Test7LdapOptions() {
            {
                var o = new LdapOptions();
                Assert.IsNotNull(o.SearchBases, "SearchBases initialised");
                Assert.AreEqual(0, o.SearchBases.Length, "SearchBases empty");
            }

            {
                var dn = "DC=visus, DC=uni-stuttgart,DC=de";
                var o = new LdapOptions();
                Assert.IsNotNull(o.SearchBases, "SearchBases initialised");
                Assert.AreEqual(0, o.SearchBases.Length, "SearchBases empty");
#pragma warning disable CS0618
                o.SearchBase = dn;
                Assert.AreEqual(1, o.SearchBases.Length, "Single SearchBase created.");
                Assert.AreSame(o.SearchBase, o.SearchBases[0].DistinguishedName, "First element set");
                o.IsSubtree = false;
                Assert.AreEqual(1, o.SearchBases.Length, "No additional SearchBase created.");
                Assert.AreEqual(SearchScope.Base, o.SearchBases[0].Scope, "Non-recursive set");
#pragma warning restore CS0618
            }

            {
                var dn = "DC=visus, DC=uni-stuttgart,DC=de";
                var o = new LdapOptions() {
                    SearchBases = new[] { new SearchBase() }
                };
                Assert.IsNotNull(o.SearchBases, "SearchBases initialised");
                Assert.AreEqual(1, o.SearchBases.Length, "SearchBases holds one element");
#pragma warning disable CS0618
                o.SearchBase = dn;
                Assert.AreEqual(1, o.SearchBases.Length, "No additional SearchBase created.");
                Assert.AreSame(o.SearchBase, o.SearchBases[0].DistinguishedName, "First element updated");
                o.IsSubtree = false;
                Assert.AreEqual(1, o.SearchBases.Length, "No additional SearchBase created.");
                Assert.AreEqual(SearchScope.Base, o.SearchBases[0].Scope, "Non-recursive set");
#pragma warning restore CS0618
            }

        }
        #endregion

        #region Private fields
        private readonly TestSecrets _testSecrets;
        #endregion
    }
}
