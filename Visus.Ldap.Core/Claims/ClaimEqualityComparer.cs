﻿// <copyright file="ClaimEqualityComparer.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;


namespace Visus.Ldap.Claims {

    /// <summary>
    /// Implements an <see cref="IEqualityComparer{T}"/> for claims, which
    /// enables making distinct enumerations of claims based on the
    /// <see cref="Claim.Type"/> and <see cref="Claim.Value"/>.
    /// </summary>
    /// <remarks>
    /// Note that the implementation does not look at any other properties than
    /// <see cref="Claim.Type"/> and <see cref="Claim.Value"/>, wherefore it
    /// might not yield the expected results if used for any other purpose than
    /// making distinct lists of claims generated by this library, which only
    /// set these two properties.
    /// </remarks>
    public class ClaimEqualityComparer : IEqualityComparer<Claim> {

        /// <summary>
        /// The default instance of this class.
        /// </summary>
        public static readonly ClaimEqualityComparer Instance = new();

        /// <inheritdoc />
        public bool Equals(Claim? lhs, Claim? rhs) {
            if ((lhs == null) && (rhs == null)) {
                return true;

            } else if ((lhs == null) != (rhs == null)) {
                return false;
            }
            Debug.Assert(lhs != null);
            Debug.Assert(rhs != null);

            return ((lhs.Type == rhs.Type)
                && (lhs.Value == rhs.Value));

        }

        /// <inheritdoc />
        public int GetHashCode([DisallowNull] Claim obj) 
            => obj.Type.GetHashCode() ^ obj.Value.GetHashCode();
    }
}