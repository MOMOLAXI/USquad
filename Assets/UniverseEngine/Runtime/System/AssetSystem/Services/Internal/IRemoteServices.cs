
namespace UniverseEngine
{
	internal interface IRemoteServices
	{
		string GetRemoteMainURL(string fileName);
		string GetRemoteFallbackURL(string fileName);
	}
}