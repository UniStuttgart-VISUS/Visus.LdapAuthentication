// <copyright file="LdapUserExtensionsTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2022 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;


namespace Visus.LdapAuthentication.Tests {

    /// <summary>
    /// Tests <see cref="LdapUserExtensions"/>.
    /// </summary>
    [TestClass]
    public sealed class LdapUserExtensionsTest {

        #region Nested class TestUser
        private class TestUser : LdapUserBase {
            public override IEnumerable<Claim> Claims {
                get {
                    yield return new Claim(ClaimTypes.Name, "Max Mustermann");
                    yield return new Claim(ClaimTypes.GroupSid, "1");
                    yield return new Claim(ClaimTypes.GroupSid, "2");
                    yield return new Claim(ClaimTypes.GroupSid, "3");
                    yield return new Claim(ClaimTypes.GroupSid, "4");
                }
            }
        }
        #endregion

        [TestMethod]
        public void TestToClaimsIdentity() {
            var user = new TestUser();

            {
                var identity = LdapUserExtensions.ToClaimsIdentity(null);
                Assert.IsNull(identity);
            }

            {
                var identity = user.ToClaimsIdentity();
                Assert.IsNotNull(identity);
                Assert.AreEqual(5, identity.Claims.Count());
                Assert.IsTrue(identity.Claims.Any(c => c.Type == ClaimTypes.Name));
                Assert.IsTrue(identity.Claims.Any(c => c.Type == ClaimTypes.GroupSid));
                Assert.IsTrue(identity.Claims.Any(c => c.Value == "1"));
                Assert.IsTrue(identity.Claims.Any(c => c.Value == "2"));
                Assert.IsTrue(identity.Claims.Any(c => c.Value == "3"));
                Assert.IsTrue(identity.Claims.Any(c => c.Value == "4"));
            }

            {
                var identity = user.ToClaimsIdentity(c => c.Type != ClaimTypes.GroupSid);
                Assert.IsNotNull(identity);
                Assert.AreEqual(1, identity.Claims.Count());
                Assert.IsTrue(identity.Claims.Any(c => c.Type == ClaimTypes.Name));
                Assert.IsFalse(identity.Claims.Any(c => c.Type == ClaimTypes.GroupSid));
            }
        }

        [TestMethod]
        public void TestCustomAuthenticationType() {
            var user = new TestUser();

            {
                var identity = user.ToClaimsIdentity("Test");
                Assert.IsNotNull(identity);
                Assert.AreEqual(5, identity.Claims.Count());
                Assert.IsTrue(identity.Claims.Any(c => c.Type == ClaimTypes.Name));
                Assert.IsTrue(identity.Claims.Any(c => c.Type == ClaimTypes.GroupSid));
                Assert.IsTrue(identity.Claims.Any(c => c.Value == "1"));
                Assert.IsTrue(identity.Claims.Any(c => c.Value == "2"));
                Assert.IsTrue(identity.Claims.Any(c => c.Value == "3"));
                Assert.IsTrue(identity.Claims.Any(c => c.Value == "4"));
                Assert.AreEqual("Test", identity.AuthenticationType);
            }
        }
    }
}
