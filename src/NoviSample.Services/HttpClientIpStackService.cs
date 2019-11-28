namespace Kritikos.NoviSample.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Threading.Tasks;

	using Kritikos.NoviSample.Services.Contracts;
	using Kritikos.NoviSample.Services.Models;

	using Newtonsoft.Json;

	public class HttpClientIpStackService : IIpInfoProviderAsync
	{
		private const string BaseUri = "api.ipstack.com";
		private static readonly HttpClient Client = new HttpClient();

		private static readonly JsonSerializerSettings SerializerSettings =
			new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

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
			var details = JsonConvert.DeserializeObject<IpDetailResponse>(result, SerializerSettings);
			return details;
		}

		public async Task<IpDetailResponse> GetDetailsAsync(string ip)
		{
			var result = await Client.GetStringAsync($"{ip}?access_key={apikey}");
			var details = JsonConvert.DeserializeObject<IpDetailResponse>(result, SerializerSettings);
			return details;
		}

		// Actual batching, does not work since we're on the free plan...
		public async Task<List<IpDetailResponse>> BulkDetailsAsync(string[] ipList)
		{
			var result = await Client.GetStringAsync($"{string.Join(",", ipList)}?access_key={apikey}");
			var details = JsonConvert.DeserializeObject<List<IpDetailResponse>>(result, SerializerSettings);
			return details;
		}

		// Poor man's batching...
		public async Task<List<IpDetailResponse>> GetBulkDetailsAsync(string[] ipList)
		{
			if (ipList == null)
			{
				throw new ArgumentNullException(nameof(ipList));
			}

			var result = new List<IpDetailResponse>();
			foreach (var address in ipList)
			{
				result.Add(await GetDetailsAsync(address));
			}

			return result;
		}
	}
}
