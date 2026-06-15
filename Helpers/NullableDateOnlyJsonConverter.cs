using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace mapa_back.Helpers
{
	public class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
	{
		private const string Format = "yyyy-MM-dd";

		public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
			{
				return null;
			}

			if (reader.TokenType == JsonTokenType.String)
			{
				string? str = reader.GetString();
				if (string.IsNullOrWhiteSpace(str))
				{
					return null;
				}
				if (DateOnly.TryParseExact(str, Format, out var result))
				{
					return result;
				}
				if (DateOnly.TryParse(str, out result))
				{
					return result;
				}
			}

			throw new JsonException($"Unable to parse \"{reader.GetString()}\" as DateOnly.");
		}

		public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
		{
			if (value.HasValue)
			{
				writer.WriteStringValue(value.Value.ToString(Format));
			}
			else
			{
				writer.WriteNullValue();
			}
		}
	}
}