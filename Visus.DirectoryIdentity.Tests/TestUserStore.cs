// <copyright file="TestUserStore.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication;


namespace Visus.DirectoryIdentity.Tests {

    /// <summary>
    /// Tests for the <see cref="LdapUserStore{TUser}"/>.
    /// </summary>
    [TestClass]
    public sealed class TestUserStore {

        #region Public constructors
        public TestUserStore() {
            try {
                var configuration = new ConfigurationBuilder()
                    .AddUserSecrets<TestSecrets>()
                    .Build();
                this._testSecrets = new TestSecrets();
                configuration.Bind(this._testSecrets);

                var services = new ServiceCollection();
                services.AddLdapOptions(configuration);
                services.AddLdapAuthenticationService<LdapIdentityUser>();
                services.AddLdapSearchService<LdapIdentityUser>();
                services.AddScoped(s => Mock.Of<ILogger<LdapAuthenticationService<LdapIdentityUser>>>());
                services.AddScoped(s => Mock.Of<ILogger<LdapSearchService<LdapIdentityUser>>>());
                this._services = services.BuildServiceProvider();
            } catch {
                this._testSecrets = null;
            }
        }
        #endregion

        [TestMethod]
        public async Task TestDependencyInjection() {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();

            var services = new ServiceCollection();
            services.AddLdapOptions(configuration);
            services.AddLdapAuthenticationService<LdapIdentityUser>();
            services.AddLdapSearchService<LdapIdentityUser>();
            services.AddScoped(s => Mock.Of<ILogger<LdapAuthenticationService<LdapIdentityUser>>>());
            services.AddScoped(s => Mock.Of<ILogger<LdapSearchService<LdapIdentityUser>>>());
            services.AddIdentityCore<LdapIdentityUser>(o => o.SignIn.RequireConfirmedAccount = false).AddLdapStore();

            var provider = services.BuildServiceProvider();
            Assert.IsNotNull(provider, "Service provider valid");

            var options = provider.GetService<IOptions<LdapOptions>>();
            Assert.IsNotNull(options, "Can retrieve LDAP options");

            var hasher = provider.GetService<IPasswordHasher<LdapIdentityUser>>();
            Assert.IsNotNull(hasher, "Have password hasher.");
            Assert.AreEqual(typeof(PasswordHasher<LdapIdentityUser>), hasher.GetType(), "Our password hasher");

            var store = provider.GetRequiredService<IUserStore<LdapIdentityUser>>();
            Assert.IsNotNull(store, "Resolve user store");
        }

        [TestMethod]
        public async Task TestQueryUser() {
            var authService = this._services.GetService<ILdapAuthenticationService<LdapIdentityUser>>();
            Assert.IsNotNull(authService, "Authentication service created");

            var searchService = this._services.GetService<ILdapSearchService<LdapIdentityUser>>();
            Assert.IsNotNull(searchService, "Search service created");

            var options = this._services.GetService<IOptions<LdapOptions>>();
            Assert.IsNotNull(options, "Options created");

            var store = new LdapUserStore<LdapIdentityUser>(authService, searchService, options);
            Assert.IsNotNull(store, "Store created");

            {
                var user = await store.FindByIdAsync(this._testSecrets.ExistingUserIdentity,
                    CancellationToken.None);
                Assert.IsNotNull(user, "User found by identity");
                var identity = await store.GetUserIdAsync(user, CancellationToken.None);
                Assert.IsNotNull(identity, "Identity found");
                Assert.AreEqual(user.Identity, identity, "Correct identity retrieved.");
            }
        }

        #region Private fields
        private readonly IServiceProvider _services;
        private readonly TestSecrets _testSecrets;
        #endregion
    }
}
