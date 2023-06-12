using System;
using UnityEditor;
using UnityEngine;

namespace UniverseEngine.Editor
{
	[DisplayName("收集精灵类型的纹理")]
	public class CollectSprite : IFilterRule
	{
		public bool IsCollectAsset(FilterRuleData data)
		{
			Type mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(data.AssetPath);
			if (mainAssetType == typeof(Texture2D))
			{
				TextureImporter texImporter = AssetImporter.GetAtPath(data.AssetPath) as TextureImporter;
				if (texImporter != null && texImporter.textureType == TextureImporterType.Sprite)
					return true;
				else
					return false;
			}
			else
			{
				return false;
			}
		}
	}
}
