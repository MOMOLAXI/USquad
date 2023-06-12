using UnityEditor;
using UnityEngine;

namespace UniverseEngine.Editor
{
	public static class AssetBundleInspector
	{
		[CustomEditor(typeof(AssetBundle))]
		internal class AssetBundleEditor : UnityEditor.Editor
		{
			internal bool PathFoldout;
			internal bool AdvancedFoldout;
			
			public override void OnInspectorGUI()
			{
				AssetBundle bundle = target as AssetBundle;

				using (new EditorGUI.DisabledScope(true))
				{
					GUIStyle leftStyle = new(GUI.skin.GetStyle("Label"))
					{
						alignment = TextAnchor.UpperLeft
					};
					GUILayout.Label(new GUIContent("Name: " + bundle.name), leftStyle);

					string[] assetNames = bundle.GetAllAssetNames();
					PathFoldout = EditorGUILayout.Foldout(PathFoldout, "Source Asset Paths");
					if (PathFoldout)
					{
						EditorGUI.indentLevel++;
						foreach (string asset in assetNames)
							EditorGUILayout.LabelField(asset);
						EditorGUI.indentLevel--;
					}

					AdvancedFoldout = EditorGUILayout.Foldout(AdvancedFoldout, "Advanced Data");
				}

				if (AdvancedFoldout)
				{
					EditorGUI.indentLevel++;
					base.OnInspectorGUI();
					EditorGUI.indentLevel--;
				}
			}
		}
	}
}