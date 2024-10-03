// <copyright file="TestExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Visus.LdapAuthentication.Claims;
using Visus.LdapAuthentication.Mapping;
using Visus.LdapAuthentication.Services;
using Visus.Ldap;
using Visus.Ldap.Claims;
using Novell.Directory.Ldap;
using Visus.Ldap.Services;
using Visus.LdapAuthentication.Configuration;


namespace Visus.LdapAuthentication.Tests {

    /// <summary>
    /// Provides extension methods for injecting test-related stuff to the DI
    /// container etc.
    /// </summary>
    internal static class TestExtensions {

        public static IConfigurationRoot CreateConfiguration() {
            return new ConfigurationBuilder()
                .AddUserSecrets<TestSecrets>()
                .Build();
        }

        public static TestSecrets CreateSecrets() {
            var retval = new TestSecrets();
            CreateConfiguration().Bind(retval);
            return retval;
        }

        public static IServiceCollection AddMockLoggers(
                this IServiceCollection services) {
            services.AddSingleton(s => Mock.Of<ILogger<ClaimsBuilderBase<LdapUser, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<ClaimsBuilderBase<TestUser1, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<ClaimsBuilderBase<TestUser2, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<ClaimsMapper>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapConnectionService>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapAuthenticationService<LdapUser, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapAuthenticationService<TestUser1, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapAuthenticationService<TestUser2, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapSearchService<LdapUser, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapSearchService<TestUser1, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapSearchService<TestUser2, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapMapper<LdapUser, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapMapper<TestUser1, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapMapper<TestUser2, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapCacheService<LdapUser, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapCacheService<TestUser1, LdapGroup>>>());
            services.AddSingleton(s => Mock.Of<ILogger<LdapCacheService<TestUser2, LdapGroup>>>());
            return services;
        }
    }
}
