// <copyright file="IntConverter.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Visus.Ldap.Properties;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// A converter that parses a <see cref="string"/> or <see cref="byte[]"/>
    /// attribute into a <see cref="sbyte"/>, <see cref="short"/>,
    /// <see cref="int"/> or <see crsef="long"/> or their unsigned variants.
    /// </summary>
    /// <remarks>
    /// This converter is required, because ADDS stores some numbers as strings.
    /// </remarks>
    public sealed class NumberConverter : IValueConverter {

        #region Public methods
        /// <inheritdoc />
        public object? Convert(object? value, Type target, object? parameter,
                CultureInfo culture) {
            ArgumentNullException.ThrowIfNull(target, nameof(target));

            switch (value) {
                case string s: return ConvertTo(s, target, culture);
                case byte[] b: return ConvertTo(b, target);
                case null: return null;
                default: throw new ArgumentNullException(
                        Resources.ErrorInconvertibleNumber,
                        nameof(value));
            }
        }
        #endregion

        #region Private class methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object ConvertTo(byte[] b, Type target)
            => target switch {
                var sb when sb == typeof(sbyte) => (sbyte) b[0],
                var ss when ss == typeof(short) => BitConverter.ToInt16(b),
                var si when si == typeof(int) => BitConverter.ToInt32(b),
                var sl when sl == typeof(long) => BitConverter.ToInt64(b),
                var ub when ub == typeof(byte) => b[0],
                var us when us == typeof(ushort) => BitConverter.ToUInt16(b),
                var ui when ui == typeof(uint) => BitConverter.ToUInt32(b),
                var ul when ul == typeof(ulong) => BitConverter.ToUInt64(b),
                var f when f == typeof(float) => BitConverter.ToSingle(b),
                var d when d == typeof(double) => BitConverter.ToDouble(b),
                var bi when bi == typeof(BigInteger) => new BigInteger(b),
                _ => throw new ArgumentException(
                    Resources.ErrorInvalidNumberTarget,
                    nameof(target))
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object? ConvertTo(string value,
                Type target,
                IFormatProvider formatProvider) {
            var parse = target.GetMethod("Parse",
                BindingFlags.Public | BindingFlags.Static,
                [typeof(string), typeof(IFormatProvider)]);

            if (parse == null) {
                throw new ArgumentNullException(
                    Resources.ErrorMissingNumberParse,
                    nameof(value));
            }

            return parse.Invoke(null, [value, formatProvider]);
        }
        #endregion
    }
}
