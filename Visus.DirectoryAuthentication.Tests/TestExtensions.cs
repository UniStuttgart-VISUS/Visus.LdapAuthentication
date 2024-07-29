// <copyright file="TestExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Visus.DirectoryAuthentication.Claims;
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryAuthentication.Services;
using Visus.Ldap;
using Visus.Ldap.Claims;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Tests {

    /// <summary>
    /// Provides extension methods for injecting test-related stuff to the DI
    /// container etc.
    /// </summary>
    internal static class TestExtensions {

        public static IConfigurationRoot CreateConfiguration() {
            return new ConfigurationBuilder()
                .AddTestSecrets()
                .Build();
        }

        public static TestSecrets CreateSecrets() {
            var retval = new TestSecrets();
            CreateConfiguration().Bind(retval);
            return retval;
        }

        public static IConfigurationBuilder AddTestSecrets(
                this IConfigurationBuilder builder)
            => builder.AddUserSecrets<TestSecrets>();

        public static IServiceCollection AddMockLoggers(
                this IServiceCollection services) {
            services.AddSingleton(s => Mock.Of<ILogger<ClaimsBuilderBase<LdapUser, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<ClaimsMapper>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapConnectionService>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapAuthenticationService<LdapUser, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapSearchService<LdapUser, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapMapper<LdapUser, LdapGroup>>>());
            return services;
        }

        public static IServiceCollection AddDefaultLdapAuthentication(
                this IServiceCollection services, Action<LdapOptions> options) {
            return services.AddLdapAuthentication<LdapUser,
                LdapGroup,
                LdapMapper<LdapUser, LdapGroup>,
                LdapAttributeMap<LdapUser, LdapOptions>,
                LdapAttributeMap<LdapGroup, LdapOptions>,
                ClaimsBuilder<LdapUser, LdapGroup>,
                ClaimsMapper,
                ClaimsMap<LdapUser, LdapOptions>,
                ClaimsMap<LdapGroup, LdapOptions>>(options);
        }
    }
}
