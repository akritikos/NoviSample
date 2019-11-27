namespace Kritikos.NoviSample.Services.Contracts
{
	using System.Diagnostics.CodeAnalysis;
	using System.Threading.Tasks;

	[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Assigment provided interface")]
	public interface IIPInfoProvider
	{
		IPDetails GetDetails(string ip);

		Task<IPDetails> GetDetailsAsync(string ip) => Task.FromResult(GetDetails(ip));
	}
}
