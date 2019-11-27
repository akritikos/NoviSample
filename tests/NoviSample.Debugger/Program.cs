namespace Kritikos.NoviSample.Debugger
{
	using System;

	using Kritikos.NoviSample.Services;
	using Kritikos.NoviSample.Services.Contracts;

	public static class Program
	{
		public static void Main()
		{
			var apiKey = Environment.GetEnvironmentVariable("IpStack:Api");
			var provider = new HttpClientIpStackService(apiKey);

			var t = provider.GetDetails("46.198.130.75");
		}
	}
}
