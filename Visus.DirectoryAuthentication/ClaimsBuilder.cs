// <copyright file="ClaimsBuilder.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// The default implementation of
    /// <see cref="IClaimsBuilder{TUser, TGroup}"/> which is based on attribute
    /// annotations of the user and group objects.
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TGroup"></typeparam>
    public sealed class ClaimsBuilder<TUser, TGroup>
            : IClaimsBuilder<TUser, TGroup> {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="mapper"></param>
        public ClaimsBuilder(ILogger<ClaimsBuilder<TUser, TGroup>> logger) {
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));

            this._groupClaims = GetClaims<TGroup>();
            this._userClaims = GetClaims<TUser>();
        }
        #endregion

        /// <inheritdoc />
        public IEnumerable<Claim> Build(TUser user) {
            _ = user ?? throw new ArgumentNullException(nameof(user));

            // Add claims derived from properties of the user.
            foreach (var p in this._userClaims) {
                var v = p.Key.GetValue(user) as string;

                if (v != null) {
                    foreach (var c in p.Value) {
                        this._logger.LogTrace("Adding claim {claim} with "
                            + "value \"{value}\".", c, v);
                        yield return new(c, v);
                    }
                }
            }

            // Add the claims derived from groups.
            var groups = this._mapper.GetGroups(user);
            foreach (var g in groups) {
                foreach (var p in this._groupClaims) {
                    var v = p.Key.GetValue(g) as string;

                    if (v != null) {
                        foreach (var c in p.Value) {
                            this._logger.LogTrace("Adding claim {claim} with "
                                + "value \"{value}\".", c, v);
                            yield return new(c, v);
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public IClaimsBuilder<TUser, TGroup> UseMapper(
                ILdapMapper<TUser, TGroup> mapper) {
            this._mapper = mapper;
            return this;
        }

        #region Private methods
        /// <summary>
        /// Gets the attribute-based claims of the given
        /// <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        private static IDictionary<PropertyInfo, IEnumerable<string>> GetClaims<
                TObject>() {
            var props = from p in typeof(TObject).GetProperties()
                        let a = p.GetCustomAttributes<ClaimAttribute>()
                        where (p.PropertyType == typeof(string)) && (a != null) && a.Any()
                        select new {
                            Property = p,
                            Claims = a.Select(aa => aa.Name)
                        };
            return props.ToDictionary(p => p.Property, p => p.Claims);
        }
        #endregion

        #region Private fields
        private readonly IDictionary<PropertyInfo, IEnumerable<string>> _groupClaims;
        private readonly ILogger _logger;
        private ILdapMapper<TUser, TGroup> _mapper;
        private readonly IDictionary<PropertyInfo, IEnumerable<string>> _userClaims;
        #endregion
    }
}
