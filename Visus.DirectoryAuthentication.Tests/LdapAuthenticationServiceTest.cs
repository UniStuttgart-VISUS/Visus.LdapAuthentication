// <copyright file="LdapAuthenticationServiceTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Threading.Tasks;
using Visus.Ldap;


namespace Visus.DirectoryAuthentication.Tests {

    /// <summary>
    /// Tests for the <see cref="LdapAuthenticationService"/>.
    /// </summary>
    [TestClass]
    public sealed class LdapAuthenticationServiceTest {

        public LdapAuthenticationServiceTest() {
            var configuration = TestExtensions.CreateConfiguration();
            var collection = new ServiceCollection().AddMockLoggers();
            collection.AddLdapAuthentication(o => {
                var section = configuration.GetSection("LdapOptions");
                section.Bind(o);
            });
            this._services = collection.BuildServiceProvider();
        }

        [TestMethod]
        public void TestLoginPrincipal() {
            if (this._testSecrets?.LdapOptions != null) {
                var service = this._services.GetService<ILdapAuthenticationService<LdapUser>>();
                Assert.IsNotNull(service);

                var principal = service.LoginPrincipal(
                    this._testSecrets.LdapOptions.User!,
                    this._testSecrets.LdapOptions.Password!);
                Assert.IsNotNull(principal);
                Assert.IsTrue(principal.Claims.Any());
            }
        }

        [TestMethod]
        public async Task TestLoginPrincipalAsync() {
            if (this._testSecrets?.LdapOptions != null) {
                var service = this._services.GetService<ILdapAuthenticationService<LdapUser>>();
                Assert.IsNotNull(service);

                var principal = await service.LoginPrincipalAsync(
                    this._testSecrets.LdapOptions.User!,
                    this._testSecrets.LdapOptions.Password!);
                Assert.IsNotNull(principal);
                Assert.IsTrue(principal.Claims.Any());
            }
        }

        [TestMethod]
        public void TestLoginUser() {
            if (this._testSecrets?.LdapOptions != null) {
                var service = this._services.GetService<ILdapAuthenticationService<LdapUser>>();
                Assert.IsNotNull(service);

                var user = service.LoginUser(
                    this._testSecrets.LdapOptions.User!,
                    this._testSecrets.LdapOptions.Password!);
                Assert.IsNotNull(user);
                Assert.IsNotNull(user.Groups);
                Assert.IsTrue(user.Groups.Any());
                Assert.IsTrue(user.Groups.Count() >= 1);
                Assert.IsFalse(user.Groups.Any(g => g == null));
            }
        }

        [TestMethod]
        public async Task TestLoginUserAsync() {
            if (this._testSecrets?.LdapOptions != null) {
                var service = this._services.GetService<ILdapAuthenticationService<LdapUser>>();
                Assert.IsNotNull(service);

                var user = await service.LoginUserAsync(
                    this._testSecrets.LdapOptions.User!,
                    this._testSecrets.LdapOptions.Password!);
                Assert.IsNotNull(user);
                Assert.IsNotNull(user.Groups);
                Assert.IsTrue(user.Groups.Any());
                Assert.IsTrue(user.Groups.Count() >= 1);
                Assert.IsFalse(user.Groups.Any(g => g == null));
            }
        }

        [TestMethod]
        public void TestLoginUserFailure() {
            if (this._testSecrets?.LdapOptions != null) {
                var service = this._services.GetService<ILdapAuthenticationService<LdapUser>>();
                Assert.IsNotNull(service);

                Assert.ThrowsException<LdapException>(() => {
                    service.LoginUser(
                        this._testSecrets.LdapOptions.User!,
                        this._testSecrets.LdapOptions.Password! + " is wrong");
                });
            }
        }

        [TestMethod]
        public void TestLoginUserFailureAsync() {
            if (this._testSecrets?.LdapOptions != null) {
                var service = this._services.GetService<ILdapAuthenticationService<LdapUser>>();
                Assert.IsNotNull(service);

                Assert.ThrowsExceptionAsync<LdapException>(async () => {
                    await service.LoginUserAsync(
                        this._testSecrets.LdapOptions.User!,
                        this._testSecrets.LdapOptions.Password! + " is wrong");
                });
            }
        }

        [TestMethod]
        public void TestLoginUserWithClaims() {
            if (this._testSecrets?.LdapOptions != null) {
                var service = this._services.GetService<ILdapAuthenticationService<LdapUser>>();
                Assert.IsNotNull(service);

                (var user, var claims) = service.LoginUser(
                    this._testSecrets.LdapOptions.User!,
                    this._testSecrets.LdapOptions.Password!,
                    null);
                Assert.IsNotNull(user);
                Assert.IsNotNull(claims);
                Assert.IsTrue(claims.Any());
            }
        }

        [TestMethod]
        public async Task TestLoginUserWithClaimsAsync() {
            if (this._testSecrets?.LdapOptions != null) {
                var service = this._services.GetService<ILdapAuthenticationService<LdapUser>>();
                Assert.IsNotNull(service);

                (var user, var claims) = await service.LoginUserAsync(
                    this._testSecrets.LdapOptions.User!,
                    this._testSecrets.LdapOptions.Password!,
                    null);
                Assert.IsNotNull(user);
                Assert.IsNotNull(claims);
                Assert.IsTrue(claims.Any());
            }
        }

        private readonly ServiceProvider _services;
        private readonly TestSecrets _testSecrets = TestExtensions.CreateSecrets();
    }
}
