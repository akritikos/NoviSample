namespace Kritikos.NoviSample.HostedServices.Implementations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;

	using Kritikos.NoviSample.HostedServices.Contracts;
	using Kritikos.NoviSample.Services.Models;

	public class InMemoryBackgroundIpQueue : IInMemoryBackgroundIpQueue
	{
		private Dictionary<string, List<string>> QueuedItems { get; }
			= new Dictionary<string, List<string>>();

		private Dictionary<string, List<IpDetailResponse>> CompletedItems { get; }
			= new Dictionary<string, List<IpDetailResponse>>();

		public void QueueBackgroundWorkItem((string identifier, List<string> addresses) batch)
			=> QueuedItems.Add(batch.identifier, batch.addresses);

		public Task<List<(string Identifier, string address)>> DequeueAsync(CancellationToken cancellationToken)
		{
			var selectedItems = 0;
			var toDequeue = QueuedItems.SelectMany(x => x.Value.Select(y =>
				{
					selectedItems++;
					return (Identifier: x.Key, address: y, count: selectedItems);
				}))
				.TakeWhile(x => x.count <= 10)
				.Select(x => (x.Identifier, x.address))
				.ToList();
			return Task.FromResult(toDequeue);
		}

		public void MarkCompleted(List<(string identifier, IpDetailResponse response)> completed)
		{
			if (completed == null)
			{
				throw new ArgumentNullException(nameof(completed));
			}

			foreach (var (identifier, response) in completed)
			{
				QueuedItems[identifier].Remove(response.Ip);
				if (!CompletedItems.ContainsKey(identifier))
				{
					CompletedItems.Add(identifier, new List<IpDetailResponse>());
				}

				CompletedItems[identifier].Add(response);

				if (QueuedItems[identifier].Count == 0)
				{
					QueuedItems.Remove(identifier);
					CompletedItems.Remove(identifier);
				}
			}
		}

		public (int? remaining, int? done) GetProgressOfBatch(string identifier)
		{
			var remaining = QueuedItems.ContainsKey(identifier)
				? QueuedItems[identifier]
				: null;
			var done = CompletedItems.ContainsKey(identifier)
				? CompletedItems[identifier]
				: null;

			return (remaining?.Count, done?.Count);
		}
	}
}
