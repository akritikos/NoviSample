namespace Kritikos.NoviSample.Api
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Runtime.Caching;

	using Kritikos.NoviSample.HostedServices;
	using Kritikos.NoviSample.HostedServices.Contracts;
	using Kritikos.NoviSample.HostedServices.Implementations;
	using Kritikos.NoviSample.Persistence;
	using Kritikos.NoviSample.Services;
	using Kritikos.NoviSample.Services.Contracts;

	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.EntityFrameworkCore;
	using Microsoft.EntityFrameworkCore.Diagnostics;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;
	using Microsoft.OpenApi.Models;

	using Serilog;
	using Serilog.Extensions.Logging;

	using Swashbuckle.AspNetCore.SwaggerUI;

	public class Startup
	{
		static Startup()
			=> LoggerFactory = new SerilogLoggerFactory(Log.Logger);

		public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
		{
			Configuration = configuration;
			HostEnvironment = hostEnvironment;
		}

		private static ILoggerFactory LoggerFactory { get; }

		private IConfiguration Configuration { get; }

		private IWebHostEnvironment HostEnvironment { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddHostedService<MigrationService>();
			services.AddDbContext<NovibetDbContext>(o =>
			{
				o.UseSqlServer(Configuration.GetConnectionString("Novibet"), options => options.EnableRetryOnFailure())
					.EnableSensitiveDataLogging(HostEnvironment.IsDevelopment())
					.ConfigureWarnings(warnings =>
					{
						warnings.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning);
						warnings.Log(RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning);
					})
					.UseLoggerFactory(LoggerFactory);
			});

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "Novibet API", Version = "v1" });
				c.IncludeXmlComments(
					Path.Combine(
						AppContext.BaseDirectory,
						$"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));

				c.DescribeAllParametersInCamelCase();
			});

			services.AddControllers()
				.AddControllersAsServices()
				.SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

			services.AddSingleton(MemoryCache.Default);
			services.AddSingleton<IIpInfoProviderAsync>(
				new HttpClientIpStackService(Configuration["IpStack:Api"] ?? string.Empty));
			services.AddSingleton<IInMemoryBackgroundIpQueue, InMemoryBackgroundIpQueue>();

			services.AddHostedService<IpBatchingService>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Novibet API");
				c.DocExpansion(DocExpansion.None);
				c.EnableDeepLinking();
				c.EnableFilter();
				c.EnableValidator();
				c.DocumentTitle = "Novibet API Documentation";

				if (!HostEnvironment.IsDevelopment())
				{
					return;
				}

				c.DisplayOperationId();
				c.DisplayRequestDuration();
			});

			app.UseSerilogRequestLogging();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGet("/", async context =>
				{
					await context.Response.WriteAsync("Hello World!");
				});
				endpoints.MapControllers();
			});
		}
	}
}
