using System.Collections.Generic;

namespace UniverseEngine.Editor
{
	public abstract class UniverseEditorModule<T> : UniverseEditorModule where T : UniverseManagedSetting
	{
		public static T Setting => SettingModule.GetSetting<T>();
	}
	
	public abstract class UniverseEditorModule
	{
		public static T Create<T>() where T : UniverseEditorModule, new()
		{
			return new();
		}

		protected UniverseEditorModule() { }

		public void Init()
		{
			InitAssetProcessors();
			OnInit();
		}

		public void Update()
		{
			OnUpdate();
		}

		public void ReloadAssembly()
		{
			OnReloadAssembly();
		}

		protected virtual void OnInit() { }

		protected virtual void OnUpdate() { }

		protected virtual void OnReloadAssembly() { }

		protected virtual void OnMoveFromAsset(string asset) { }
		protected virtual void OnMoveFromAssets(IList<string> assets) { }

		protected virtual void OnMoveAssets(IList<string> assets) { }

		protected virtual void OnMoveAsset(string asset) { }

		protected virtual void OnImportAssets(IList<string> assets) { }

		protected virtual void OnImportAsset(string asset) { }

		protected virtual void OnDeleteAssets(IList<string> assets) { }

		protected virtual void OnDeleteAsset(string asset) { }

		void InitAssetProcessors()
		{
			AssetProcessor.OnDeleteAsset -= OnDeleteAssetProcess;
			AssetProcessor.OnDeleteAsset += OnDeleteAssetProcess;

			AssetProcessor.OnDeleteAssets -= OnDeleteAssetsProcess;
			AssetProcessor.OnDeleteAssets += OnDeleteAssetsProcess;

			AssetProcessor.OnImportAsset -= OnImportAssetProcess;
			AssetProcessor.OnImportAsset += OnImportAssetProcess;

			AssetProcessor.OnImportAssets -= OnImportAssetsProcess;
			AssetProcessor.OnImportAssets += OnImportAssetsProcess;

			AssetProcessor.OnMoveAsset -= OnMoveAssetProcess;
			AssetProcessor.OnMoveAsset += OnMoveAssetProcess;

			AssetProcessor.OnMoveAssets -= OnMoveAssetsProcess;
			AssetProcessor.OnMoveAssets += OnMoveAssetsProcess;

			AssetProcessor.OnMoveFromAsset -= OnMoveFromAssetProcess;
			AssetProcessor.OnMoveFromAsset += OnMoveFromAssetProcess;

			AssetProcessor.OnMoveFromAssets -= OnMoveFromAssetsProcess;
			AssetProcessor.OnMoveFromAssets += OnMoveFromAssetsProcess;
		}

		void OnMoveFromAssetProcess(string asset)
		{
			OnMoveFromAsset(asset);
		}

		void OnMoveFromAssetsProcess(IList<string> assets)
		{
			OnMoveFromAssets(assets);
		}

		void OnMoveAssetsProcess(IList<string> assets)
		{
			OnMoveAssets(assets);
		}

		void OnMoveAssetProcess(string asset)
		{
			OnMoveAsset(asset);
		}

		void OnImportAssetsProcess(IList<string> assets)
		{
			OnImportAssets(assets);
		}

		void OnImportAssetProcess(string asset)
		{
			OnImportAsset(asset);
		}

		void OnDeleteAssetsProcess(IList<string> assets)
		{
			OnDeleteAssets(assets);
		}

		void OnDeleteAssetProcess(string asset)
		{
			OnDeleteAsset(asset);
		}
	}
}
