namespace Kritikos.NoviSample.Api
{
	using System;
	using System.IO;
	using System.Reflection;

	using Kritikos.NoviSample.Persistence;

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

		public Startup(IConfiguration configuration, IWebHostEnvironment environment)
		{
			Configuration = configuration;
			Environment = environment;
		}

		private static ILoggerFactory LoggerFactory { get; }

		private IConfiguration Configuration { get; }

		private IWebHostEnvironment Environment { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<NovibetDbContext>(o =>
			{
				o.UseSqlServer(Configuration.GetConnectionString("Envity"), options => options.EnableRetryOnFailure())
					.EnableSensitiveDataLogging(Environment.IsDevelopment())
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

				if (!Environment.IsDevelopment())
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
