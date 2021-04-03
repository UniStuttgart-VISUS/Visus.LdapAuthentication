// <copyright file="SidConverter.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;
using System;
using System.Linq;
using System.Text;
using Visus.LdapAuthentication.Properties;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Provides a platform-independent implementation for converting Windows
    /// security identifiers.
    /// </summary>
    public sealed class SidConverter : ILdapAttributeConverter {

        #region Public class methods
        /// <summary>
        /// Converts the raw bytes of a SID into the well-known string
        /// representation.
        /// </summary>
        /// <remarks>
        /// This method manually converts a SID into a string. We cannot use
        /// the built-in features of C# here, because we are running on Linux.
        /// See also https://devblogs.microsoft.com/oldnewthing/20040315-00/?p=40253
        /// and https://docs.microsoft.com/en-us/windows/desktop/secauthz/sid-components.
        /// </remarks>
        /// <param name="sid">The byte representation of the SID. It is safe to
        /// pass <c>null</c>, in which case the result will be <c>null</c> as
        /// well.</param>
        /// <returns>The string representation of the SID.</returns>
        /// <exception cref="ArgumentException">In case <paramref name="sid"/>
        /// is not <c>null</c> and shorter than two bytes.</exception>
        public static string Convert(byte[] sid) {
            if (sid == null) {
                return null;
            }

            if (sid.Length < 2) {
                var msg = Resources.ErrorInvalidSid;
                msg = string.Format(msg, BitConverter.ToString(sid));
                throw new ArgumentException(msg, nameof(sid));
            }

            // Start with the mandatory "S-".
            var sb = new StringBuilder("S-");

            // Continue by adding the revision level.
            sb.AppendFormat("{0}-", sid[0]);

            // Determine the number of sub-authorities.
            var subAuthorities = (int) sid[1];
            if (sid.Length != 2 + 6 + subAuthorities * 4) {
                var msg = Resources.ErrorInvalidSid;
                msg = string.Format(msg, BitConverter.ToString(sid));
                throw new ArgumentException(msg, nameof(sid));
            }

            // Retrieve the authority, which is a 48-bit big-endian number.
            var authority = BitConverter.ToUInt32(
                sid.Skip(2).Take(6).Reverse().ToArray());
            sb.AppendFormat("{0}", authority);

            // Retrieve the sub-authorities, which are 32-bit little-endian
            // numbers.
            for (int i = 0; i < subAuthorities; ++i) {
                sb.AppendFormat("-{0}", BitConverter.ToUInt32(
                    sid, 2 + 6 + i * 4));
            }

            return sb.ToString();
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public object Convert(LdapAttribute attribute, object parameter) {
            _ = attribute ?? throw new ArgumentNullException(nameof(attribute));
            return Convert(attribute.ByteValue);
        }
        #endregion
    }
}
