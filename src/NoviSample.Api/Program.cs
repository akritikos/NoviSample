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

		private static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				})
				.ConfigureAppConfiguration((context, options) => options
					.SetBasePath(context.HostingEnvironment.ContentRootPath)
					.AddJsonFile("appsettings.json", false, true)
					.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
					.AddJsonFile("connectionStrings.json", true, true)
					.AddEnvironmentVariables())
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
