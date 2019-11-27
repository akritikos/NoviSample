namespace Kritikos.NoviSample.Api
{
	using Kritikos.NoviSample.Persistence;

	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Logging;

	[ApiController]
	[Route("api/ip")]
	public class IpController : Controller
	{
		public IpController(ILogger<IpController> logger, NovibetDbContext dbContext)
		{
			Logger = logger;
			Context = dbContext;
		}

		private NovibetDbContext Context { get; }

		private ILogger<IpController> Logger { get; }

		[HttpGet("")]
		public ActionResult Index()
		{
			Logger.LogInformation("Accessed!");
			return Ok();
		}
	}
}
