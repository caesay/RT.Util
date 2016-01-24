using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RT.Classify
{
    internal static class Util
    {
        public static int Utf8Length(this string input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            return Encoding.UTF8.GetByteCount(input);
        }
        public static int Utf16Length(this string input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            return Encoding.Unicode.GetByteCount(input);
        }
        public static byte[] ToUtf8(this string input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            return Encoding.UTF8.GetBytes(input);
        }
        public static byte[] ToUtf16(this string input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            return Encoding.Unicode.GetBytes(input);
        }
        public static string FromUtf8(this byte[] input, bool removeBom = false)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            var result = Encoding.UTF8.GetString(input);
            if (removeBom && result.StartsWith("\ufeff"))
                return result.Substring(1);
            return result;
        }
        public static string FromUtf16(this byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            return Encoding.Unicode.GetString(input);
        }
        public static bool TryGetInterfaceGenericParameters(this Type type, Type @interface, out Type[] typeParameters)
        {
            typeParameters = null;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == @interface)
            {
                typeParameters = type.GetGenericArguments();
                return true;
            }

            var implements = type.FindInterfaces((ty, obj) => ty.IsGenericType && ty.GetGenericTypeDefinition() == @interface, null).FirstOrDefault();
            if (implements == null)
                return false;

            typeParameters = implements.GetGenericArguments();
            return true;
        }
        public static object GetDefaultValue(this Type type)
        {
            if (!type.IsValueType)
                return null;
            // This works for nullables too
            return Activator.CreateInstance(type);
        }
        public static IEnumerable<FieldInfo> GetAllFields(this Type type)
        {
            IEnumerable<FieldInfo> fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var baseType = type.BaseType;
            return (baseType == null) ? fields : GetAllFields(baseType).Concat(fields);
        }
        public static IEnumerable<PropertyInfo> GetAllProperties(this Type type)
        {
            IEnumerable<PropertyInfo> properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var baseType = type.BaseType;
            return (baseType == null) ? properties : GetAllProperties(baseType).Concat(properties);
        }
        public static string CLiteralEscape(this string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            var result = new StringBuilder(value.Length + value.Length / 2);

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '\0': result.Append(@"\0"); break;
                    case '\a': result.Append(@"\a"); break;
                    case '\b': result.Append(@"\b"); break;
                    case '\t': result.Append(@"\t"); break;
                    case '\n': result.Append(@"\n"); break;
                    case '\v': result.Append(@"\v"); break;
                    case '\f': result.Append(@"\f"); break;
                    case '\r': result.Append(@"\r"); break;
                    case '\\': result.Append(@"\\"); break;
                    case '"': result.Append(@"\"""); break;
                    default:
                        if (c >= 0xD800 && c < 0xDC00)
                        {
                            if (i == value.Length - 1) // string ends on a broken surrogate pair
                                result.AppendFormat(@"\u{0:X4}", (int)c);
                            else
                            {
                                char c2 = value[i + 1];
                                if (c2 >= 0xDC00 && c2 <= 0xDFFF)
                                {
                                    // nothing wrong with this surrogate pair
                                    i++;
                                    result.Append(c);
                                    result.Append(c2);
                                }
                                else // first half of a surrogate pair is not followed by a second half
                                    result.AppendFormat(@"\u{0:X4}", (int)c);
                            }
                        }
                        else if (c >= 0xDC00 && c <= 0xDFFF) // the second half of a broken surrogate pair
                            result.AppendFormat(@"\u{0:X4}", (int)c);
                        else if (c >= ' ')
                            result.Append(c);
                        else // the character is in the 0..31 range
                            result.AppendFormat(@"\u{0:X4}", (int)c);
                        break;
                }
            }

            return result.ToString();
        }
        public static string CLiteralUnescape(this string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            var result = new StringBuilder(value.Length);

            int i = 0;
            while (i < value.Length)
            {
                char c = value[i];
                if (c != '\\')
                    result.Append(c);
                else
                {
                    if (i + 1 >= value.Length)
                        throw new ArgumentException($"String ends before the escape sequence at position {i} is complete", "value");
                    i++;
                    c = value[i];
                    int code;
                    switch (c)
                    {
                        case '0': result.Append('\0'); break;
                        case 'a': result.Append('\a'); break;
                        case 'b': result.Append('\b'); break;
                        case 't': result.Append('\t'); break;
                        case 'n': result.Append('\n'); break;
                        case 'v': result.Append('\v'); break;
                        case 'f': result.Append('\f'); break;
                        case 'r': result.Append('\r'); break;
                        case '\\': result.Append('\\'); break;
                        case '"': result.Append('"'); break;
                        case 'x':
                            // See how many characters are hex digits
                            var len = 0;
                            i++;
                            while (len <= 4 && i + len < value.Length && ((value[i + len] >= '0' && value[i + len] <= '9') || (value[i + len] >= 'a' && value[i + len] <= 'f') || (value[i + len] >= 'A' && value[i + len] <= 'F')))
                                len++;
                            if (len == 0)
                                throw new ArgumentException($"Invalid hex escape sequence \"\\x\" at position {i - 2}", "value");
                            code = int.Parse(value.Substring(i, len), NumberStyles.AllowHexSpecifier);
                            result.Append((char)code);
                            i += len - 1;
                            break;
                        case 'u':
                            if (i + 4 >= value.Length)
                                throw new ArgumentException($"Invalid hex escape sequence \"\\u\" at position {i}", "value");
                            i++;
                            code = int.Parse(value.Substring(i, 4), NumberStyles.AllowHexSpecifier);
                            result.Append((char)code);
                            i += 3;
                            break;
                        default:
                            throw new ArgumentException($"Unrecognised escape sequence at position {i - 1}: \\{c}", "value");
                    }
                }

                i++;
            }

            return result.ToString();
        }
    }
}
