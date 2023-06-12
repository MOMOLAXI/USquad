namespace UniverseEngine.Editor
{
	public class TaskEncryption : IBuildTask
	{
		public string GetDisplayName() => "资源包加密";

		public EBuildMode[] IgnoreBuildModes => new[]
		{
			EBuildMode.SimulateBuild, EBuildMode.DryRunBuild
		};

		void IBuildTask.Run(BuildContext context)
		{
			BuildParametersContext buildParameters = context.GetContextObject<BuildParametersContext>();
			BuildMapContext buildMapContext = context.GetContextObject<BuildMapContext>();
			EncryptingBundleFiles(buildParameters, buildMapContext);
		}

		/// <summary>
		/// 加密文件
		/// </summary>
		static void EncryptingBundleFiles(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext)
		{
			IAssetEncryption encryptionServices = AssetEncryptionSystem.GetEncryption(buildParametersContext.Arguments.EncryptionType);
			if (encryptionServices == null)
				return;

			if (encryptionServices.GetType() == typeof(AssetNoneEncryption))
			{
				return;
			}

			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			foreach (BuildBundleInfo bundleInfo in buildMapContext.BundleCollection)
			{
				AssetBundleEncryptFileInfo fileInfo = new()
				{
					BundleName = bundleInfo.BundleName,
					FilePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}"
				};

				AssetBundleEncryptResult encryptResult = encryptionServices.Encrypt(fileInfo);
				if (encryptResult.LoadMethod != EBundleLoadMethod.Normal)
				{
					// 注意：原生文件不支持加密
					if (bundleInfo.IsRawFile)
					{
						Log<AssetBuildModule>.Warning($"Encryption not support raw file : {bundleInfo.BundleName}");
						continue;
					}

					string filePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}.encrypt";
					FileSystem.CreateFile(filePath, encryptResult.EncryptedData);
					bundleInfo.EncryptedFilePath = filePath;
					bundleInfo.LoadMethod = encryptResult.LoadMethod;
					Log<AssetBuildModule>.Info($"Bundle文件加密完成：{filePath}");
				}

			}
		}
	}
}
