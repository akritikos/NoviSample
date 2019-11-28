namespace Kritikos.NoviSample.HostedServices
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	using Kritikos.NoviSample.Persistence;

	using Microsoft.Data.SqlClient;
	using Microsoft.EntityFrameworkCore;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;

	public class MigrationService : IHostedService
	{
		public MigrationService(ILogger<MigrationService> logger, IServiceProvider services)
		{
			Logger = logger;
			Services = services;
		}

		private ILogger<MigrationService> Logger { get; }

		private IServiceProvider Services { get; }

		/// <inheritdoc />
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			using var scope = Services.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<NovibetDbContext>();
			var migrations = context.Database.GetPendingMigrations().ToList();
			if (!migrations.Any())
			{
				return;
			}

			Logger.LogInformation("Applying {Count} migrations!", migrations.Count);

			try
			{
				await context.Database.MigrateAsync();
			}
			catch (SqlException e)
			{
				Logger.LogCritical(e, "Error while updating database! {@Reason}", e.Message);
			}

			Logger.LogInformation(
				"Successfuly applied migrations: {@MigrationList}",
				migrations.ToList());
		}

		/// <inheritdoc />
		public Task StopAsync(CancellationToken cancellationToken)
			=> Task.CompletedTask;
	}
}
