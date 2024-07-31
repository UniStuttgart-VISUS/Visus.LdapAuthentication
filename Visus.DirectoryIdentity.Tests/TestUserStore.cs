//// <copyright file="TestUserStore.cs" company="Visualisierungsinstitut der Universität Stuttgart">
//// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
//// Licensed under the MIT licence. See LICENCE file for details.
//// </copyright>
//// <author>Christoph Müller</author>

//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Visus.DirectoryAuthentication;


//namespace Visus.DirectoryIdentity.Tests {

//    /// <summary>
//    /// Tests for the <see cref="LdapUserStore{TUser}"/>.
//    /// </summary>
//    [TestClass]
//    public sealed class TestUserStore {

//        #region Public constructors
//        public TestUserStore() {
//            try {
//                this._configuration = new ConfigurationBuilder()
//                    .AddUserSecrets<TestSecrets>()
//                    .Build();
//                this._testSecrets = new TestSecrets();
//                this._configuration.Bind(this._testSecrets);
//            } catch {
//                this._testSecrets = null;
//            }
//        }
//        #endregion

//        [TestMethod]
//        public void TestDependencyInjection() {
//            var services = new ServiceCollection();
//            services.AddScoped(s => Mock.Of<ILogger<LdapAuthenticationService<LdapIdentityUser>>>());
//            services.AddScoped(s => Mock.Of<ILogger<LdapSearchService<LdapIdentityUser>>>());
//            services.AddIdentityCore<LdapIdentityUser>(o => o.SignIn.RequireConfirmedAccount = false).AddLdapStore(o => {
//                this._configuration.GetSection("LdapOptions").Bind(o);
//            });

//            var provider = services.BuildServiceProvider();
//            Assert.IsNotNull(provider, "Service provider valid");

//            var options = provider.GetService<IOptions<LdapOptions>>();
//            Assert.IsNotNull(options, "Can retrieve LDAP options");

//            var hasher = provider.GetService<IPasswordHasher<LdapIdentityUser>>();
//            Assert.IsNotNull(hasher, "Have password hasher.");
//            Assert.AreEqual(typeof(PasswordHasher<LdapIdentityUser>), hasher.GetType(), "Our password hasher");

//            var store = provider.GetRequiredService<IUserStore<LdapIdentityUser>>();
//            Assert.IsNotNull(store, "Resolve user store");
//        }

//        [TestMethod]
//        public async Task TestQueryUser() {
//            var services = new ServiceCollection();
//            services.AddScoped(s => Mock.Of<ILogger<LdapAuthenticationService<LdapIdentityUser>>>());
//            services.AddScoped(s => Mock.Of<ILogger<LdapSearchService<LdapIdentityUser>>>());
//            services.AddIdentityCore<LdapIdentityUser>(o => o.SignIn.RequireConfirmedAccount = false).AddLdapStore(o => {
//                this._configuration.GetSection("LdapOptions").Bind(o);
//            });

//            var provider = services.BuildServiceProvider();
//            Assert.IsNotNull(provider, "Service provider valid");

//            var store = provider.GetRequiredService<IUserStore<LdapIdentityUser>>();
//            Assert.IsNotNull(store, "Resolve user store");

//            {
//                var user = await store.FindByIdAsync(this._testSecrets.ExistingUserIdentity,
//                    CancellationToken.None);
//                Assert.IsNotNull(user, "User found by identity");
//                var identity = await store.GetUserIdAsync(user, CancellationToken.None);
//                Assert.IsNotNull(identity, "Identity found");
//                Assert.AreEqual(user.Identity, identity, "Correct identity retrieved.");
//            }
//        }

//        #region Private fields
//        private readonly IConfigurationRoot _configuration;
//        private readonly TestSecrets _testSecrets;
//        #endregion
//    }
//}
