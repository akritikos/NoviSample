namespace Kritikos.NoviSample.Debugger
{
	using System;

	using Kritikos.NoviSample.Services;
	using Kritikos.NoviSample.Services.Contracts;

	public static class Program
	{
		public static void Main()
		{
			var apiKey = Environment.GetEnvironmentVariable("IpStack:Api") ?? string.Empty;
			var provider = new HttpClientIpStackService(apiKey);

#pragma warning disable CS0618 // Type or member is obsolete
			var t = provider.GetDetails("46.198.130.75");
#pragma warning restore CS0618 // Type or member is obsolete

			Console.WriteLine(t);
		}
	}
}
