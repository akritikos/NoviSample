namespace Kritikos.NoviSample.Api.Helpers
{
	using System;

	using Kritikos.NoviSample.Persistence;
	using Kritikos.NoviSample.Services.Models;

	public static class Mapper
	{
		public static IpDetails IpDetailResponseToEntity(IpDetailResponse response)
			=> response == null
				? throw new ArgumentNullException(nameof(response))
				: new IpDetails
				{
					City = response.City,
					Continent = response.Continent,
					Country = response.Country,
					Ip = response.Ip,
					Latitude = response.Latitude,
					Longitude = response.Longitude,
				};
	}
}
