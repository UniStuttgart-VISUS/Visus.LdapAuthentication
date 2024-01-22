// <copyright file="ServiceCollectionTests.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Visus.DirectoryAuthentication.Tests {

    /// <summary>
    /// Tests our extensions for <see cref="IServiceCollection"/>.
    /// </summary>
    [TestClass]
    public sealed class ServiceCollectionTests {

        [TestMethod]
        public void TestAddOptions() {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();

            var expected = new LdapOptions();
            configuration.GetSection("LdapOptions").Bind(expected);

            var collection = new ServiceCollection();
            collection.AddLdapOptions(configuration);

            var provider = collection.BuildServiceProvider();

            {
                var actual = provider.GetService<ILdapOptions>();
                Assert.IsNotNull(actual, "ILdapOptions injected");
                Assert.AreEqual(expected.Server, actual.Server, "Server matches");
                Assert.AreEqual(expected.User, actual.User, "User matches");
            }

            {
                var actual = provider.GetService<IOptions<LdapOptions>>();
                Assert.IsNotNull(actual?.Value, "IOptions<LdapOptions> injected");
                Assert.AreEqual(expected.Server, actual?.Value.Server, "Server matches");
                Assert.AreEqual(expected.User, actual?.Value.User, "User matches");
            }
        }
    }
}
