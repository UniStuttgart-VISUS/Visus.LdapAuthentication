// <copyright file="ServiceCollectionTests.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace Visus.DirectoryAuthentication.Tests {

    /// <summary>
    /// Tests our extensions for <see cref="IServiceCollection"/>.
    /// </summary>
    [TestClass]
    public sealed class ServiceCollectionTests {

        [TestMethod]
        public void TestAuthServiceResolution() {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();

            var collection = new ServiceCollection();
            collection.AddLdapAuthenticationService(o => {
                var section = configuration.GetSection("LdapOptions");
                section.Bind(o);
                o.Schema = Schema.ActiveDirectory;
            });
            collection.AddSingleton(s => Mock.Of<ILogger<LdapAuthenticationService<LdapUser, LdapGroup>>>());
            collection.AddSingleton(s => Mock.Of<ILogger<ClaimsBuilder<LdapUser, LdapGroup>>>());

            var provider = collection.BuildServiceProvider();

            var mapper = provider.GetService<ILdapMapper<LdapUser, LdapGroup>>();
            Assert.IsNotNull(mapper, "Mapper resolved");

            var service = provider.GetService<ILdapAuthenticationService<LdapUser>>();
            Assert.IsNotNull(service, "Service resolved");

            var options = provider.GetService<IOptions<LdapOptions>>();
            Assert.IsNotNull(options?.Value, "Options resolved");
        }

        [TestMethod]
        public void TestConnectionServiceResolution() {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();

            var collection = new ServiceCollection();
            collection.AddLdapConnectionService(o => {
                var section = configuration.GetSection("LdapOptions");
                section.Bind(o);
                o.Schema = Schema.ActiveDirectory;
            });
            collection.AddScoped(s => Mock.Of<ILogger<LdapConnectionService>>());

            var provider = collection.BuildServiceProvider();
            var service = provider.GetService<ILdapConnectionService>();
            Assert.IsNotNull(service, "Service resolved");

            var options = provider.GetService<IOptions<LdapOptions>>();
            Assert.IsNotNull(options?.Value, "Options resolved");
        }

        [TestMethod]
        public void TestSearchServiceResolution() {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();

            var collection = new ServiceCollection();
            collection.AddLdapSearchService(o => {
                var section = configuration.GetSection("LdapOptions");
                section.Bind(o);
                o.Schema = Schema.ActiveDirectory;
            });
            collection.AddScoped(s => Mock.Of<ILogger<LdapSearchService<LdapUser, LdapGroup>>>());
            collection.AddSingleton(s => Mock.Of<ILogger<ClaimsBuilder<LdapUser, LdapGroup>>>());

            var provider = collection.BuildServiceProvider();

            var mapper = provider.GetService<ILdapMapper<LdapUser, LdapGroup>>();
            Assert.IsNotNull(mapper, "Mapper resolved");

            var service = provider.GetService<ILdapSearchService<LdapUser, LdapGroup>>();
            Assert.IsNotNull(service, "Service resolved");

            var options = provider.GetService<IOptions<LdapOptions>>();
            Assert.IsNotNull(options?.Value, "Options resolved");
        }

        [TestMethod]
        public void TestResolveAll() {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();

            var collection = new ServiceCollection();

            collection.AddLdapServices(o => {
                var section = configuration.GetSection("LdapOptions");
                section.Bind(o);
                o.Schema = Schema.ActiveDirectory;
            });
            collection.AddSingleton(s => Mock.Of<ILogger<LdapAuthenticationService<LdapUser, LdapGroup>>>());
            collection.AddSingleton(s => Mock.Of<ILogger<LdapSearchService<LdapUser, LdapGroup>>>());
            collection.AddSingleton(s => Mock.Of<ILogger<LdapConnectionService>>());
            collection.AddSingleton(s => Mock.Of<ILogger<ClaimsBuilder<LdapUser, LdapGroup>>>());

            var provider = collection.BuildServiceProvider();

            {
                var service = provider.GetService<ILdapAuthenticationService<LdapUser>>();
                Assert.IsNotNull(service, "ILdapAuthenticationService<LdapUser> resolved");
            }

            {
                var service = provider.GetService<ILdapConnectionService>();
                Assert.IsNotNull(service, "ILdapConnectionService resolved");
            }

            {
                var service = provider.GetService<ILdapSearchService<LdapUser, LdapGroup>>();
                Assert.IsNotNull(service, "ILdapSearchService<LdapUser> resolved");
            }

            var options = provider.GetService<IOptions<LdapOptions>>();
            Assert.IsNotNull(options?.Value, "Options resolved");
        }
    }
}
