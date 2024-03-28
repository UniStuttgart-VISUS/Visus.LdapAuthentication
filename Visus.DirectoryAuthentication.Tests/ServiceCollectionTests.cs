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
                section.Bind(o)
            });
            collection.AddScoped(s => Mock.Of<ILogger<LdapAuthenticationService<LdapUser>>>());

            var provider = collection.BuildServiceProvider();

            var mapper = provider.GetService<ILdapUserMapper<LdapUser>>();
            Assert.IsNotNull(mapper, "Mapper resolved");

            var service = provider.GetService<ILdapAuthenticationService>();
            Assert.IsNotNull(service, "Service resolved");

            var typedService = provider.GetService<ILdapAuthenticationService<LdapUser>>();
            Assert.IsNotNull(service, "Typed service resolved");

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
            });
            collection.AddScoped(s => Mock.Of<ILogger<LdapSearchService<LdapUser>>>());

            var provider = collection.BuildServiceProvider();

            var mapper = provider.GetService<ILdapUserMapper<LdapUser>>();
            Assert.IsNotNull(mapper, "Mapper resolved");

            var service = provider.GetService<ILdapSearchService>();
            Assert.IsNotNull(service, "Service resolved");

            var typedService = provider.GetService<ILdapSearchService<LdapUser>>();
            Assert.IsNotNull(service, "Typed service resolved");

            var options = provider.GetService<IOptions<LdapOptions>>();
            Assert.IsNotNull(options?.Value, "Options resolved");
        }

        [TestMethod]
        public void TestResolveAll() {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();

            var collection = new ServiceCollection();

            collection.AddLdapAuthenticationService(o => {
                var section = configuration.GetSection("LdapOptions");
                section.Bind(o);
            });
            collection.AddScoped(s => Mock.Of<ILogger<LdapAuthenticationService<LdapUser>>>());

            collection.AddLdapConnectionService(o => {
                var section = configuration.GetSection("LdapOptions");
                section.Bind(o);
            });
            collection.AddScoped(s => Mock.Of<ILogger<LdapConnectionService>>());

            collection.AddLdapSearchService(o => {
                var section = configuration.GetSection("LdapOptions");
                section.Bind(o);
            });
            collection.AddScoped(s => Mock.Of<ILogger<LdapSearchService<LdapUser>>>());

            var provider = collection.BuildServiceProvider();

            {
                var service = provider.GetService<ILdapAuthenticationService>();
                Assert.IsNotNull(service, "ILdapAuthenticationService resolved");
            }

            {
                var service = provider.GetService<ILdapAuthenticationService<LdapUser>>();
                Assert.IsNotNull(service, "ILdapAuthenticationService<LdapUser> resolved");
            }

            {
                var service = provider.GetService<ILdapConnectionService>();
                Assert.IsNotNull(service, "ILdapConnectionService resolved");
            }

            {
                var service = provider.GetService<ILdapSearchService>();
                Assert.IsNotNull(service, "ILdapSearchService resolved");
            }

            {
                var service = provider.GetService<ILdapSearchService<LdapUser>>();
                Assert.IsNotNull(service, "ILdapSearchService<LdapUser> resolved");
            }

            var options = provider.GetService<IOptions<LdapOptions>>();
            Assert.IsNotNull(options?.Value, "Options resolved");
        }
    }
}
