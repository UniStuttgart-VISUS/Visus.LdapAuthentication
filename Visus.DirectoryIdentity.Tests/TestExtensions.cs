﻿// <copyright file="TestExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Visus.Ldap;
using Visus.Ldap.Claims;
using Visus.DirectoryAuthentication.Claims;
using Visus.DirectoryAuthentication.Services;
using Visus.DirectoryAuthentication.Mapping;
using Microsoft.Extensions.Configuration;


namespace Visus.DirectoryIdentity.Tests {

    /// <summary>
    /// Provides extension methods for injecting test-related stuff to the DI
    /// container etc.
    /// </summary>
    internal static class TestExtensions {

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

        public static IConfigurationRoot CreateConfiguration() {
            return new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();
        }
    }
}
