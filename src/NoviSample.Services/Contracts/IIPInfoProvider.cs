namespace Kritikos.NoviSample.Services.Contracts
{
	using System.Diagnostics.CodeAnalysis;

	[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Assigment provided interface")]
	public interface IIPInfoProvider
	{
		IPDetails GetDetails(string ip);
	}
}
