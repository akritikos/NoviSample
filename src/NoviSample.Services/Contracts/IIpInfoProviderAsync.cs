namespace Kritikos.NoviSample.Services.Contracts
{
	using System.Collections.Generic;
	using System.Threading.Tasks;

	using Kritikos.NoviSample.Services.Models;

	public interface IIpInfoProviderAsync : IIPInfoProvider
	{
		Task<IpDetailResponse> GetDetailsAsync(string ip);

		Task<List<IpDetailResponse>> GetBulkDetailsAsync(string[] ipList);
	}
}
