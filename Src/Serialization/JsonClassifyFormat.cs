﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RT.Util.ExtensionMethods;
using RT.Util.Json;

namespace RT.Util.Serialization
{
    public static partial class ClassifyFormats
    {
        private class jsonClassifyFormat : IClassifyFormat<JsonValue>
        {
            JsonValue IClassifyFormat<JsonValue>.ReadFromStream(Stream stream)
            {
                return JsonValue.Parse(stream.ReadAllText());
            }

            void IClassifyFormat<JsonValue>.WriteToStream(JsonValue element, Stream stream)
            {
                using (var textWriter = new StreamWriter(stream, Encoding.UTF8))
                    textWriter.Write(JsonValue.ToStringIndented(element));
            }

            bool IClassifyFormat<JsonValue>.IsNull(JsonValue element)
            {
                return element == null || element is JsonNoValue;
            }

            object IClassifyFormat<JsonValue>.GetSimpleValue(JsonValue element)
            {
                if (element is JsonDict && element.ContainsKey(":value"))
                    element = element[":value"];

                if (element is JsonString)
                    return element.GetString();
                else if (element is JsonBool)
                    return element.GetBool();
                else if (element is JsonNumber)
                    return ((JsonNumber) element).RawValue;
                else
                    return null;
            }

            JsonValue IClassifyFormat<JsonValue>.GetSelfValue(JsonValue element)
            {
                return element;
            }

            IEnumerable<JsonValue> IClassifyFormat<JsonValue>.GetList(JsonValue element, int? tupleSize)
            {
                if (element is JsonDict && element.ContainsKey(":value"))
                    return element[":value"].GetList();
                return element.GetList();
            }

            void IClassifyFormat<JsonValue>.GetKeyValuePair(JsonValue element, out JsonValue key, out JsonValue value)
            {
                key = element[0];
                value = element[1];
            }

            IEnumerable<KeyValuePair<object, JsonValue>> IClassifyFormat<JsonValue>.GetDictionary(JsonValue element)
            {
                return element.GetDict().Select(kvp => new KeyValuePair<object, JsonValue>(kvp.Key, kvp.Value));
            }

            bool IClassifyFormat<JsonValue>.HasField(JsonValue element, string fieldName)
            {
                if (fieldName.StartsWith(':'))
                    fieldName = ":" + fieldName;
                return element is JsonDict && element.ContainsKey(fieldName);
            }

            JsonValue IClassifyFormat<JsonValue>.GetField(JsonValue element, string fieldName)
            {
                if (fieldName.StartsWith(':'))
                    fieldName = ":" + fieldName;
                return element[fieldName];
            }

            string IClassifyFormat<JsonValue>.GetType(JsonValue element, out bool isFullType)
            {
                if (element is JsonDict)
                {
                    isFullType = element.ContainsKey(":fulltype");
                    return element.Safe[isFullType ? ":fulltype" : ":type"].GetStringSafe();
                }
                isFullType = false;
                return null;
            }

            bool IClassifyFormat<JsonValue>.IsReference(JsonValue element)
            {
                return element is JsonDict && element.ContainsKey(":ref");
            }

            bool IClassifyFormat<JsonValue>.IsReferable(JsonValue element)
            {
                return element is JsonDict && element.ContainsKey(":refid");
            }

            bool IClassifyFormat<JsonValue>.IsFollowID(JsonValue element)
            {
                return element is JsonDict && element.ContainsKey(":id");
            }

            string IClassifyFormat<JsonValue>.GetReferenceID(JsonValue element)
            {
                return
                    element.ContainsKey(":ref") ? element[":ref"].GetString() :
                    element.ContainsKey(":refid") ? element[":refid"].GetString() :
                    element.ContainsKey(":id") ? element[":id"].GetString() : null;
            }

            JsonValue IClassifyFormat<JsonValue>.FormatNullValue()
            {
                return JsonNoValue.Instance;
            }

            JsonValue IClassifyFormat<JsonValue>.FormatSimpleValue(object value)
            {
                if (value == null)
                    return null;

                if (value is double)
                    return (double) value;
                if (value is float)
                    return (float) value;
                if (value is byte)
                    return (byte) value;
                if (value is sbyte)
                    return (sbyte) value;
                if (value is short)
                    return (short) value;
                if (value is ushort)
                    return (ushort) value;
                if (value is int)
                    return (int) value;
                if (value is uint)
                    return (uint) value;
                if (value is long)
                    return (long) value;
                if (value is ulong)
                    return (double) (ulong) value;
                if (value is decimal)
                    return (decimal) value;
                if (value is bool)
                    return (bool) value;
                if (value is string)
                    return (string) value;

                // This takes care of enum types
                return ExactConvert.ToString(value);
            }

            JsonValue IClassifyFormat<JsonValue>.FormatSelfValue(JsonValue value)
            {
                return value;
            }

            JsonValue IClassifyFormat<JsonValue>.FormatList(bool isTuple, IEnumerable<JsonValue> values)
            {
                return new JsonList(values);
            }

            JsonValue IClassifyFormat<JsonValue>.FormatKeyValuePair(JsonValue key, JsonValue value)
            {
                return new JsonList(new[] { key, value });
            }

            JsonValue IClassifyFormat<JsonValue>.FormatDictionary(IEnumerable<KeyValuePair<object, JsonValue>> values)
            {
                return values.ToJsonDict(kvp => ExactConvert.ToString(kvp.Key), kvp => kvp.Value);
            }

            JsonValue IClassifyFormat<JsonValue>.FormatObject(IEnumerable<KeyValuePair<string, JsonValue>> fields)
            {
                return new JsonDict(fields.Select(kvp => kvp.Key.StartsWith(':') ? new KeyValuePair<string, JsonValue>(":" + kvp.Key, kvp.Value) : kvp));
            }

            JsonValue IClassifyFormat<JsonValue>.FormatFollowID(string id)
            {
                return new JsonDict { { ":id", id } };
            }

            JsonValue IClassifyFormat<JsonValue>.FormatReference(string refId)
            {
                return new JsonDict { { ":ref", refId } };
            }

            JsonValue IClassifyFormat<JsonValue>.FormatReferable(JsonValue element, string refId)
            {
                if (!(element is JsonDict))
                    return new JsonDict { { ":refid", refId }, { ":value", element } };

                element[":refid"] = refId;
                return element;
            }

            JsonValue IClassifyFormat<JsonValue>.FormatWithType(JsonValue element, string type, bool isFullType)
            {
                if (!(element is JsonDict))
                    return new JsonDict { { isFullType ? ":fulltype" : ":type", type }, { ":value", element } };

                element[isFullType ? ":fulltype" : ":type"] = type;
                return element;
            }
        }
    }
}