namespace Kritikos.NoviSample.Services.Models
{
	using System.ComponentModel.DataAnnotations;

	using Kritikos.NoviSample.Services.Contracts;

	using Newtonsoft.Json;

	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Performance",
		"CA1819:Properties should not return arrays",
		Justification = "Sql implementation of database concurrency")]
	public class IpDetailResponse : IPDetails
	{
		public long Id { get; set; }

		[Timestamp]
#pragma warning disable CS8618 // Database handled field
		public byte[] RowVersion { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

		public string City { get; set; } = string.Empty;

		[JsonProperty(PropertyName = "country_name")]
		public string Country { get; set; } = string.Empty;

		[JsonProperty(PropertyName = "continent_name")]
		public string Continent { get; set; } = string.Empty;

		public double Latitude { get; set; }

		public double Longitude { get; set; }
	}
}
