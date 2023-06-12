namespace UniverseEngine.Editor
{
	public readonly struct PackRuleResult
	{
		readonly string m_BundleName;
		readonly string m_BundleExtension;

		public PackRuleResult(string bundleName, string bundleExtension)
		{
			m_BundleName = bundleName;
			m_BundleExtension = bundleExtension;
		}

		/// <summary>
		/// 获取主资源包全名称
		/// </summary>
		public string GetMainBundleName(string packageName, bool uniqueBundleName)
		{
			string bundleName = FileSystem.GetRegularPath(m_BundleName).Replace('/', '_').Replace('.', '_').ToLower();
			string fullName = uniqueBundleName ? $"{packageName}_{bundleName}.{m_BundleExtension}" : $"{bundleName}.{m_BundleExtension}";
			return fullName.ToLower();
		}

		/// <summary>
		/// 获取共享资源包全名称
		/// </summary>
		public string GetShareBundleName(string packageName, bool uniqueBundleName)
		{
			string bundleName = FileSystem.GetRegularPath(m_BundleName).Replace('/', '_').Replace('.', '_').ToLower();
			string fullName = uniqueBundleName ? $"{packageName}_share_{bundleName}.{m_BundleExtension}" : $"share_{bundleName}.{m_BundleExtension}";
			return fullName.ToLower();
		}
	}
}
