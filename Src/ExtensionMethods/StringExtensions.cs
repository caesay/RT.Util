﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Reflection;

namespace RT.Util.ExtensionMethods
{
    /// <summary>
    /// Provides extension methods on the <see langword="string"/> type.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Concatenates the specified number of repetitions of the current string.
        /// </summary>
        /// <param name="Input">The string to be repeated.</param>
        /// <param name="NumTimes">The number of times to repeat the string.</param>
        /// <returns>A concatenated string containing the original string the specified number of times.</returns>
        public static string Repeat(this string Input, int NumTimes)
        {
            if (NumTimes == 0) return "";
            if (NumTimes == 1) return Input;
            if (NumTimes == 2) return Input + Input;
            var sb = new StringBuilder();
            for (int i = 0; i < NumTimes; i++)
                sb.Append(Input);
            return sb.ToString();
        }

        /// <summary>
        /// Escapes all necessary characters in the specified string so as to make it usable safely in an HTML or XML context.
        /// </summary>
        /// <param name="Input">The string to apply HTML or XML escaping to.</param>
        /// <returns>The specified string with the necessary HTML or XML escaping applied.</returns>
        public static string HTMLEscape(this string Input)
        {
            return Input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&#39;").Replace("\"", "&quot;");
        }

        /// <summary>
        /// Returns the set of characters allowed in a URL.
        /// </summary>
        public static string URLAllowedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789$-_.!*'(),/:;@";

        /// <summary>
        /// Escapes all necessary characters in the specified string so as to make it usable safely in a URL.
        /// </summary>
        /// <param name="Input">The string to apply URL escaping to.</param>
        /// <returns>The specified string with the necessary URL escaping applied.</returns>
        public static string URLEscape(this string Input)
        {
            byte[] UTF8 = Input.ToUTF8();
            StringBuilder sb = new StringBuilder();
            foreach (byte b in UTF8)
                if (URLAllowedCharacters.Contains((char) b))
                    sb.Append((char) b);
                else
                    sb.Append(string.Format("%{0:X2}", b));
            return sb.ToString();
        }

        /// <summary>
        /// Reverses the escaping performed by <see cref="URLEscape"/> by decoding hexadecimal URL escape sequences into their original characters.
        /// </summary>
        /// <param name="Input">String containing URL escape sequences to be decoded.</param>
        /// <returns>The specified string with all URL escape sequences decoded.</returns>
        public static string URLUnescape(this string Input)
        {
            if (Input.Length < 3)
                return Input;

            int BufferSize = Input.Length;
            for (int i = 0; i < Input.Length; i++)
                if (Input[i] == '%') { BufferSize -= 2; }

            byte[] Buffer = new byte[BufferSize];

            BufferSize = 0;
            int j = 0;
            while (j < Input.Length)
            {
                if (Input[j] == '%')
                {
                    try
                    {
                        Buffer[BufferSize] = byte.Parse("" + Input[j + 1] + Input[j + 2], NumberStyles.HexNumber);
                        BufferSize++;
                    }
                    catch (Exception) { }
                    j += 3;
                }
                else
                {
                    Buffer[BufferSize] = Input[j] == '+' ? (byte) ' ' : (byte) Input[j];
                    BufferSize++;
                    j++;
                }
            }
            return Encoding.UTF8.GetString(Buffer, 0, BufferSize);
        }

        /// <summary>
        /// Converts the specified string to UTF-8.
        /// </summary>
        /// <param name="Input">String to convert to UTF-8.</param>
        /// <returns>The specified string, converted to a byte-array containing the UTF-8 encoding of the string.</returns>
        public static byte[] ToUTF8(this string Input)
        {
            return Encoding.UTF8.GetBytes(Input);
        }

        /// <summary>
        /// Determines the length of the UTF-8 encoding of the specified string.
        /// </summary>
        /// <param name="Input">String to determined UTF-8 length of.</param>
        /// <returns>The length of the string in bytes when encoded as UTF-8.</returns>
        public static int UTF8Length(this string Input)
        {
            return Encoding.UTF8.GetByteCount(Input);
        }

        /// <summary>
        /// Converts the specified string to a byte array. Non-ASCII characters are replaced with question marks ('?').
        /// </summary>
        /// <param name="Input">String to convert to a byte array.</param>
        /// <returns>The specified string, converted to a byte-array with non-ASCII characters replaced with question marks ('?').</returns>
        public static byte[] ToASCII(this string Input)
        {
            return Encoding.ASCII.GetBytes(Input);
        }

        /// <summary>
        /// Returns a JavaScript-compatible representation of the string in double-quotes with the appropriate characters escaped.
        /// </summary>
        /// <param name="Input">String to escape.</param>
        /// <returns>JavaScript-compatible representation of the input string.</returns>
        public static string JSEscape(this string Input)
        {
            return "\"" + Input.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"";
        }

        /// <summary>Examines the type T for a static field with the specified name and returns the
        /// value of that static field. If T is an enum, returns the enum value with the specified name.</summary>
        /// <example>
        ///     <code>
        ///         public enum X { One, Two, Three }
        ///         public class Y { public static string Constant = "C#"; }
        ///         // ...
        ///         Console.WriteLine("One".ToStaticValue&lt;X&gt;());         // outputs "One"
        ///         Console.WriteLine("Constant".ToStaticValue&lt;Y&gt;());    // outputs "C#"
        ///     </code>
        /// </example>
        /// <typeparam name="T">The type to examine. Can be a class, struct, or enum.</typeparam>
        /// <param name="Input">A string specifying the field name to search for. In the case of enums, the name of the enum value.</param>
        /// <returns>The value of the specified field. In the case of enums, this is the enum value.</returns>
        public static object ToStaticValue<T>(this string Input)
        {
            return ToStaticValue(Input, typeof(T));
        }

        /// <summary>Examines the type InputType for a static field with the specified name and returns the
        /// value of that static field. If InputType is an enum, returns the enum value with the specified name.</summary>
        /// <example>
        ///     <code>
        ///         public enum X { One, Two, Three }
        ///         public class Y { public static string Constant = "C#"; }
        ///         // ...
        ///         Console.WriteLine("One".ToStaticValue(typeof(X)));         // outputs "One"
        ///         Console.WriteLine("Constant".ToStaticValue(typeof(Y)));    // outputs "C#"
        ///     </code>
        /// </example>
        /// <param name="Input">A string specifying the field name to search for. In the case of enums, the name of the enum value.</param>
        /// <param name="InputType">The type to examine. Can be a class, struct, or enum.</param>
        /// <returns>The value of the specified field. In the case of enums, this is the enum value.</returns>
        public static object ToStaticValue(this string Input, Type InputType)
        {
            foreach (var Field in InputType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (Field.Name == Input)
                    return Field.GetValue(null);
            }
            throw new Exception(string.Format("The type {0} did not contain a field called {1}.", InputType, Input));
        }

        /// <summary>
        /// Joins all strings in <see pref="values"/> using the string as the separator.
        /// <example>
        ///     <code>
        ///         var a = ", ".Join(new[] { 'Paris', 'London', 'Tokyo' });
        ///         // a contains "Paris, London, Tokyo"
        ///     </code>
        /// </example>
        /// </summary>
        public static string Join(this string separator, IEnumerable<string> values)
        {
            return separator.Join(values.GetEnumerator());
        }

        /// <summary>
        /// Joins all strings in <see pref="values"/> using the string as the separator.
        /// <example>
        ///     <code>
        ///         var a = ", ".Join(new[] { 'Paris', 'London', 'Tokyo' });
        ///         // a contains "Paris, London, Tokyo"
        ///     </code>
        /// </example>
        /// </summary>
        public static string Join(this string separator, IEnumerator<string> values)
        {
            StringBuilder SB = new StringBuilder();
            if (values.MoveNext())
                SB.Append(values.Current);
            while (values.MoveNext())
            {
                SB.Append(separator);
                SB.Append(values.Current);
            }
            return SB.ToString();
        }

        /// <summary>
        /// Joins all strings in the enumerable using the specified string as the separator.
        /// <example>
        ///     <code>
        ///         var a = (new[] { 'Paris', 'London', 'Tokyo' }).Join(", ");
        ///         // a contains "Paris, London, Tokyo"
        ///     </code>
        /// </example>
        /// </summary>
        public static string Join(this IEnumerable<string> values, string separator)
        {
            return separator.Join(values);
        }
    }
}
