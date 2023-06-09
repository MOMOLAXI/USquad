﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace UniverseEngine.Editor
{
	[Serializable]
	public class AssetBundleCollectorGroup
	{
		/// <summary>
		/// 分组名称
		/// </summary>
		[XmlAttribute]
		public string GroupName = string.Empty;

		/// <summary>
		/// 分组描述
		/// </summary>
		[XmlAttribute]
		public string GroupDesc = string.Empty;

		/// <summary>
		/// 资源分类标签
		/// </summary>
		[XmlAttribute]
		public string AssetTags = string.Empty;

		/// <summary>
		/// 分组激活规则
		/// </summary>
		[XmlAttribute]
		public string ActiveRuleName = nameof(EnableGroup);

		/// <summary>
		/// 分组的收集器列表
		/// </summary>
		public List<AssetBundleCollector> Collectors = new();
		
		/// <summary>
		/// 检测配置错误
		/// </summary>
		public void CheckConfigError()
		{
			if (!AssetCollectModule.HasRuleName<IActiveRule>(ActiveRuleName))
			{
				throw new($"Invalid {nameof(IActiveRule)} class type : {ActiveRuleName} in group : {GroupName}");
			}

			foreach (AssetBundleCollector collector in Collectors)
			{
				collector.CheckConfigError();
			}
		}

		/// <summary>
		/// 修复配置错误
		/// </summary>
		public bool FixConfigError()
		{
			bool isFixed = false;
			foreach (AssetBundleCollector collector in Collectors)
			{
				if (collector.FixConfigError())
				{
					isFixed = true;
				}
			}
			return isFixed;
		}

		/// <summary>
		/// 获取打包收集的资源文件
		/// </summary>
		public List<CollectAssetInfo> GetAllCollectAssets(CollectCommand command)
		{
			Dictionary<string, CollectAssetInfo> result = new(10000);

			// 检测分组是否激活
			IActiveRule activeRule = AssetCollectModule.GetRuleInstance<IActiveRule>(ActiveRuleName);
			if (activeRule.IsActive() == false)
			{
				return new();
			}

			// 收集打包资源
			foreach (AssetBundleCollector collector in Collectors)
			{
				List<CollectAssetInfo> temper = collector.GetAllCollectAssets(command, this);
				foreach (CollectAssetInfo assetInfo in temper)
				{
					if (!result.ContainsKey(assetInfo.AssetPath))
					{
						result.Add(assetInfo.AssetPath, assetInfo);
					}
					else
					{
						throw new($"The collecting asset file is existed : {assetInfo.AssetPath} in group : {GroupName}");
					}
				}
			}

			// 检测可寻址地址是否重复
			if (command.EnableAddressable)
			{
				Dictionary<string, string> addressTemper = new();
				foreach (KeyValuePair<string, CollectAssetInfo> collectInfoPair in result)
				{
					if (collectInfoPair.Value.CollectorType == ECollectorType.MainAssetCollector)
					{
						string address = collectInfoPair.Value.Address;
						string assetPath = collectInfoPair.Value.AssetPath;
						if (addressTemper.TryGetValue(address, out string existed) == false)
							addressTemper.Add(address, assetPath);
						else
							throw new($"The address is existed : {address} in group : {GroupName} \nAssetPath:\n     {existed}\n     {assetPath}");
					}
				}
			}

			// 返回列表
			return result.Values.ToList();
		}
	}
}
