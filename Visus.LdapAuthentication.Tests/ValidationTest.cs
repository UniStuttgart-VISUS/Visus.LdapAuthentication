// <copyright file="ValidationTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Visus.Ldap.Mapping;
using Visus.LdapAuthentication.Configuration;


namespace Visus.LdapAuthentication.Tests {

    /// <summary>
    /// Tests validators provided by the library.
    /// </summary>
    [TestClass]
    public sealed class ValidationTest {

        [TestMethod]
        public void TestLdapMappingValidator() {
            var validator = new LdapMappingValidator();

            {
                var mapping = new LdapMapping() {
                    DistinguishedNameAttribute = null
                };
                var result = validator.Validate(mapping);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapMapping.DistinguishedNameAttribute)));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapMapping.GroupsAttribute)));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapMapping.PrimaryGroupAttribute)));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapMapping.PrimaryGroupIdentityAttribute)));
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
                options.Mapping = new() {
                    DistinguishedNameAttribute = null
                };
                options.Servers = Array.Empty<string>();
                var result = validator.Validate(options);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == $"{nameof(LdapOptions.Mapping)}.{nameof(LdapMapping.DistinguishedNameAttribute)}"));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == $"{nameof(LdapOptions.Mapping)}.{nameof(LdapMapping.GroupsAttribute)}"));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == $"{nameof(LdapOptions.Mapping)}.{nameof(LdapMapping.PrimaryGroupAttribute)}"));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == $"{nameof(LdapOptions.Mapping)}.{nameof(LdapMapping.UserFilter)}"));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == $"{nameof(LdapOptions.Mapping)}.{nameof(LdapMapping.UsersFilter)}"));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapOptions.Schema)));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == nameof(LdapOptions.SearchBases)));
                Assert.IsTrue(result.Errors.Any(r => r.PropertyName == $"{nameof(LdapOptions.Servers)}"));
            }

            {
                var options = new LdapOptions {
                    Schema = Schema.ActiveDirectory,
                    Servers = ["127.0.0.1"],
                    SearchBases = new Dictionary<string, SearchScope> {
                        { "DC=visus,DC=uni-stuttgart,DC=de", SearchScope.Base }
                    }
                };
                var result = validator.Validate(options);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.IsValid);
            }
        }

    }
}
