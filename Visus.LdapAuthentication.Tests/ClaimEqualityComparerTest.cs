// <copyright file="ClaimEqualityComparerTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Claims;
using Visus.LdapAuthentication.Claims;


namespace Visus.LdapAuthentication.Tests {

    /// <summary>
    /// Test for <see cref="ClaimEqualityComparer"/>.
    /// </summary>
    [TestClass]
    public sealed class ClaimEqualityComparerTest {

        [TestMethod]
        public void TestEquals() {
            {
                var claim1 = new Claim(ClaimTypes.WindowsAccountName, "hugo");
                var claim2 = new Claim(ClaimTypes.WindowsAccountName, "hugo");
                Assert.IsTrue(ClaimEqualityComparer.Instance.Equals(claim1, claim2));
            }

            {
                var claim1 = new Claim(ClaimTypes.WindowsAccountName, "hugo");
                var claim2 = new Claim(ClaimTypes.WindowsAccountName, "horst");
                Assert.IsFalse(ClaimEqualityComparer.Instance.Equals(claim1, claim2));
            }

            {
                var claim1 = new Claim(ClaimTypes.WindowsAccountName, "hugo");
                var claim2 = new Claim(ClaimTypes.GivenName, "hugo");
                Assert.IsFalse(ClaimEqualityComparer.Instance.Equals(claim1, claim2));
            }
        }

        [TestMethod]
        public void TestGetHashCode() {
            {
                var claim1 = new Claim(ClaimTypes.WindowsAccountName, "hugo");
                var claim2 = new Claim(ClaimTypes.WindowsAccountName, "hugo");
                Assert.AreEqual(ClaimEqualityComparer.Instance.GetHashCode(claim1),
                    ClaimEqualityComparer.Instance.GetHashCode(claim2));
            }

            {
                var claim1 = new Claim(ClaimTypes.WindowsAccountName, "hugo");
                var claim2 = new Claim(ClaimTypes.WindowsAccountName, "horst");
                Assert.AreNotEqual(ClaimEqualityComparer.Instance.GetHashCode(claim1),
                    ClaimEqualityComparer.Instance.GetHashCode(claim2));
            }

            {
                var claim1 = new Claim(ClaimTypes.WindowsAccountName, "hugo");
                var claim2 = new Claim(ClaimTypes.GivenName, "hugo");
                Assert.AreNotEqual(ClaimEqualityComparer.Instance.GetHashCode(claim1),
                    ClaimEqualityComparer.Instance.GetHashCode(claim2));
            }
        }
    }
}
