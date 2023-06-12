using System;

namespace UniverseEngine
{
	internal class DeserializeManifestOperation : AsyncOperationBase
	{
		readonly BufferReader m_Buffer;

		/// <summary>
		/// 解析的清单实例
		/// </summary>
		public PackageManifest Manifest { private set; get; }

		public DeserializeManifestOperation(byte[] binaryData)
		{
			m_Buffer = new(binaryData);
		}
		internal override void Start()
		{
			Deserialize().Forget();
		}

		internal override void Update()
		{

		}

		async UniTaskVoid Deserialize()
		{
			try
			{
				Manifest = new();
				(bool result, string error) = await Manifest.ReadFromBufferAsync(m_Buffer);
				if (!result)
				{
					Status = EOperationStatus.Failed;
					Error = error;
				}
				else
				{
					Status = EOperationStatus.Succeed;
					Error = string.Empty;
				}
			}
			catch (Exception exception)
			{
				Manifest = null;
				Status = EOperationStatus.Failed;
				Error = exception.Message;
			}
		}
	}
}
