﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ahd.Graphite
{
    /// <summary>
    /// time series datapoint
    /// </summary>
    [JsonConverter(typeof(MetricDatapointConverter))]
    public class MetricDatapoint
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// value of the datapoint
        /// </summary>
        [JsonPropertyName("value")]
        public double? Value { get; }

        /// <summary>
        /// timestamp of the datapoint
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp
        {
            get { return Epoch.AddSeconds(UnixTimestamp); }
        }

        /// <summary>
        /// timestamp in unix epoch (seconds since 1970)
        /// </summary>
        public long UnixTimestamp { get; }

        /// <summary>
        /// creates a datapoint with the specified value and timestamp
        /// </summary>
        /// <param name="value">value of the datapoint</param>
        /// <param name="timestamp">seconds since unix epoch</param>
        public MetricDatapoint(double? value, long timestamp)
        {
            Value = value;
            UnixTimestamp = timestamp;
        }

        internal class MetricDatapointConverter : JsonConverter<MetricDatapoint>
        {
            public override MetricDatapoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var exception = new JsonException("Cannot deserialize MetricDatapoint");
                if (reader.TokenType != JsonTokenType.StartArray || !reader.Read())
                    throw exception;

                double? value = null;
                if (reader.TokenType != JsonTokenType.Number && reader.TokenType != JsonTokenType.Null)
                {
                   
                    throw new JsonException($"Unexpected Token {reader.TokenType}");
                }
                
                if (reader.TryGetDouble(out var current))
                    value = current;

                if (!reader.Read() || !reader.TryGetDouble(out var timestamp) || !reader.Read() || reader.TokenType != JsonTokenType.EndArray)
                    throw exception;

                return new MetricDatapoint(value, (long) timestamp);
            }

            public override void Write(Utf8JsonWriter writer, MetricDatapoint datapoint, JsonSerializerOptions options)
            {
                if (datapoint == null)
                {
                    writer.WriteNullValue();
                    return;
                }
                writer.WriteStartArray();
                if (!datapoint.Value.HasValue)
                    writer.WriteNullValue();
                else
                    writer.WriteNumberValue(datapoint.Value.Value);
                writer.WriteNumberValue(datapoint.UnixTimestamp);
                writer.WriteEndArray();
            }
        }
    }
}