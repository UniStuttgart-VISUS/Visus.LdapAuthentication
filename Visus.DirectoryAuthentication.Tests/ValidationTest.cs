// <copyright file="ValidationTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;


namespace Visus.DirectoryAuthentication.Tests {

    /// <summary>
    /// Tests validators provided by the library.
    /// </summary>
    [TestClass]
    public sealed class ValidationTest {

        [TestMethod]
        public void TestLdapMappingValidator() {
            var validator = new LdapMappingValidator();

            {
                var mapping = new LdapMapping();
                var result = validator.Validate(mapping);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapMapping.DistinguishedNameAttribute)));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapMapping.GroupsAttribute)));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapMapping.PrimaryGroupAttribute)));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapMapping.UserFilter)));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapMapping.UsersFilter)));
            }

            {
                var mapping = new LdapOptions().Mappings.First().Value;
                var result = validator.Validate(mapping);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.IsValid);
            }
        }

        [TestMethod]
        public void TestLdapOptionsValidator() {
            var validator = new LdapOptionsValidator();

            {
                var options = new LdapOptions();
                options.Mappings = null;
                var result = validator.Validate(options);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapOptions.Mapping)));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapOptions.Mappings)));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapOptions.Schema)));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapOptions.SearchBases)));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapOptions.Servers)));
            }

            {
                var options = new LdapOptions();
                options.Mapping = new();
                options.Servers = new[] { new LdapServer() };
                var result = validator.Validate(options);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == $"{nameof(LdapOptions.Mapping)}.{nameof(LdapMapping.DistinguishedNameAttribute)}"));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == $"{nameof(LdapOptions.Mapping)}.{nameof(LdapMapping.GroupsAttribute)}"));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == $"{nameof(LdapOptions.Mapping)}.{nameof(LdapMapping.PrimaryGroupAttribute)}"));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == $"{nameof(LdapOptions.Mapping)}.{nameof(LdapMapping.UserFilter)}"));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == $"{nameof(LdapOptions.Mapping)}.{nameof(LdapMapping.UsersFilter)}"));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapOptions.Schema)));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapOptions.SearchBases)));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == $"{nameof(LdapOptions.Servers)}[0].{nameof(LdapServer.Address)}"));
            }

            {
                var options = new LdapOptions() {
                    Schema = Schema.ActiveDirectory
                };
                options.Servers = new[] {
                    new LdapServer() {
                        Address = "127.0.0.1"
                    }
                };
                options.SearchBases = new Dictionary<string, SearchScope> {
                    { "DC=visus,DC=uni-stuttgart,DC=de", SearchScope.Base }
                };
                var result = validator.Validate(options);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.IsValid);
            }
        }

        [TestMethod]
        public void TestLdapServerValidator() {
            var validator = new LdapServerValidator();

            {
                var server = new LdapServer();
                var result = validator.Validate(server);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapServer.Address)));
            }

            {
                var server = new LdapServer() {
                    Address = string.Empty
                };
                var result = validator.Validate(server);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapServer.Address)));
            }

            {
                var server = new LdapServer() {
                    Address = "127.0.0.1"
                };
                var result = validator.Validate(server);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.IsValid);
            }
        }
    }
}
