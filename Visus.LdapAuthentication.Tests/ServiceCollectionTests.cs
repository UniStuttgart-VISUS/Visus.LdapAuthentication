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
using System;
using System.Collections.Generic;


namespace Visus.LdapAuthentication.Tests {

    /// <summary>
    /// Tests our extensions for <see cref="IServiceCollection"/>.
    /// </summary>
    [TestClass]
    public sealed class ServiceCollectionTests {

        class CustomOptions : IOptions {
            public string DefaultDomain { get; set; }
            public bool IsNoCertificateCheck { get; set; }
            public bool IsRecursiveGroupMembership { get; set; }
            public bool IsSsl { get; set; }
            public bool IsSubtree { get; set; }
            public LdapMapping Mapping { get; set; }
            public Dictionary<string, LdapMapping> Mappings { get; set; }
            public int PageSize { get; set;}
            public string Password { get; set; }
            public int Port { get; set; }
            public int ProtocolVersion { get; set; }
            public string RootCaThumbprint { get; set; }
            public string Schema { get; set; }
            public string SearchBase { get; set; }
            public IDictionary<string, SearchScope> SearchBases { get; set; }
            public string Server { get; set; }
            public string ServerCertificateIssuer { get; set; }
            public string[] ServerThumbprint { get; set; }
            public int Timeout { get; set; }
            public string User { get; set; }
        }

        [TestMethod]
        [Obsolete]
        public void TestAddOptionsFromDefaultName() {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();

            var expected = new LdapOptions();
            configuration.GetSection("LdapOptions").Bind(expected);

            var collection = new ServiceCollection();
            collection.AddLdapOptions(configuration, "LdapOptions");

            var provider = collection.BuildServiceProvider();

            {
                var actual = provider.GetService<IOptions<LdapOptions>>();
                Assert.IsNotNull(actual?.Value, "IOptions<LdapOptions> injected");
                Assert.AreEqual(expected.Server, actual?.Value.Server, "Server matches");
                Assert.AreEqual(expected.User, actual?.Value.User, "User matches");
            }
        }

        [TestMethod]
        [Obsolete]
        public void TestAddOptionsFromSection() {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();

            var expected = new LdapOptions();
            var section = configuration.GetSection("LdapOptions");
            section.Bind(expected);

            var collection = new ServiceCollection();
            collection.AddLdapOptions(section);

            var provider = collection.BuildServiceProvider();

            {
                var actual = provider.GetService<IOptions<LdapOptions>>();
                Assert.IsNotNull(actual?.Value, "IOptions<LdapOptions> injected");
                Assert.AreEqual(expected.Server, actual?.Value.Server, "Server matches");
                Assert.AreEqual(expected.User, actual?.Value.User, "User matches");
            }
        }

        [TestMethod]
        [Obsolete]
        public void TestAddCustomOptionsFromDefaultName() {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();

            var expected = new CustomOptions();
            configuration.GetSection("LdapOptions").Bind(expected);

            var collection = new ServiceCollection();
            collection.AddLdapOptions<CustomOptions>(configuration, "LdapOptions");

            var provider = collection.BuildServiceProvider();

            {
                var actual = provider.GetService<IOptions<CustomOptions>>();
                Assert.IsNotNull(actual?.Value, "IOptions<CustomOptions> injected");
                Assert.AreEqual(expected.Server, actual?.Value.Server, "Server matches");
                Assert.AreEqual(expected.User, actual?.Value.User, "User matches");
            }

            {
                var actual = provider.GetService<IOptions>();
                Assert.IsNotNull(actual, "ILdapOptions injected");
                Assert.AreEqual(expected.Server, actual.Server, "Server matches");
                Assert.AreEqual(expected.User, actual.User, "User matches");
            }
        }

        [TestMethod]
        [Obsolete]
        public void TestAddCustomOptionsFromSection() {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();

            var expected = new CustomOptions();
            var section = configuration.GetSection("LdapOptions");
            section.Bind(expected);

            var collection = new ServiceCollection();
            collection.AddLdapOptions<CustomOptions>(section);

            var provider = collection.BuildServiceProvider();

            {
                var actual = provider.GetService<IOptions<CustomOptions>>();
                Assert.IsNotNull(actual?.Value, "IOptions<CustomOptions> injected");
                Assert.AreEqual(expected.Server, actual?.Value.Server, "Server matches");
                Assert.AreEqual(expected.User, actual?.Value.User, "User matches");
            }

            {
                var actual = provider.GetService<IOptions>();
                Assert.IsNotNull(actual, "ILdapOptions injected");
                Assert.AreEqual(expected.Server, actual.Server, "Server matches");
                Assert.AreEqual(expected.User, actual.User, "User matches");
            }
        }

        [TestMethod]
        [Obsolete]
        public void TestAuthServiceResolution() {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();

            var collection = new ServiceCollection();
            collection.AddLdapAuthenticationService();
            collection.AddScoped(s => Mock.Of<ILogger<LdapAuthenticationService<LdapUser>>>());

            {
                var provider = collection.BuildServiceProvider();
                Assert.ThrowsException<InvalidOperationException>(() => provider.GetService<ILdapAuthenticationService>());
            }

            var section = configuration.GetSection("LdapOptions");
            collection.AddLdapOptions(section);

            {
                var provider = collection.BuildServiceProvider();
                var service = provider.GetService<ILdapAuthenticationService>();
                Assert.IsNotNull(service, "Service resolved");
            }
        }

        [TestMethod]
        public void TestAuthServiceResolutionWithAction() {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();

            var collection = new ServiceCollection();
            collection.AddLdapAuthenticationService(o => {
                configuration.GetSection("LdapOptions").Bind(o);
            });
            collection.AddScoped(s => Mock.Of<ILogger<LdapAuthenticationService<LdapUser>>>());

            var provider = collection.BuildServiceProvider();

            var service = provider.GetService<ILdapAuthenticationService>();
            Assert.IsNotNull(service, "Service resolved");

            var options = provider.GetService<IOptions<LdapOptions>>();
            Assert.IsNotNull(options?.Value, "Options also available");
        }

        [TestMethod]
        public void TestTypedAuthServiceResolutionWithAction() {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();

            var collection = new ServiceCollection();
            collection.AddLdapAuthenticationService<LdapUser>(o => {
                configuration.GetSection("LdapOptions").Bind(o);
            });
            collection.AddScoped(s => Mock.Of<ILogger<LdapAuthenticationService<LdapUser>>>());

            var provider = collection.BuildServiceProvider();

            var service = provider.GetService<ILdapAuthenticationService<LdapUser>>();
            Assert.IsNotNull(service, "Service resolved");

            var options = provider.GetService<IOptions<LdapOptions>>();
            Assert.IsNotNull(options?.Value, "Options also available");
        }

        [TestMethod]
        public void TestAddAllServices() {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();

            var collection = new ServiceCollection();

            collection.AddLdapAuthenticationService<LdapUser>(o => {
                configuration.GetSection("LdapOptions").Bind(o);
            });
            collection.AddLdapAuthenticationService(o => {
                configuration.GetSection("LdapOptions").Bind(o);
            });
            collection.AddScoped(s => Mock.Of<ILogger<LdapAuthenticationService<LdapUser>>>());

            collection.AddLdapConnectionService(o => {
                configuration.GetSection("LdapOptions").Bind(o);
            });
            collection.AddScoped(s => Mock.Of<ILogger<LdapConnectionService>>());

            collection.AddLdapSearchService<LdapUser>(o => {
                configuration.GetSection("LdapOptions").Bind(o);
            });
            collection.AddLdapSearchService(o => {
                configuration.GetSection("LdapOptions").Bind(o);
            });
            collection.AddScoped(s => Mock.Of<ILogger<LdapSearchService<LdapUser>>>());

            var provider = collection.BuildServiceProvider();

            {
                var service = provider.GetService<ILdapAuthenticationService<LdapUser>>();
                Assert.IsNotNull(service, "ILdapAuthenticationService<LdapUser> resolved");
            }

            {
                var service = provider.GetService<ILdapAuthenticationService>();
                Assert.IsNotNull(service, "ILdapAuthenticationService resolved");
            }

            {
                var service = provider.GetService<ILdapConnectionService>();
                Assert.IsNotNull(service, "ILdapConnectionService resolved");
            }

            {
                var service = provider.GetService<ILdapSearchService<LdapUser>>();
                Assert.IsNotNull(service, "ILdapSearchService<LdapUser> resolved");
            }

            {
                var service = provider.GetService<ILdapSearchService>();
                Assert.IsNotNull(service, "ILdapSearchService resolved");
            }

            {
                var options = provider.GetService<IOptions<LdapOptions>>();
                Assert.IsNotNull(options?.Value, "Options also available");
            }
        }
    }
}
