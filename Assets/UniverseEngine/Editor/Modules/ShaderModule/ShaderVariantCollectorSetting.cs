﻿namespace UniverseEngine.Editor
{
	public class ShaderVariantCollectorSetting : UniverseManagedSetting
	{
		/// <summary>
		/// 文件存储路径
		/// </summary>
		public string SavePath = "Assets/UniverseResource/Shader/UniverseShaderVariants.shadervariants";

		/// <summary>
		/// 收集的包裹名称
		/// </summary>
		public string CollectPackage = string.Empty;

		/// <summary>
		/// 容器值
		/// </summary>
		public int ProcessCapacity = 1000;
	}
}