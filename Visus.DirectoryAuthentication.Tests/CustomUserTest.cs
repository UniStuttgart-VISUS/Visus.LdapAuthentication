// <copyright file="CustomUserTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Visus.Ldap;


namespace Visus.DirectoryAuthentication.Tests {

    [TestClass]
    public class CustomUserTest {

        #region Public constructors
        public CustomUserTest() {
            if (this._testSecrets?.CanRun == true) {
                var configuration = TestExtensions.CreateConfiguration();
                var collection = new ServiceCollection().AddMockLoggers();
                collection.AddLdapAuthentication<TestUser1, LdapGroup>(o => {
                    var section = configuration.GetSection("LdapOptions");
                    section.Bind(o);
                });
                collection.AddLdapAuthentication<TestUser2, LdapGroup>(o => {
                    var section = configuration.GetSection("LdapOptions");
                    section.Bind(o);
                });
                this._services = collection.BuildServiceProvider();
            }
        }
        #endregion

        [TestMethod]
        public void TestGetUser1ByAccountName() {
            if (this._testSecrets.CanRun == true) {
                Assert.IsNotNull(this._services);
                var service = this._services.GetService<ILdapSearchService<TestUser1, LdapGroup>>();
                Assert.IsNotNull(service);

                var user = service.GetUserByAccountName(this._testSecrets.ExistingUserAccount!);
                Assert.IsNotNull(user, "Search returned existing user account.");
                Assert.IsNotNull(user.Groups);
                Assert.IsTrue(user.Groups.Count() > 1);
                Assert.IsNotNull(user.ProfilePicture);
            }
        }

        [TestMethod]
        public void TestGetUser2ByAccountName() {
            if (this._testSecrets.CanRun == true) {
                Assert.IsNotNull(this._services);
                var service = this._services.GetService<ILdapSearchService<TestUser2, LdapGroup>>();
                Assert.IsNotNull(service);

                var user = service.GetUserByAccountName(this._testSecrets.ExistingUserAccount!);
                Assert.IsNotNull(user, "Search returned existing user account.");
            }
        }

        [TestMethod]
        public async Task TestGetUser1ByAccountNameAsync() {
            if (this._testSecrets.CanRun == true) {
                Assert.IsNotNull(this._services);
                var service = this._services.GetService<ILdapSearchService<TestUser1, LdapGroup>>();
                Assert.IsNotNull(service);

                var user = await service.GetUserByAccountNameAsync(this._testSecrets.ExistingUserAccount!);
                Assert.IsNotNull(user, "Search returned existing user account.");
                Assert.IsNotNull(user.ProfilePicture);
            }
        }

        [TestMethod]
        public async Task TestGetUser2ByAccountNameAsync() {
            if (this._testSecrets.CanRun == true) {
                Assert.IsNotNull(this._services);
                var service = this._services.GetService<ILdapSearchService<TestUser2, LdapGroup>>();
                Assert.IsNotNull(service);

                var user = await service.GetUserByAccountNameAsync(this._testSecrets.ExistingUserAccount!);
                Assert.IsNotNull(user, "Search returned existing user account.");
            }
        }

        [TestMethod]
        public void TestGetUser1ByIdentity() {
            if (this._testSecrets.CanRun == true) {
                Assert.IsNotNull(this._services);
                var service = this._services.GetService<ILdapSearchService<TestUser1, LdapGroup>>();
                Assert.IsNotNull(service);

                {
                    var user = service.GetUserByIdentity(this._testSecrets.ExistingUserIdentity!);
                    Assert.IsNotNull(user, "Search returned existing user identity.");
                    Assert.IsNotNull(user.ProfilePicture);
                }

                {
                    var user = service.GetUserByIdentity(this._testSecrets.NonExistingUserIdentity!);
                    Assert.IsNull(user, "Search returned no non-existing user identity.");
                }
            }
        }

        [TestMethod]
        public void TestGetUser2ByIdentity() {
            if (this._testSecrets.CanRun == true) {
                Assert.IsNotNull(this._services);
                var service = this._services.GetService<ILdapSearchService<TestUser2, LdapGroup>>();
                Assert.IsNotNull(service);

                {
                    var user = service.GetUserByIdentity(this._testSecrets.ExistingUserIdentity!);
                    Assert.IsNotNull(user, "Search returned existing user identity.");
                }

                {
                    var user = service.GetUserByIdentity(this._testSecrets.NonExistingUserIdentity!);
                    Assert.IsNull(user, "Search returned no non-existing user identity.");
                }
            }
        }

        [TestMethod]
        public async Task TestGetUser1ByIdentityAsync() {
            if (this._testSecrets.CanRun == true) {
                Assert.IsNotNull(this._services);
                var service = this._services.GetService<ILdapSearchService<TestUser1, LdapGroup>>();
                Assert.IsNotNull(service);

                {
                    var user = await service.GetUserByIdentityAsync(this._testSecrets.ExistingUserIdentity!);
                    Assert.IsNotNull(user, "Search returned existing user identity.");
                    Assert.IsNotNull(user.ProfilePicture);
                }

                {
                    var user = await service.GetUserByIdentityAsync(this._testSecrets.NonExistingUserIdentity!);
                    Assert.IsNull(user, "Search returned no non-existing user identity.");
                }
            }
        }

        [TestMethod]
        public async Task TestGetUser2ByIdentityAsync() {
            if (this._testSecrets.CanRun == true) {
                Assert.IsNotNull(this._services);
                var service = this._services.GetService<ILdapSearchService<TestUser2, LdapGroup>>();
                Assert.IsNotNull(service);

                {
                    var user = await service.GetUserByIdentityAsync(this._testSecrets.ExistingUserIdentity!);
                    Assert.IsNotNull(user, "Search returned existing user identity.");
                }

                {
                    var user = await service.GetUserByIdentityAsync(this._testSecrets.NonExistingUserIdentity!);
                    Assert.IsNull(user, "Search returned no non-existing user identity.");
                }
            }
        }

        private readonly ServiceProvider? _services;
        private readonly TestSecrets _testSecrets = TestExtensions.CreateSecrets();
    }
}
