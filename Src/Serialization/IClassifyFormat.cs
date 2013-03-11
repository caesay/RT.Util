﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RT.Util.Serialization
{
    /// <summary>
    ///     Used by <see cref="Classify"/> to serialize and deserialize objects. Implement this to enable serialization to a new
    ///     format.</summary>
    /// <typeparam name="TElement">
    ///     Type of the serialized form of an object or any sub-object.</typeparam>
    public interface IClassifyFormat<TElement>
    {
        /// <summary>
        ///     Reads the serialized form from a stream.</summary>
        /// <param name="stream">
        ///     Stream to read from.</param>
        /// <returns>
        ///     The serialized form read from the stream.</returns>
        TElement ReadFromStream(Stream stream);

        /// <summary>
        ///     Writes the serialized form to a stream.</summary>
        /// <param name="element">
        ///     Serialized form to write to the stream.</param>
        /// <param name="stream">
        ///     Stream to write to.</param>
        void WriteToStream(TElement element, Stream stream);

        /// <summary>
        ///     Determines whether the specified element represents a <c>null</c> value.</summary>
        /// <remarks>
        ///     This should return <c>true</c> if the element was generated by <see cref="FormatNullValue"/> and <c>false</c>
        ///     otherwise.</remarks>
        bool IsNull(TElement element);

        /// <summary>
        ///     Called when Classify expects the element to be one of the following types: <c>byte</c>, <c>sbyte</c>,
        ///     <c>short</c>, <c>ushort</c>, <c>int</c>, <c>uint</c>, <c>long</c>, <c>ulong</c>, <c>decimal</c>, <c>float</c>,
        ///     <c>double</c>, <c>bool</c>, <c>char</c>, <c>string</c>, or <c>DateTime</c>. The implementation is free to return a
        ///     value of any of these types, and Classify will automatically use <see cref="ExactConvert"/> to convert the value
        ///     to the required target type.</summary>
        /// <remarks>
        ///     This should decode values passed into <see cref="FormatSimpleValue"/>, although it is acceptable if the type has
        ///     changed. For example, all values may be returned as <c>string</c>, as long as <see cref="ExactConvert"/> will
        ///     convert that <c>string</c> back to the original value.</remarks>
        object GetSimpleValue(TElement element);

        /// <summary>
        ///     Decodes the serialized form of the element type itself.</summary>
        /// <remarks>
        ///     This should do the reverse of <see cref="FormatSelfValue"/>.</remarks>
        TElement GetSelfValue(TElement element);

        /// <summary>
        ///     Decodes a list.</summary>
        /// <param name="element">
        ///     The element to decode.</param>
        /// <param name="tupleSize">
        ///     If null, a variable-length list is expected; otherwise, a fixed-length list (a tuple) is expected.</param>
        /// <returns>
        ///     A collection containing the sub-elements contained in the list. The collection returned need not have the size
        ///     specified by <paramref name="tupleSize"/>.</returns>
        /// <remarks>
        ///     This should do the reverse of <see cref="FormatList"/>.</remarks>
        IEnumerable<TElement> GetList(TElement element, int? tupleSize);

        /// <summary>
        ///     Decodes a key-value pair.</summary>
        /// <param name="element">
        ///     The element to decode.</param>
        /// <param name="key">
        ///     Receives the key part of the pair.</param>
        /// <param name="value">
        ///     Receives the value part of the pair.</param>
        /// <remarks>
        ///     This should do the reverse of <see cref="FormatKeyValuePair"/>.</remarks>
        void GetKeyValuePair(TElement element, out TElement key, out TElement value);

        /// <summary>
        ///     Decodes a dictionary.</summary>
        /// <param name="element">
        ///     The element to decode.</param>
        /// <returns>
        ///     A collection containing the key/value pairs in the dictionary. The keys in the returned collection are expected to
        ///     be convertible to the correct type using <see cref="ExactConvert"/>.</returns>
        /// <remarks>
        ///     This should decode values passed into <see cref="FormatDictionary"/>, although it is acceptable if the type of the
        ///     keys has changed. For example, all keys may be returned as <c>string</c>, as long as <see cref="ExactConvert"/>
        ///     will convert that <c>string</c> back to the original value.</remarks>
        IEnumerable<KeyValuePair<object, TElement>> GetDictionary(TElement element);

        /// <summary>
        ///     Determines whether the element is an object and contains a sub-element for the specified field.</summary>
        /// <param name="element">
        ///     The element that may represent an object.</param>
        /// <param name="fieldName">
        ///     The name of the field sought.</param>
        /// <returns>
        ///     <c>true</c> if <paramref name="element"/> is an object and has the specified field; <c>false</c>
        ///     otherwise.</returns>
        /// <remarks>
        ///     This should return <c>true</c> if <paramref name="element"/> represents an element generated by <see
        ///     cref="FormatObject"/> in which the field with the specified <paramref name="fieldName"/> was present.</remarks>
        bool HasField(TElement element, string fieldName);

        /// <summary>
        ///     Returns the sub-element pertaining to the specified field.</summary>
        /// <param name="element">
        ///     The element that represents an object.</param>
        /// <param name="fieldName">
        ///     The name of the field within the object whose sub-element is sought.</param>
        /// <returns>
        ///     The sub-element for the specified field.</returns>
        /// <remarks>
        ///     <para>
        ///         Classify calls <see cref="HasField"/> first for each field and only calls this method if <see
        ///         cref="HasField"/> returned true.</para>
        ///     <para>
        ///         This should return the same element that was passed into <see cref="FormatObject"/> for the same <paramref
        ///         name="fieldName"/>.</para></remarks>
        TElement GetField(TElement element, string fieldName);

        /// <summary>
        ///     Determines the type of the object stored in the specified element.</summary>
        /// <param name="element">
        ///     The element that represents an object.</param>
        /// <returns>
        ///     <c>null</c> if no type information was persisted in this element; otherwise, the decoded type name.</returns>
        /// <remarks>
        ///     This should decode the information generated by <see cref="FormatWithType"/>.</remarks>
        string GetType(TElement element);

        /// <summary>
        ///     Determines whether this element represents a reference to another object in the same serialized graph.</summary>
        /// <param name="element">
        ///     The element to decode.</param>
        /// <returns>
        ///     <c>true</c> if this element represents such a reference; <c>false</c> otherwise.</returns>
        /// <remarks>
        ///     This should recognize elements generated by <see cref="FormatReference"/>.</remarks>
        bool IsReference(TElement element);

        /// <summary>
        ///     Determines whether this element represents an object that can be referred to by a reference element.</summary>
        /// <param name="element">
        ///     The element to decode.</param>
        /// <returns>
        ///     <c>true</c> if this element is referable; <c>false</c> otherwise.</returns>
        /// <remarks>
        ///     This should recognize elements generated by <see cref="FormatReferable"/>.</remarks>
        bool IsReferable(TElement element);

        /// <summary>
        ///     Determines whether this element represents a reference to another file. See <see
        ///     cref="ClassifyFollowIdAttribute"/> for details.</summary>
        /// <param name="element">
        ///     The element to decode.</param>
        /// <returns>
        ///     <c>true</c> if this element is such a reference; <c>false</c> otherwise.</returns>
        /// <remarks>
        ///     This should recognize elements generated by <see cref="FormatFollowID"/>.</remarks>
        bool IsFollowID(TElement element);

        /// <summary>
        ///     Returns the ID encoded in this element. This is called only if <see cref="IsReference"/>, <see
        ///     cref="IsReferable"/> or <see cref="IsFollowID"/> returned <c>true</c>.</summary>
        /// <param name="element">
        ///     The element to decode.</param>
        /// <returns>
        ///     The ID encoded in this element.</returns>
        /// <remarks>
        ///     This should return the same ID that was passed into <see cref="FormatReference"/>, <see cref="FormatReferable"/>
        ///     or <see cref="FormatFollowID"/>.</remarks>
        string GetReferenceID(TElement element);

        TElement FormatNullValue(string name);
        TElement FormatSimpleValue(string name, object value);
        TElement FormatSelfValue(string name, TElement value);
        TElement FormatList(string name, bool isTuple, IEnumerable<TElement> values);
        TElement FormatKeyValuePair(string name, TElement key, TElement value);
        TElement FormatDictionary(string name, IEnumerable<KeyValuePair<object, TElement>> values);
        TElement FormatObject(string name, IEnumerable<KeyValuePair<string, TElement>> fields);
        TElement FormatFollowID(string name, string id);
        TElement FormatReference(string name, string refId);
        TElement FormatReferable(TElement element, string refId);
        TElement FormatWithType(TElement element, string type);
    }
}
