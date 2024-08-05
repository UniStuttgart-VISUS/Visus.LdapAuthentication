// <copyright file="ServiceCollectionTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication;


namespace Visus.DirectoryIdentity.Tests {

    [TestClass]
    public class ServiceCollectionTest {

        [TestMethod]
        public async Task TestAddIdentityLdapStore() {
            if (this._testSecrets.CanRun) {
                var configuration = TestExtensions.CreateConfiguration();
                var collection = new ServiceCollection().AddMockLoggers();

                collection.AddIdentityCore<IdentityUser>()
                    .AddIdentityLdapStore(o => {
                        var section = configuration.GetSection("LdapOptions");
                        section.Bind(o);
                    });

                var services = collection.BuildServiceProvider();

                var search = services.GetService<ILdapSearchService<IdentityUser, IdentityRole>>();
                Assert.IsNotNull(search);

                var userManager = services.GetService<UserManager<IdentityUser>>();
                Assert.IsNotNull(userManager);

                var userStore = services.GetService<IQueryableUserStore<IdentityUser>>();
                Assert.IsNotNull(userStore);
                Assert.IsNotNull(userStore.Users.FirstOrDefault());

                {
                    var user = await userStore.FindByIdAsync(this._testSecrets.ExistingUserIdentity!, default);
                    Assert.IsNotNull(user);
                }
            }
        }


        [TestMethod]
        public void TestLdapIdentity() {
            if (this._testSecrets.CanRun) {
                var configuration = TestExtensions.CreateConfiguration();
                var collection = new ServiceCollection().AddMockLoggers();

                collection.AddIdentityCore<LdapIdentityUser>().AddLdapStore<LdapIdentityUser, LdapIdentityRole>(o => {
                    var section = configuration.GetSection("LdapOptions");
                    section.Bind(o);
                });

                var services = collection.BuildServiceProvider();

                var search = services.GetService<ILdapSearchService<LdapIdentityUser, LdapIdentityRole>>();
                Assert.IsNotNull(search);

                var userManager = services.GetService<UserManager<LdapIdentityUser>>();
                Assert.IsNotNull(userManager);

                var userStore = services.GetService<IQueryableUserStore<LdapIdentityUser>>();
                Assert.IsNotNull(userStore);
                Assert.IsNotNull(userStore.Users.FirstOrDefault());
            }
        }

        private readonly TestSecrets _testSecrets = TestSecrets.Create();
    }
}
