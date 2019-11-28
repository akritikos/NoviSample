namespace Kritikos.NoviSample.Api.Controller
{
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Runtime.Caching;
	using System.Threading.Tasks;

	using Kritikos.NoviSample.Api.Helpers;
	using Kritikos.NoviSample.HostedServices.Contracts;
	using Kritikos.NoviSample.Persistence;
	using Kritikos.NoviSample.Services.Contracts;
	using Kritikos.NoviSample.Services.Models;

	using Microsoft.AspNetCore.Mvc;
	using Microsoft.EntityFrameworkCore;
	using Microsoft.Extensions.Logging;

	using Newtonsoft.Json;

	[ApiController]
	[Route("api/ip")]
	public class IpController : Controller
	{
		public IpController(
			ILogger<IpController> logger,
			NovibetDbContext dbContext,
			MemoryCache cache,
			IIpInfoProviderAsync ipInfoProvider,
			IInMemoryBackgroundIpQueue queue)
		{
			Logger = logger;
			Context = dbContext;
			Cache = cache;
			Queue = queue;
			IpInfoProvider = ipInfoProvider;
		}

		private IIpInfoProviderAsync IpInfoProvider { get; }

		private NovibetDbContext Context { get; }

		private MemoryCache Cache { get; }

		private ILogger<IpController> Logger { get; }

		private IInMemoryBackgroundIpQueue Queue { get; }

		/// <summary>
		/// Removes an IP address from cache forcibly, simulating the passage of time.
		/// </summary>
		/// <param name="address">The Ip address to remove.</param>
		/// <returns>Nothing, call can not fail and never has a response.</returns>
		[HttpGet("Uncache/{address}")]
		public ActionResult ClearFromCache(string address)
		{
			if (Cache.Contains(address))
			{
				Logger.LogInformation(LogMessages.RemovingIpFromCacheRequested, address);
				Cache.Remove(address);
			}
			else
			{
				Logger.LogWarning(LogMessages.RemovalOfNonExistingCacheItemRequested, address);
			}

			return NoContent();
		}

		/// <summary>
		/// Gets Ip address details from cache, database or IpStack, in that order.
		/// </summary>
		/// <param name="address">The IP to fetch details for.</param>
		/// <returns>Ip details or error message.</returns>
		[HttpGet("retrieve/{address}")]
		public async Task<ActionResult> GetIpSlidingCache(string address)
		{
			var isValid = IPAddress.TryParse(address, out var ip);
			if (!isValid)
			{
				Logger.LogError(LogMessages.InvalidIpFormat, address);
				return BadRequest("Unsupported IP format!");
			}

			if (Cache.Get(address) is IpDetailResponse cached)
			{
				Logger.LogDebug(LogMessages.RequestedInfoForIp, address);
				return Ok(cached);
			}

			var details = await Context.IpDetails.Where(x => x.Ip == address).FirstOrDefaultAsync();
			if (details != null)
			{
				// Depending on needs, sliding expiration might be better for performance reasons
				Cache.Add(address, details, DateTime.UtcNow.AddMinutes(1));
				Logger.LogWarning(LogMessages.RequestedInfoForIpNoCache, address);
				return Ok(details);
			}

			IpDetails response;
			try
			{
				var apiQuery = await IpInfoProvider.GetDetailsAsync(address);
				response = Mapper.IpDetailResponseToEntity(apiQuery);
			}
			catch (JsonSerializationException e)
			{
				Logger.LogCritical(e, LogMessages.InvalidIpResponse, e.Message);
				return BadRequest(
					"Could not extract data from API service, are you sure the IP address you requested is world routable?");
			}

			Context.IpDetails.Add(response);
			await Context.SaveChangesAsync();

			// Depending on needs, sliding expiration might be better for performance reasons
			Cache.Add(address, response, DateTime.UtcNow.AddMinutes(1));
			Logger.LogCritical(LogMessages.RequestedInfoForIpNoPersistence, address);

			return Ok(response);
		}

		[HttpPost("batch")]
		public ActionResult BatchIpRequest(string[] addresses)
		{
			var jobId = Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture);
			Queue.QueueBackgroundWorkItem((jobId, addresses.ToList()));
			return Ok(jobId);
		}

		[HttpGet("batch/{jobId}")]
		public ActionResult BatchIpReport(string jobId)
		{
			var (remaining, done) = Queue.GetProgressOfBatch(jobId);
			if (remaining == null || done == null)
			{
				return NotFound("No such job, are you sure it hasn't been completed?");
			}

			return Ok($"{done}/{remaining + done}");
		}
	}
}
