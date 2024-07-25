// <copyright file="LdapAuthenticationServiceTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Visus.DirectoryAuthentication.Services;


namespace Visus.DirectoryAuthentication.Tests
{

    /// <summary>
    /// Tests for the <see cref="LdapAuthenticationService"/>.
    /// </summary>
    [TestClass]
    public sealed class LdapAuthenticationServiceTest {

        [TestMethod]
        public void TestLogin() {
            var configuration = TestExtensions.CreateConfiguration();
            var secrets = TestExtensions.CreateSecrets();
            var section = configuration.GetSection("LdapOptions");

            if (section != null) {
                var collection = new ServiceCollection().AddMockLoggers();
                collection.AddLdapServices(o => {
                    section.Bind(o);
                });

                var provider = collection.BuildServiceProvider();

                var service = provider.GetService<ILdapAuthenticationService<LdapUser>>();
                Assert.IsNotNull(service);

                var user = service.Login(secrets.LdapOptions.User, secrets.LdapOptions.Password);
                Assert.IsNotNull(user);
                Assert.IsNotNull(user.Groups);
                Assert.IsTrue(user.Groups.Any());
                Assert.IsNotNull(user.Claims);
                Assert.IsTrue(user.Claims.Any());
            }
        }
    }
}
