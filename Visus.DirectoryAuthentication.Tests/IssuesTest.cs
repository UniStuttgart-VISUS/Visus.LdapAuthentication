// <copyright file="IssuesTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.DirectoryServices.Protocols;


namespace Visus.DirectoryAuthentication.Tests {

    /// <summary>
    /// Tests for issues from GitHub.
    /// </summary>
    [TestClass]
    public sealed class IssuesTest {

        public IssuesTest() {
            try {
                var configuration = new ConfigurationBuilder()
                    .AddUserSecrets<TestSecrets>()
                    .Build();
                this._testSecrets = new TestSecrets();
                configuration.Bind(this._testSecrets);

            } catch {
                this._testSecrets = null;
            }
        }

        [TestMethod]
        public void TestIssue10() {
            if (this._testSecrets != null) {
                var service = new LdapAuthenticationService<LdapUser>(
                    this._testSecrets.LdapOptions,
                    Mock.Of<ILogger<LdapAuthenticationService<LdapUser>>>());

                Assert.ThrowsException<LdapException>(() => {
                    var user = service.Login(this._testSecrets.LdapOptions.User,
                        this._testSecrets.LdapOptions.Password + " is wrong");
                });
            }
        }

        private readonly TestSecrets _testSecrets;
    }
}
