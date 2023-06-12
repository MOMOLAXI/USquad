namespace UniverseEngine
{
	public readonly struct PackageManifestInfo
	{
		public readonly string PackageName;
		public readonly string DefaultHostServer;
		public readonly string FallbackHostServer;

		public PackageManifestInfo(string packageName, string defaultHostServer, string fallbackHostServer)
		{
			PackageName = packageName;
			DefaultHostServer = defaultHostServer;
			FallbackHostServer = fallbackHostServer;
		}
	}
}
