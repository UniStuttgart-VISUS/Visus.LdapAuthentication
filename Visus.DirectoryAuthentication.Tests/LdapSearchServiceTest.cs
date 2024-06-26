﻿// <copyright file="LdapOptionsTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
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
using System.Threading.Tasks;


namespace Visus.DirectoryAuthentication.Tests {

    /// <summary>
    /// Tests the <see cref="LdapSearchService"/>.
    /// </summary>
    [TestClass]
    public class LdapSearchServiceTest {

        #region Nested class CustomMapper1
        private sealed class CustomMapper1 : ILdapMapper<LdapUser, LdapGroup> {

            public CustomMapper1(LdapOptions options) {
                this._base = new(options, null);
                this._options = options;
            }

            public IEnumerable<string> RequiredGroupAttributes => this._base.RequiredGroupAttributes;

            public IEnumerable<string> RequiredUserAttributes => this._base.RequiredUserAttributes;

            public void Assign(LdapUser user, SearchResultEntry entry, LdapConnection connection, ILogger logger)
                => this._base.Assign(user, entry, connection, logger);

            public string GetIdentity(LdapUser user) => this._base.GetIdentity(user);

            public IEnumerable<LdapGroup> GetGroups(LdapUser user) => this._base.GetGroups(user);

            public string GetIdentity(SearchResultEntry entry) => this._base.GetIdentity(entry);


            private readonly LdapMapper<LdapUser, LdapGroup> _base;
            private readonly LdapOptions _options;
        }
        #endregion

        #region Public constructors
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
        #endregion

        [TestMethod]
        public void TestGetDistinguishedNames() {
            if (this._testSecrets?.LdapOptions != null) {
                var options = Options.Create(this._testSecrets.LdapOptions);
                var service = new LdapSearchService<LdapUser, LdapGroup>(
                    new LdapMapper<LdapUser, LdapGroup>(options),
                    options,
                    Mock.Of<ILogger<LdapSearchService<LdapUser, LdapGroup>>>());

                var att = LdapAttributeAttribute.GetLdapAttribute<LdapUser>(
                    nameof(LdapUser.AccountName),
                    this._testSecrets.LdapOptions.Schema);
                var users = service.GetDistinguishedNames($"({att.Name}={this._testSecrets.ExistingUserAccount})");
                Assert.IsTrue(users.Any(), "Search returned at least one DN.");
            }
        }

        [TestMethod]
        public async Task TestGetDistinguishedNamesAsync() {
            if (this._testSecrets?.LdapOptions != null) {
                var options = Options.Create(this._testSecrets.LdapOptions);
                var mapper = new LdapMapper<LdapUser, LdapGroup>(options);
                var service = new LdapSearchService<LdapUser, LdapGroup>(
                    mapper,
                    options,
                    Mock.Of<ILogger<LdapSearchService<LdapUser, LdapGroup>>>());

                var att = LdapAttributeAttribute.GetLdapAttribute<LdapUser>(
                    nameof(LdapUser.AccountName),
                    this._testSecrets.LdapOptions.Schema);
                var users = await service.GetDistinguishedNamesAsync($"({att.Name}={this._testSecrets.ExistingUserAccount})");
                Assert.IsTrue(users.Any(), "Search returned at least one DN.");
            }
        }

        [TestMethod]
        public void TestGetUserByIdentity() {
            if (this._testSecrets?.LdapOptions != null) {
                var options = Options.Create(this._testSecrets.LdapOptions);
                var claims = new ClaimsBuilder<LdapUser, LdapGroup>(
                    Mock.Of<ILogger<ClaimsBuilder<LdapUser, LdapGroup>>>());
                var mapper = new LdapMapper<LdapUser, LdapGroup>(options, claims);
                var service = new LdapSearchService<LdapUser, LdapGroup>(
                    mapper,
                    options,
                    Mock.Of<ILogger<LdapSearchService<LdapUser, LdapGroup>>>());

                var user = service.GetUserByIdentity(this._testSecrets.ExistingUserIdentity);
                Assert.IsNotNull(user, "Existing user was found.");
                Assert.IsNotNull(user.Claims, "Mapper has set claims");
                Assert.IsTrue(user.Claims.Any(), "At least one claim");
            }
        }

        [TestMethod]
        public async Task TestGetUserByIdentityAsync() {
            if (this._testSecrets?.LdapOptions != null) {
                var options = Options.Create(this._testSecrets.LdapOptions);
                var mapper = new LdapMapper<LdapUser, LdapGroup>(options);
                var service = new LdapSearchService<LdapUser, LdapGroup>(
                    mapper,
                    options,
                    Mock.Of<ILogger<LdapSearchService<LdapUser, LdapGroup>>>());

                var user = await service.GetUserByIdentityAsync(this._testSecrets.ExistingUserIdentity);
                Assert.IsNotNull(user, "Existing user was found.");
            }
        }

        [TestMethod]
        public void TestGetUsers() {
            if (this._testSecrets?.LdapOptions != null) {
                var options = Options.Create(this._testSecrets.LdapOptions);
                var mapper = new LdapMapper<LdapUser, LdapGroup>(options);
                var service = new LdapSearchService<LdapUser, LdapGroup>(
                    mapper,
                    options,
                    Mock.Of<ILogger<LdapSearchService<LdapUser, LdapGroup>>>());

                var users = service.GetUsers();
                Assert.IsTrue(users.Any(), "Directory search returns any user.");
            }
        }

        [TestMethod]
        public void TestGetUsersFiltered() {
            if (this._testSecrets?.LdapOptions != null) {
                var options = Options.Create(this._testSecrets.LdapOptions);
                var mapper = new LdapMapper<LdapUser, LdapGroup>(options);
                var service = new LdapSearchService<LdapUser, LdapGroup>(
                    mapper,
                    options,
                    Mock.Of<ILogger<LdapSearchService<LdapUser, LdapGroup>>>());

                var att = LdapAttributeAttribute.GetLdapAttribute<LdapUser>(
                    nameof(LdapUser.AccountName),
                    this._testSecrets.LdapOptions.Schema);
                var users = service.GetUsers($"({att.Name}={this._testSecrets.ExistingUserAccount})");
                Assert.IsNotNull(users.Any(), "Filtered user was found.");
            }
        }

        [TestMethod]
        public async Task TestGetUsersAsync() {
            if (this._testSecrets?.LdapOptions != null) {
                var options = Options.Create(this._testSecrets.LdapOptions);
                var mapper = new LdapMapper<LdapUser, LdapGroup>(options);
                var service = new LdapSearchService<LdapUser, LdapGroup>(
                    mapper,
                    options,
                    Mock.Of<ILogger<LdapSearchService<LdapUser, LdapGroup>>>());

                var users = await service.GetUsersAsync();
                Assert.IsTrue(users.Any(), "Directory search returns any user.");
            }
        }

        [TestMethod]
        public async Task TestGetUsersAsyncFiltered() {
            if (this._testSecrets?.LdapOptions != null) {
                var options = Options.Create(this._testSecrets.LdapOptions);
                var mapper = new LdapMapper<LdapUser, LdapGroup>(options);
                var service = new LdapSearchService<LdapUser, LdapGroup>(
                    mapper,
                    options,
                    Mock.Of<ILogger<LdapSearchService<LdapUser, LdapGroup>>>());

                var att = LdapAttributeAttribute.GetLdapAttribute<LdapUser>(
                    nameof(LdapUser.AccountName),
                    this._testSecrets.LdapOptions.Schema);
                var users = await service.GetUsersAsync($"({att.Name}={this._testSecrets.ExistingUserAccount})");
                Assert.IsNotNull(users.Any(), "Filtered user was found.");
            }
        }

        [TestMethod]
        public async Task TestCustomMapper1() {
            if (this._testSecrets?.LdapOptions != null) {
                var options = Options.Create(this._testSecrets.LdapOptions);
                var mapper = new CustomMapper1(options.Value);
                var service = new LdapSearchService<LdapUser, LdapGroup>(
                    mapper,
                    options,
                    Mock.Of<ILogger<LdapSearchService<LdapUser, LdapGroup>>>());

                var att = LdapAttributeAttribute.GetLdapAttribute<LdapUser>(
                    nameof(LdapUser.AccountName),
                    this._testSecrets.LdapOptions.Schema);
                var users = await service.GetUsersAsync($"({att.Name}={this._testSecrets.ExistingUserAccount})");
                Assert.IsNotNull(users.Any(), "Filtered user was found.");
                Assert.IsNotNull(users.First().AccountName, "Have account");
                Assert.IsFalse(users.First().Claims?.Any() == true, "Custom mapper does not create claims.");
            }
        }

        private readonly TestSecrets _testSecrets;
    }
}
