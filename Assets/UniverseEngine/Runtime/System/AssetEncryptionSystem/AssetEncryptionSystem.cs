using System;
using System.Collections.Generic;
using System.Linq;

namespace UniverseEngine
{
	public enum EAssetEncryption
	{
		None = 0,
		ByOffset,
		ByStream
	}

	public enum EAssetDecryption
	{
		Default,
	}

	public class AssetEncryptionSystem : EngineSystem
	{
		static readonly Dictionary<EAssetEncryption, IAssetEncryption> s_Encryptions = new()
		{
			{ EAssetEncryption.None, new AssetNoneEncryption() },
			{ EAssetEncryption.ByOffset, new AssetOffsetEncryption() },
			{ EAssetEncryption.ByStream, new AssetStreamEncryption() },
		};

		static readonly Dictionary<EAssetDecryption, IAssetDecryption> s_Decryptions = new()
		{
			{ EAssetDecryption.Default, new AssetDefaultDecryption() }
		};

		public static void GetEncryptionNames(List<string> result)
		{
			if (result == null)
			{
				return;
			}

			result.Clear();
			result.AddRange(s_Encryptions.Keys.Select(type => type.ToString()));
		}

		public static IAssetDecryption GetDecryption(EAssetDecryption type)
		{
			if (s_Decryptions.TryGetValue(type, out IAssetDecryption encryption))
			{
				return encryption;
			}

			return s_Decryptions[EAssetDecryption.Default];
		}

		public static IAssetEncryption GetEncryption(EAssetEncryption type)
		{
			if (s_Encryptions.TryGetValue(type, out IAssetEncryption encryption))
			{
				return encryption;
			}

			return s_Encryptions[EAssetEncryption.None];
		}
	}
}
