// <copyright file="ServiceCollectionTests.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;


namespace Visus.DirectoryAuthentication.Tests {

    /// <summary>
    /// Tests our extensions for <see cref="IServiceCollection"/>.
    /// </summary>
    [TestClass]
    public sealed class ServiceCollectionTests {

        class CustomOptions : ILdapOptions {
            public string DefaultDomain { get; set; }
            public bool IsNoCertificateCheck { get; set; }
            public bool IsRecursiveGroupMembership { get; set; }
            public bool IsSsl { get; set; }
            public LdapMapping Mapping { get; set; }
            public Dictionary<string, LdapMapping> Mappings { get; set; }
            public int PageSize { get; set;}
            public string Password { get; set; }
            public int Port { get; set; }
            public int ProtocolVersion { get; set; }
            public string Schema { get; set; }
            public IDictionary<string, SearchScope> SearchBase { get; set; }
            public string Server { get; set; }
            public string ServerCertificateIssuer { get; set; }
            public string[] ServerThumbprint { get; set; }
            public TimeSpan Timeout { get; set; }
            public string User { get; set; }
        }

        [TestMethod]
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
                var actual = provider.GetService<ILdapOptions>();
                Assert.IsNotNull(actual, "ILdapOptions injected");
                Assert.AreEqual(expected.Server, actual.Server, "Server matches");
                Assert.AreEqual(expected.User, actual.User, "User matches");
            }
        }

        [TestMethod]
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
                var actual = provider.GetService<ILdapOptions>();
                Assert.IsNotNull(actual, "ILdapOptions injected");
                Assert.AreEqual(expected.Server, actual.Server, "Server matches");
                Assert.AreEqual(expected.User, actual.User, "User matches");
            }
        }
    }
}
