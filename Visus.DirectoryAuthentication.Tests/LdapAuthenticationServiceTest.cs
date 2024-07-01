// <copyright file="LdapAuthenticationServiceTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;


namespace Visus.DirectoryAuthentication.Tests {

    /// <summary>
    /// Tests for the <see cref="LdapAuthenticationService"/>.
    /// </summary>
    [TestClass]
    public sealed class LdapAuthenticationServiceTest {

        [TestMethod]
        public void TestLogin() {
            var secrets = new TestSecrets();
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();
            configuration.Bind(secrets);

            if (secrets?.LdapOptions != null) {
                var collection = new ServiceCollection();

                collection.AddLdapServices(o => {
                    var section = configuration.GetSection("LdapOptions");
                    section.Bind(o);
                });

                collection.AddSingleton(s => Mock.Of<ILogger<LdapAuthenticationService<LdapUser, LdapGroup>>>());
                collection.AddSingleton(s => Mock.Of<ILogger<LdapSearchService<LdapUser, LdapGroup>>>());
                collection.AddSingleton(s => Mock.Of<ILogger<LdapConnectionService>>());
                collection.AddSingleton(s => Mock.Of<ILogger<ClaimsBuilder<LdapUser, LdapGroup>>>());

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
