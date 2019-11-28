namespace Kritikos.NoviSample.Persistence
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Text;

	[SuppressMessage(
		"Performance",
		"CA1819:Properties should not return arrays",
		Justification = "Recommended way to handle database concurrency")]
	public class IpDetails
	{
		public long Id { get; set; }

#pragma warning disable CS8618 // Handled by database
		public byte[] RowVersion { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

		public string Ip { get; set; } = string.Empty;

		public string City { get; set; } = string.Empty;

		public string Country { get; set; } = string.Empty;

		public string Continent { get; set; } = string.Empty;

		public double Latitude { get; set; }

		public double Longitude { get; set; }
	}
}
