// <copyright file="SidConverterTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Tests {

    /// <summary>
    /// Tests <see cref="SidConverter"/>.
    /// </summary>
    [TestClass]
    public sealed class SidConverterTest {

        [TestMethod]
        public void TestConversion() {
            {
                var input = new byte[] { 0x01, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x15, 0x00, 0x00, 0x00, 0x48, 0x9F, 0x38, 0x51, 0x95, 0x5D, 0x05, 0xD8, 0x90, 0x81, 0x69, 0x0F, 0xDF, 0x05, 0x00, 0x00 };
                var expected = "S-1-5-21-1362665288-3624230293-258572688-1503";
                var actual = SidConverter.Convert(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestErrorHandling() {
            {
                var input = (byte[]) null;
                var expected = (string) null;
                var actual = SidConverter.Convert(input);
                Assert.AreEqual(expected, actual);
            }

            {
                var input = new byte[0];
                Assert.ThrowsException<ArgumentException>(() => SidConverter.Convert(input));
            }

            {
                var input = new byte[1];
                Assert.ThrowsException<ArgumentException>(() => SidConverter.Convert(input));
            }

            {
                var input = new byte[2];
                Assert.ThrowsException<ArgumentException>(() => SidConverter.Convert(input));
            }
        }
    }
}
