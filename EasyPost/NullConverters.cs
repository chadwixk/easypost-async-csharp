/*
 * Licensed under The MIT License (MIT)
 * 
 * Copyright (c) 2014 EasyPost
 * Copyright (C) 2017 AMain.com, Inc.
 * All Rights Reserved
 */

using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyPost
{
    /// <summary>
    /// Converter factory to handle conversion of null values to primitive types
    /// </summary>
    public class NullToDefaultConverterFactory : JsonConverterFactory
    {
        /// <summary>
        /// Determines whether the type can be converted. We support primitive types and the string type.
        /// </summary>
        /// <param name="typeToConvert">The type is checked as to whether it can be converted.</param>
        /// <returns>True if the type can be converted, false otherwise.</returns>
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsPrimitive || typeToConvert == typeof(string);
        }

        /// <summary>
        /// Create a converter for the provided <see cref="Type"/>.
        /// </summary>
        /// <param name="typeToConvert">The <see cref="Type"/> being converted.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
        /// <returns>
        /// An instance of a <see cref="JsonConverter{T}"/> where T is compatible with <paramref name="typeToConvert"/>.
        /// If <see langword="null"/> is returned, a <see cref="NotSupportedException"/> will be thrown.
        /// </returns>
        public override JsonConverter CreateConverter(
            Type typeToConvert, JsonSerializerOptions options)
        {
            return (JsonConverter)Activator.CreateInstance(
                typeof(NullToPrimitiveDefaultConverter<>).MakeGenericType(typeToConvert),
                BindingFlags.Instance | BindingFlags.Public,
                null,
                null,
                null);
        }
    }

    /// <summary>
    /// Class to convert from a null to a primitive type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class NullToPrimitiveDefaultConverter<T> : JsonConverter<T>
    {
        /// <summary>
        /// Read and convert the JSON to type T.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
        /// <param name="typeToConvert">The <see cref="Type"/> being converted.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
        /// <returns>The value that was converted.</returns>
        public override T Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var type = typeof(T);
            switch (reader.TokenType) {
                case JsonTokenType.True:
                case JsonTokenType.False:
                    if (type == typeof(string)) {
                        return (T)(object)reader.GetBoolean().ToString();
                    }
                    if (type == typeof(bool)) {
                        return (T)(object)reader.GetBoolean();
                    }
                    break;
                case JsonTokenType.Number:
                    if (type == typeof(string)) {
                        // A number could be a double or an integer so first try converting a integer
                        // and if that files, try as a double.
                        try {
                            return (T)(object)reader.GetInt64().ToString();
                        } catch {
                            return (T)(object)reader.GetDouble().ToString();
                        }
                    }
                    if (type == typeof(byte)) {
                        return (T)(object)reader.GetByte();
                    }
                    if (type == typeof(sbyte)) {
                        return (T)(object)reader.GetSByte();
                    }
                    if (type == typeof(short)) {
                        return (T)(object)reader.GetInt16();
                    }
                    if (type == typeof(ushort)) {
                        return (T)(object)reader.GetUInt16();
                    }
                    if (type == typeof(int)) {
                        return (T)(object)reader.GetInt32();
                    }
                    if (type == typeof(uint)) {
                        return (T)(object)reader.GetUInt32();
                    }
                    if (type == typeof(long)) {
                        return (T)(object)reader.GetInt64();
                    }
                    if (type == typeof(ulong)) {
                        return (T)(object)reader.GetUInt64();
                    }
                    if (type == typeof(float)) {
                        return (T)(object)reader.GetSingle();
                    }
                    if (type == typeof(double)) {
                        return (T)(object)reader.GetDouble();
                    }
                    break;
                case JsonTokenType.String:
                    if (type == typeof(string)) {
                        return (T)(object)reader.GetString();
                    }
                    if (type == typeof(byte)) {
                        return (T)(object)byte.Parse(reader.GetString());
                    }
                    if (type == typeof(sbyte)) {
                        return (T)(object)sbyte.Parse(reader.GetString());
                    }
                    if (type == typeof(short)) {
                        return (T)(object)short.Parse(reader.GetString());
                    }
                    if (type == typeof(ushort)) {
                        return (T)(object)ushort.Parse(reader.GetString());
                    }
                    if (type == typeof(int)) {
                        return (T)(object)int.Parse(reader.GetString());
                    }
                    if (type == typeof(uint)) {
                        return (T)(object)uint.Parse(reader.GetString());
                    }
                    if (type == typeof(long)) {
                        return (T)(object)long.Parse(reader.GetString());
                    }
                    if (type == typeof(ulong)) {
                        return (T)(object)ulong.Parse(reader.GetString());
                    }
                    if (type == typeof(float)) {
                        return (T)(object)float.Parse(reader.GetString());
                    }
                    if (type == typeof(double)) {
                        return (T)(object)double.Parse(reader.GetString());
                    }
                    break;
                case JsonTokenType.Null:
                    return default;
            }
            throw new JsonException($"Unable to convert number {reader.GetString()} to {type.Name}");
        }

        /// <summary>
        /// Write the value as JSON.
        /// </summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
        public override void Write(
            Utf8JsonWriter writer,
            T value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}