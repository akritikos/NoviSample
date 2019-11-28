namespace Kritikos.NoviSample.HostedServices.Contracts
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	using Kritikos.NoviSample.Persistence;
	using Kritikos.NoviSample.Services.Models;

	public interface IInMemoryBackgroundIpQueue
	{
		void QueueBackgroundWorkItem((string identifier, List<string> addresses) batch);

		Task<List<(string Identifier, string address)>> DequeueAsync(
			CancellationToken cancellationToken);

		void MarkCompleted(List<(string identifier, IpDetailResponse response)> completed);

		(int? remaining, int? done) GetProgressOfBatch(string identifier);
	}
}
