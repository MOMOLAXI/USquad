using System;
using System.Collections.Generic;
using UnityEditor;

namespace UniverseEngine.Editor
{
	public class AssetProcessor : AssetPostprocessor
	{
		public static event Action<string> OnImportAsset
		{
			add => s_ImportAssets.OnProcess += value;
			remove => s_ImportAssets.OnProcess -= value;
		}
		public static event Action<IList<string>> OnImportAssets
		{
			add => s_ImportAssets.OnProcessAll += value;
			remove => s_ImportAssets.OnProcessAll -= value;
		}

		public static event Action<string> OnDeleteAsset
		{
			add => s_DeletedAssets.OnProcess += value;
			remove => s_DeletedAssets.OnProcess -= value;
		}
		public static event Action<IList<string>> OnDeleteAssets
		{
			add => s_DeletedAssets.OnProcessAll += value;
			remove => s_DeletedAssets.OnProcessAll -= value;
		}

		public static event Action<string> OnMoveAsset
		{
			add => s_MovedAssets.OnProcess += value;
			remove => s_MovedAssets.OnProcess -= value;
		}
		public static event Action<IList<string>> OnMoveAssets
		{
			add => s_MovedAssets.OnProcessAll += value;
			remove => s_MovedAssets.OnProcessAll -= value;
		}

		public static event Action<string> OnMoveFromAsset
		{
			add => s_MovedFromAssets.OnProcess += value;
			remove => s_MovedFromAssets.OnProcess -= value;
		}
		public static event Action<IList<string>> OnMoveFromAssets
		{
			add => s_MovedFromAssets.OnProcessAll += value;
			remove => s_MovedFromAssets.OnProcessAll -= value;
		}

		static readonly AssetProcessor<string> s_ImportAssets = new();
		static readonly AssetProcessor<string> s_DeletedAssets = new();
		static readonly AssetProcessor<string> s_MovedAssets = new();
		static readonly AssetProcessor<string> s_MovedFromAssets = new();

		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			s_ImportAssets.Process(importedAssets);
			s_DeletedAssets.Process(deletedAssets);
			s_MovedAssets.Process(movedAssets);
			s_MovedFromAssets.Process(movedFromAssetPaths);
		}
	}

	public class AssetProcessor<T>
	{
		public event Action<T> OnProcess;
		public event Action<IList<T>> OnProcessAll;

		public void Process(IList<T> assets)
		{
			if (Collections.IsNullOrEmpty(assets))
			{
				return;
			}

			OnProcessAll?.Invoke(assets);

			foreach (T asset in assets)
			{
				OnProcess?.Invoke(asset);
			}
		}
	}
}
