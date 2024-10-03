// <copyright file="LdapCaching.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


namespace Visus.Ldap.Configuration {

    /// <summary>
    /// Specifies possible caching options for LDAP object.
    /// </summary>
    public enum LdapCaching {

        /// <summary>
        /// Do not cache objects, but retrieve them every time from the
        /// directory.
        /// </summary>
        None = 0,

        /// <summary>
        /// Cache objects for a fixed amount of time.
        /// </summary>
        FixedExpiration,

        /// <summary>
        /// Cache objects with a sliding expiration window, i.e. objects are
        /// only evicted from cache when they are no longer used.
        /// </summary>
        /// <remarks>
        /// Using this option is dangerous if groups are used to assign
        /// permissions to users, because the removal of hierarchical group
        /// memberships in the directory may never be propagated to the
        /// application is the groups in questions are constantly used. If this
        /// option is used, you should restart applications after changing groups
        /// in the directory to make sure that permission that have been revoked
        /// are seen by the application.
        /// </remarks>
        SlidingExpiration
    }
}
