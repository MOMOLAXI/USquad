using System.Collections.Generic;

namespace UniverseEngine.Editor
{
	public struct CollectResult
	{
		/// <summary>
		/// 收集命令
		/// </summary>
		public CollectCommand Command { private set; get; }

		/// <summary>
		/// 收集的资源信息列表
		/// </summary>
		public List<CollectAssetInfo> CollectAssets { private set; get; }

		public CollectResult(CollectCommand command)
		{
			Command = command;
			CollectAssets = new();
		}
		public void SetCollectAssets(List<CollectAssetInfo> collectAssets)
		{
			CollectAssets.Clear();
			CollectAssets.AddRange(collectAssets);
		}
	}
}
