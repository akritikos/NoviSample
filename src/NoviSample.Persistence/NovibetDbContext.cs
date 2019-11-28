#pragma warning disable CS8618 // Entity frameworks requires DbSets to be privately setable.
namespace Kritikos.NoviSample.Persistence
{
	using Kritikos.NoviSample.Services.Models;

	using Microsoft.EntityFrameworkCore;
	using Microsoft.Extensions.Logging;

	public class NovibetDbContext : DbContext
	{
		private readonly ILoggerFactory loggerFactory;

		public NovibetDbContext(ILoggerFactory loggerfactory)
			=> loggerFactory = loggerfactory;

		public NovibetDbContext(DbContextOptions<NovibetDbContext> options, ILoggerFactory loggerfactory)
			: base(options)
			=> loggerFactory = loggerfactory;

		public DbSet<IpDetails> IpDetails { get; private set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder?.IsConfigured ?? false)
			{
				optionsBuilder.UseSqlServer(
					"Server=(LocalDb)\\MSSQLLocalDB;Database=Novibet;Trusted_Connection=True;MultipleActiveResultSets=true");
			}

			optionsBuilder?.UseLoggerFactory(loggerFactory);

			base.OnConfiguring(optionsBuilder);
		}
	}
}
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
