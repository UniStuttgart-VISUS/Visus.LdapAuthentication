// <copyright file="ClaimAttributeTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Visus.Ldap.Claims;


namespace Visus.LdapAuthentication.Tests {

    /// <summary>
    /// Test for <see cref="ClaimAttribute"/>.
    /// </summary>
    [TestClass]
    public sealed class ClaimAttributeTest {

        #region Nested class TestClass1
        private sealed class TestClass1 {

            public string Property1 { get; set; }

            [Claim("claim1")]
            public string Property2 { get; set; }

            [Claim("claim2")]
            [Claim("claim3")]
            public string Property3 { get; set; }
        }
        #endregion

        [TestMethod]
        public void TestNoClaims() {
            {
                var pi = typeof(TestClass1).GetProperty(nameof(TestClass1.Property1));
                var claims = ClaimAttribute.GetClaims(pi);
                Assert.IsNotNull(claims);
                Assert.IsFalse(claims.Any());
            }

            {
                var claims = ClaimAttribute.GetClaims(typeof(TestClass1), nameof(TestClass1.Property1));
                Assert.IsNotNull(claims);
                Assert.IsFalse(claims.Any());
            }

            {
                var claims = ClaimAttribute.GetClaims<TestClass1>(nameof(TestClass1.Property1));
                Assert.IsNotNull(claims);
                Assert.IsFalse(claims.Any());
            }
        }

        [TestMethod]
        public void TestOneClaim() {
            {
                var pi = typeof(TestClass1).GetProperty(nameof(TestClass1.Property2));
                var claims = ClaimAttribute.GetClaims(pi);
                Assert.IsNotNull(claims);
                Assert.AreEqual(1, claims.Count());
            }

            {
                var claims = ClaimAttribute.GetClaims(typeof(TestClass1), nameof(TestClass1.Property2));
                Assert.IsNotNull(claims);
                Assert.AreEqual(1, claims.Count());
            }

            {
                var claims = ClaimAttribute.GetClaims<TestClass1>(nameof(TestClass1.Property2));
                Assert.IsNotNull(claims);
                Assert.AreEqual(1, claims.Count());
            }
        }

        [TestMethod]
        public void TestMultipleClaims() {
            {
                var pi = typeof(TestClass1).GetProperty(nameof(TestClass1.Property3));
                var claims = ClaimAttribute.GetClaims(pi);
                Assert.IsNotNull(claims);
                Assert.AreEqual(2, claims.Count());
            }

            {
                var claims = ClaimAttribute.GetClaims(typeof(TestClass1), nameof(TestClass1.Property3));
                Assert.IsNotNull(claims);
                Assert.AreEqual(2, claims.Count());
            }

            {
                var claims = ClaimAttribute.GetClaims<TestClass1>(nameof(TestClass1.Property3));
                Assert.IsNotNull(claims);
                Assert.AreEqual(2, claims.Count());
            }
        }
    }
}