namespace Kritikos.NoviSample.Api.Helpers
{
	public static class LogMessages
	{
		public const string UnhandledException = "An unhandled exception has occured! {Description}";

		public const string MigrationsApplied = "Successfuly applied migrations: {@MigrationList}";

		public const string RemovingIpFromCacheRequested = "Requested removal of {Ip} from cache!";

		public const string RemovalOfNonExistingCacheItemRequested =
			"Requested removal of {Ip} from cache, no such address in cache though!";

		public const string RequestedInfoForIp = "Requested {Ip} info, cache hit!";

		public const string RequestedInfoForIpNoCache = "Requested {Ip} info, cache miss!";

		public const string RequestedInfoForIpNoPersistence = "Requested {Ip} info, database miss!";

		public const string InvalidIpFormat = "Requested IP information for invalid ip format: {Ip}";

		public const string InternalIp = "Requested IP information for private ip adress range: {Ip}";

		public const string InvalidIpResponse = "Unable to serialize data to IpDetails: {Reason}";
	}
}
