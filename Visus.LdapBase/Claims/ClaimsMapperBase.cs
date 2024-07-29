// <copyright file="ClaimsMapperBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using Visus.Ldap.Mapping;


namespace Visus.Ldap.Claims {

    /// <summary>
    /// The basic implementation of <see cref="IClaimsMapper{TEntry}"/>, which
    /// requires subclasses to provide and implementation of
    /// <see cref="GetAttribute(TEntry, LdapAttributeAttribute)"/>
    /// </summary>
    /// <typeparam name="TEntry">The type of the LDAP entry, which is dependent
    /// on the underlying library.</typeparam>
    public abstract class ClaimsMapperBase<TEntry> : IClaimsMapper<TEntry> {

        #region Public properties
        /// <inheritdoc />
        public IEnumerable<string> RequiredGroupAttributes
            => this._groupMap.AttributeNames;

        /// <inheritdoc />
        public IEnumerable<string> RequiredUserAttributes
            => this._userMap.AttributeNames;
        #endregion

        #region Public methods
        /// <inheritdoc />
        public IEnumerable<Claim> GetClaims(TEntry user,
                TEntry? primaryGroup,
                IEnumerable<TEntry> groups,
                ClaimFilter? filter = null) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            ArgumentNullException.ThrowIfNull(groups, nameof(groups));

            // Add claims from the user entry itself.
            foreach (var c in this.GetClaims(user, this._userMap, filter)) {
                yield return c;
            }

            // Add claims from the primary group.
            if (primaryGroup != null) {
                foreach (var c in this.GetClaims(primaryGroup, this._groupMap,
                        filter)) {
                    yield return c;
                }
            }

            // Add claims for the remaining groups.
            foreach (var g in groups) {
                foreach (var c in this.GetClaims(g, this._groupMap, filter)) {
                    yield return c;
                }
            }
        }
        #endregion

        #region Protected constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="userMap">The mapping from LDAP attributes to claims
        /// for user entries.</param>
        /// <param name="groupMap">The mapping from LDAP attributes to claims
        /// for group entries.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="userMap"/> is <c>null</c>, or if
        /// <paramref name="groupMap"/> is <c>null</c>.</exception>
        protected ClaimsMapperBase(IUserClaimsMap userMap,
                IGroupClaimsMap groupMap) {
            this._groupMap = groupMap
                ?? throw new ArgumentNullException(nameof(groupMap));
            this._userMap = userMap
                ?? throw new ArgumentNullException(nameof(userMap));
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Gets the value of the specified LDAP <paramref name="attribute"/>
        /// from the given entry.
        /// </summary>
        /// <param name="entry">The entry to retrieve the attribute from.
        /// </param>
        /// <param name="attribute">Describes the attribute to retrieve,
        /// in particula its name and a potenial <see cref="IValueConverter"/>
        /// that should be used.</param>
        /// <returns>The value of the attribute or <c>null</c> if the attribute
        /// does not exist.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="entry"/>
        /// is <c>null</c>, or if <paramref name="attribute"/> is <c>null</c>.
        /// </exception>
        protected abstract object? GetAttribute(TEntry entry,
            LdapAttributeAttribute attribute);
        #endregion

        #region Private methods
        /// <summary>
        /// Gets the claims in the given <paramref name="map"/> for the
        /// given <paramref name="entry"/>.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        private IEnumerable<Claim> GetClaims(TEntry entry,
                IClaimsMap map,
                ClaimFilter? filter) {
            Debug.Assert(entry != null);
            Debug.Assert(map != null);

            foreach (var a in map) {
                var value = this.GetAttribute(entry, a.Key) as string;

                if (value != null) {
                    foreach (var c in a.Value) {
                        if (filter?.Invoke(c.Name, value) != false) {
                            yield return new(c.Name, value);
                        }
                    }
                }
            }
        }
        #endregion

        #region Private fields
        private readonly IGroupClaimsMap _groupMap;
        private readonly IUserClaimsMap _userMap;
        #endregion
    }
}
