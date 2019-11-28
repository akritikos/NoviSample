namespace Kritikos.NoviSample.Services.Models
{
	using System.Diagnostics.CodeAnalysis;

	using Kritikos.NoviSample.Services.Contracts;

	using Newtonsoft.Json;

	[SuppressMessage(
		"Performance",
		"CA1819:Properties should not return arrays",
		Justification = "Sql implementation of database concurrency")]
	public class IpDetailResponse : IPDetails
	{
		public string Ip { get; set; } = string.Empty;

		public string City { get; set; } = string.Empty;

		[JsonProperty(PropertyName = "country_name")]
		public string Country { get; set; } = string.Empty;

		[JsonProperty(PropertyName = "continent_name")]
		public string Continent { get; set; } = string.Empty;

		public double Latitude { get; set; } = double.NaN;

		public double Longitude { get; set; } = double.NaN;
	}
}
