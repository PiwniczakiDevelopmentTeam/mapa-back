using System.Text.Json.Serialization;

namespace mapa_back.Models.RSPOApi
{
    public class Geolokalizacja
    {
		[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
		[JsonPropertyName("latitude")]
		public double Latitude { get; set; }

		[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
		[JsonPropertyName("longitude")]
		public double Longitude { get; set; }
	}
}
