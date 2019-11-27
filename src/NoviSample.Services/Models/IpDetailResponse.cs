namespace Kritikos.NoviSample.Services.Models
{
	using Kritikos.NoviSample.Services.Contracts;

	using Newtonsoft.Json;

	public class IpDetailResponse : IPDetails
	{
		public string City { get; set; } = string.Empty;

		[JsonProperty(PropertyName = "country_name")]
		public string Country { get; set; } = string.Empty;

		[JsonProperty(PropertyName = "continent_name")]
		public string Continent { get; set; } = string.Empty;

		public double Latitude { get; set; }

		public double Longitude { get; set; }
	}
}
