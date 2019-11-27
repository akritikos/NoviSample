namespace NoviSample.Api
{
	using System;
	using System.Linq;
	using System.Reflection;

	using Kritikos.NoviSample.Api;
	using Kritikos.NoviSample.Api.Helpers;
	using Kritikos.NoviSample.Persistence;

	using Microsoft.AspNetCore.Hosting;
	using Microsoft.EntityFrameworkCore;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;

	using Serilog;
	using Serilog.Events;
	using Serilog.Exceptions;
	using Serilog.Exceptions.Core;
	using Serilog.Exceptions.Destructurers;
	using Serilog.Exceptions.SqlServer.Destructurers;
	using Serilog.Filters;
	using Serilog.Formatting;
	using Serilog.Formatting.Compact;
	using Serilog.Formatting.Json;
	using Serilog.Sinks.SystemConsole.Themes;

	public static class Program
	{
		private const string NoisyLogs =
			"SourceContext='Microsoft.Hosting.Lifetime' or SourceContext='Serilog.AspNetCore.RequestLoggingMiddleware'";

		public static void Main(string[] args)
		{
			Log.Logger = BasicLogger()
				.CreateLogger();

			try
			{
				var host = CreateHostBuilder(args).Build();
				host.MigrateDatabase();

				host.Run();
			}
#pragma warning disable CA1031 // Final frontier, catching generic type exception to log it and preserve the stack trace
			catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				Log.Fatal(e, LogMessages.UnhandledException, e.Message);
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		private static void MigrateDatabase(this IHost host)
		{
			using var scope = host.Services.CreateScope();

			var services = scope.ServiceProvider;
			var context = services.GetRequiredService<NovibetDbContext>();
			var migrations = context.Database.GetPendingMigrations().ToList();

			if (migrations.Any())
			{
				context.Database.Migrate();
				Log.Information(
					LogMessages.MigrationsApplied,
					migrations.ToList());
			}
		}

		private static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				})
				.ConfigureAppConfiguration(options => options
					.AddJsonFile("connectionStrings.json", true, true))
				.ConfigureLogging(log =>
				{
					log.ClearProviders();
					log.SetMinimumLevel(LogLevel.Trace);
				})
				.UseSerilog();

		private static LoggerConfiguration BasicLogger()
			=> new LoggerConfiguration()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
				.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
				.MinimumLevel.Override("System", LogEventLevel.Error)
				.Enrich.WithProperty("Application", Assembly.GetExecutingAssembly().GetName().Name)
				.Enrich.FromLogContext()
				.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
					.WithDefaultDestructurers()
					.WithDestructurers(new IExceptionDestructurer[]
					{
						new DbUpdateExceptionDestructurer(), new SqlExceptionDestructurer(),
					})
					.WithRootName("Exception"))
				.WriteTo.Debug()
				.WriteTo.Logger(c => c
					.MinimumLevel.Debug()
					.Filter.ByExcluding(NoisyLogs)
					.WriteTo.Console());
	}
}
