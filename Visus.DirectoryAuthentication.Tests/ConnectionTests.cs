// <copyright file="ServiceCollectionTests.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Threading.Tasks;


namespace Visus.DirectoryAuthentication.Tests {

    [TestClass]
    public sealed class ConnectionTests {

        public ConnectionTests() {
            new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build()
                .Bind(this._testSecrets = new());
        }

        [TestMethod]
        public void GetRootDse() {
            var connection = this._testSecrets.LdapOptions.Connect(Mock.Of<ILogger>());
            Assert.IsNotNull(connection);
            var rootDse = connection.GetRootDse();
            Assert.IsNotNull(rootDse);
        }

        [TestMethod]
        public async Task GetRootDseAsync() {
            var connection = this._testSecrets.LdapOptions.Connect(Mock.Of<ILogger>());
            Assert.IsNotNull(connection);
            var rootDse = await connection.GetRootDseAsync();
            Assert.IsNotNull(rootDse);
        }

        private readonly TestSecrets _testSecrets;
    }
}
