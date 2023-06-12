using System;

namespace UniverseEngine
{
	public readonly struct AssetInfo
	{
		/// <summary>
		/// 资源路径
		/// </summary>
		public readonly string AssetPath;

		/// <summary>
		/// 唯一标识符
		/// </summary>
		public readonly string Guid;

		/// <summary>
		/// 资源类型
		/// </summary>
		public readonly Type AssetType;

		/// <summary>
		/// 错误信息
		/// </summary>
		public readonly string Error;

		/// <summary>
		/// 身份是否无效
		/// </summary>
		public readonly bool IsInvalid;

		/// <summary>
		/// 可寻址地址
		/// </summary>
		public readonly string Address;

		internal AssetInfo(Asset asset, Type assetType)
		{
			Error = string.Empty;
			IsInvalid = asset == null;
			AssetType = assetType;
			AssetPath = asset.AssetPath;
			Address = asset.Address;
			Guid = AssetType == null ? $"{AssetPath}[null]" : $"{AssetPath}[{AssetType.Name}]";
		}

		internal AssetInfo(Asset asset)
		{
			Error = string.Empty;
			IsInvalid = asset == null;
			AssetType = null;
			AssetPath = asset.AssetPath;
			Address = asset.Address;
			Guid = $"{AssetPath}[null]";
		}

		internal AssetInfo(string error)
		{
			Error = error;
			IsInvalid = true;
			AssetType = null;
			AssetPath = string.Empty;
			Address = string.Empty;
			Guid = string.Empty;
		}
	}
}
