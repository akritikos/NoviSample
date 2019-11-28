namespace Kritikos.NoviSample.HostedServices
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Caching;
	using System.Threading;
	using System.Threading.Tasks;

	using Kritikos.NoviSample.HostedServices.Contracts;
	using Kritikos.NoviSample.Persistence;
	using Kritikos.NoviSample.Services.Contracts;

	using Microsoft.EntityFrameworkCore;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;

	public class IpBatchingService : BackgroundService
	{
		private IServiceScope scope;

		public IpBatchingService(
			IInMemoryBackgroundIpQueue queue,
			ILogger<IpBatchingService> logger,
			IIpInfoProviderAsync ipInfoProvider,
			MemoryCache cache,
			IServiceScopeFactory scopeFactory)
		{
			Queue = queue;
			Logger = logger;
			IpInfoProvider = ipInfoProvider;
			scope = scopeFactory.CreateScope();
			Context = scope.ServiceProvider.GetRequiredService<NovibetDbContext>();
			Cache = cache;
		}

		private IIpInfoProviderAsync IpInfoProvider { get; }

		private IInMemoryBackgroundIpQueue Queue { get; }

		private ILogger<IpBatchingService> Logger { get; }

		private NovibetDbContext Context { get; }

		private MemoryCache Cache { get; }

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await Execute(stoppingToken);
		}

		private async Task Execute(CancellationToken stoppingToken = default)
		{
			Logger.LogDebug("Ip Batching service is ready!");
			while (!stoppingToken.IsCancellationRequested)
			{
				var item = await Queue.DequeueAsync(stoppingToken);
				if (item.Count == 0)
				{
					// No need to constantly ask for items
					await Task.Delay(1000, stoppingToken);
					continue;
				}

				var guids = item.Select(x => x.Identifier).GroupBy(x => x).Select(x => x.Key).ToList();
				Logger.LogInformation("Processing {Count} ip addresses from batches {@Batches}", item.Count, guids);
				var results = await IpInfoProvider.GetBulkDetailsAsync(item.Select(x => x.address).ToArray());
				var response = results.Select(x => (
						identifier: item.Single(y => y.address == x.Ip).Identifier,
						response: x))
					.ToList();
				var ips = response.Select(x => x.response.Ip).ToList();

				var existing = Context.IpDetails
					.TagWith("Fetching details for batch job refresh")
					.Where(x => ips.Contains(x.Ip))
					.ToList();
				var remaining = response.Where(x => !existing.Select(y => y.Ip == x.response.Ip).Any())
					.Select(x => x.response)
					.Select(x => new IpDetails
					{
						Ip = x.Ip,
						City = x.City,
						Continent = x.Continent,
						Country = x.Country,
						Latitude = x.Latitude,
						Longitude = x.Longitude,
					})
					.ToList();
				remaining.ForEach(x =>
				{
					var newVals = response.Select(y => y.response).Single(y => y.Ip == x.Ip);
					x.Latitude = newVals.Latitude;
					x.Longitude = newVals.Longitude;
					x.City = newVals.City;
					x.Continent = newVals.Continent;
					x.Country = newVals.Country;
				});

				Context.IpDetails.AddRange(remaining);
				await Context.SaveChangesAsync(stoppingToken);

				remaining.Union(existing)
					.ToList()
					.ForEach(x => Cache.Set(x.Ip, x, DateTime.UtcNow.AddMinutes(1)));
				Queue.MarkCompleted(response);

				// Simulates long duration calls
				Thread.Sleep(5000);
			}

			Logger.LogDebug("Ip Batching service is shutting down!");
		}
	}
}
