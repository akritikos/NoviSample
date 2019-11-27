namespace Kritikos.NoviSample.Services
{
	using System;
	using System.Net.Http;
	using System.Threading.Tasks;

	using Kritikos.NoviSample.Services.Contracts;
	using Kritikos.NoviSample.Services.Models;

	using Newtonsoft.Json;

	public class HttpClientIpStackService : IIPInfoProvider
	{
		private const string BaseUri = "api.ipstack.com";
		private static readonly HttpClient Client = new HttpClient();

		// Should be secure string in untrusted environments
		// showcasing normal string for simplicity
		private readonly string apikey;

		public HttpClientIpStackService(string apiKey, bool https = false)
		{
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				throw new ArgumentNullException(nameof(apiKey), "IPStack Api Key is required!");
			}

			apikey = apiKey;
			var url = https
				? new Uri($"https://{BaseUri}")
				: new Uri($"http://{BaseUri}");
			Client.BaseAddress = url;
			Client.DefaultRequestHeaders.Accept.Add(
				new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
		}

		[Obsolete(
			"HttpClient exposes only async behavior, this is a workaround to adhere to the provided interface. Please use GetDetailsAsync instead!",
			false)]
		public IPDetails GetDetails(string ip)
		{
			// Bad Practice, but our interface demands IPDetails, not Task<IPDetails> so we can't go async
			var result = Task.Run(() => Client.GetStringAsync($"{ip}?access_key={apikey}")).GetAwaiter().GetResult();
			var details = JsonConvert.DeserializeObject<IpDetailResponse>(result);
			details.Ip = ip;
			return details;
		}

		public async Task<IPDetails> GetDetailsAsync(string ip)
		{
			var result = await Client.GetStringAsync($"{ip}?access_key={apikey}");
			var details = JsonConvert.DeserializeObject<IpDetailResponse>(result);
			details.Ip = ip;
			return details;
		}
	}
}
