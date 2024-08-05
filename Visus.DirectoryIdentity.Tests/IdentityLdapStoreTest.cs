// <copyright file="IdentityLdapStoreTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace Visus.DirectoryIdentity.Tests {

    [TestClass]
    public sealed class IdentityLdapStoreTest {

        public IdentityLdapStoreTest() {
            var configuration = TestExtensions.CreateConfiguration();
            var collection = new ServiceCollection().AddMockLoggers();

            if (this._testSecrets.CanRun) {
                collection.AddIdentityCore<IdentityUser>()
                    .AddIdentityLdapStore(o => {
                        var section = configuration.GetSection("LdapOptions");
                        section.Bind(o);
                    });
            }

            this._services = collection.BuildServiceProvider();
        }

        [TestMethod]
        public async Task TestFindByIdAsync() {
            if (this._testSecrets.CanRun) {
                var userStore = this._services.GetService<IUserStore<IdentityUser>>();
                Assert.IsNotNull(userStore);
                var user = await userStore.FindByIdAsync(this._testSecrets.ExistingUserIdentity!, default);
                Assert.IsNotNull(user);
                Assert.AreEqual(this._testSecrets.ExistingUserIdentity, await userStore.GetUserIdAsync(user, default));
            }
        }

        [TestMethod]
        public async Task TestFindByNameAsync() {
            if (this._testSecrets.CanRun) {
                var userStore = this._services.GetService<IUserStore<IdentityUser>>();
                Assert.IsNotNull(userStore);
                var user = await userStore.FindByNameAsync(this._testSecrets.ExistingUserAccount!, default);
                Assert.IsNotNull(user);
                Assert.AreEqual(this._testSecrets.ExistingUserAccount, await userStore.GetUserNameAsync(user, default));
            }
        }

        [TestMethod]
        public async Task TestGetUsersForClaimAsync() {
            if (this._testSecrets.CanRun) {
                var userStore = this._services.GetService<IUserClaimStore<IdentityUser>>();
                Assert.IsNotNull(userStore);

                var claim = new Claim(ClaimTypes.Name, this._testSecrets.ExistingUserAccount!);
                var users = await userStore.GetUsersForClaimAsync(claim, default);
                Assert.IsNotNull(users);
                Assert.IsTrue(users.Count() == 1);
            }
        }

        private readonly ServiceProvider _services;
        private readonly TestSecrets _testSecrets = TestSecrets.Create();
    }
}
