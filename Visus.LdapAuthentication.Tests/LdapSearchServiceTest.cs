// <copyright file="LdapOptionsTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Novell.Directory.Ldap;
using System.Collections.Generic;
using System.Linq;
using Visus.LdapAuthentication.Configuration;
using Visus.LdapAuthentication.Extensions;
using Visus.LdapAuthentication.Services;


namespace Visus.LdapAuthentication.Tests
{

    /// <summary>
    /// Tests the <see cref="LdapSearchService"/>.
    /// </summary>
    [TestClass]
    public class LdapSearchServiceTest {

        #region Nested class CustomMapper1
        private sealed class CustomMapper1 : ILdapUserMapper<LdapUser> {

            public CustomMapper1(LdapOptions options) {
                this._base = new(options);
                this._options = options;
            }

            public IEnumerable<string> RequiredAttributes => this._base.RequiredAttributes;

            public void Assign(LdapUser user, LdapEntry entry, LdapConnection connection, ILogger logger)
                => entry.AssignTo(user, this._options);

            public string GetIdentity(LdapUser user) => this._base.GetIdentity(user);

            public string GetIdentity(LdapEntry entry) => this._base.GetIdentity(entry);

            private readonly LdapUserMapper<LdapUser> _base;
            private readonly LdapOptions _options;
        }
        #endregion

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
                var options = Options.Create(this._testSecrets.LdapOptions);
                var service = new LdapSearchService<LdapUser>(
                    new LdapUserMapper<LdapUser>(options),
                    options,
                    Mock.Of<ILogger<LdapSearchService<LdapUser>>>());

                var user = service.GetUserByIdentity(this._testSecrets.ExistingUserIdentity);
                Assert.IsNotNull(user, "Existing user was found.");
            }
        }

        [TestMethod]
        public void TestGetUsers() {
            if (this._testSecrets?.LdapOptions != null) {
                var options = Options.Create(this._testSecrets.LdapOptions);
                var service = new LdapSearchService<LdapUser>(
                    new LdapUserMapper<LdapUser>(options),
                    options,
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
                var options = Options.Create(this._testSecrets.LdapOptions);
                var service = new LdapSearchService<LdapUser>(
                    new LdapUserMapper<LdapUser>(options),
                    options,
                    Mock.Of<ILogger<LdapSearchService<LdapUser>>>());

                var att = LdapAttributeAttribute.GetLdapAttribute<LdapUser>(
                    nameof(LdapUser.AccountName),
                    this._testSecrets.LdapOptions.Schema);
                var users = service.GetDistinguishedNames($"({att.Name}={this._testSecrets.ExistingUserAccount})");
                Assert.IsTrue(users.Any(), "Search returned at least one DN.");
            }
        }


        [TestMethod]
        public void TestCustomMapper1() {
            if (this._testSecrets?.LdapOptions != null) {
                var options = Options.Create(this._testSecrets.LdapOptions);
                var service = new LdapSearchService<LdapUser>(
                    new CustomMapper1(options.Value),
                    options,
                    Mock.Of<ILogger<LdapSearchService<LdapUser>>>());

                var att = LdapAttributeAttribute.GetLdapAttribute<LdapUser>(
                    nameof(LdapUser.AccountName),
                    this._testSecrets.LdapOptions.Schema);
                var users = service.GetUsers($"({att.Name}={this._testSecrets.ExistingUserAccount})");
                Assert.IsNotNull(users.Any(), "Filtered user was found.");
                Assert.IsNotNull(users.First().AccountName, "Have account");
                Assert.IsFalse(users.First().Claims.Any(), "Custom mapper does not create claims.");
            }
        }

        private readonly TestSecrets _testSecrets;
    }
}
