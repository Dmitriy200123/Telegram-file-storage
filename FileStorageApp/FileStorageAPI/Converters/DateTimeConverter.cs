﻿using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileStorageAPI.Converters
{
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(DateTime));
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("dd'.'MM'.'yyyy' 'HH':'mm':'ss"));
        }
    }
}