using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniverseEngine.Editor
{
	public class SettingModule : UniverseEditorModule
	{
		static bool s_IsDirty;

		static readonly Lazy<string> s_RootPath = new(() => UniverseEditor.GetEditorDirectory("Settings"));

		static readonly Dictionary<string, Type> s_SettingPaths = new();
		static readonly Dictionary<Type, UniverseManagedSetting> s_Settings = new();

		protected override void OnInit()
		{
			InitializeSettings(s_Settings, LoadSetting);
		}

		protected override void OnUpdate()
		{
			if (s_IsDirty)
			{
				ForEachSetting(setting =>
				{
					if (EditorUtility.IsDirty(setting))
					{
						EditorLog.Info($"Save setting : {setting.name}");
						AssetDatabase.SaveAssetIfDirty(setting);
					}
				});
				s_IsDirty = false;
			}
		}

		protected override void OnDeleteAsset(string asset)
		{
			
			if (!s_SettingPaths.TryGetValue(asset, out Type settingType))
			{
				return;
			}

			if (s_Settings.ContainsKey(settingType))
			{
				EditorLog.Info($"{settingType.Name} Removed from {nameof(SettingModule)}");
				s_Settings.Remove(settingType);
				s_SettingPaths.Remove(asset);
			}
		}

		public static T GetSetting<T>() where T : UniverseManagedSetting
		{
			return GetSetting(typeof(T)) as T;
		}

		public static UniverseManagedSetting GetSetting(Type type)
		{
			if (s_Settings.TryGetValue(type, out UniverseManagedSetting setting))
			{
				return setting;
			}

			setting = LoadSetting(type);
			s_Settings[type] = setting;
			return setting;
		}

		static void ForEachSetting(Action<UniverseManagedSetting> onForeach)
		{
			foreach (UniverseManagedSetting setting in s_Settings.Values)
			{
				onForeach?.Invoke(setting);
			}
		}

		static void InitializeSettings<TBaseClass>(IDictionary<Type, TBaseClass> settings, Func<Type, TBaseClass> loadFunc) where TBaseClass : class
		{
			if (loadFunc == null)
			{
				return;
			}

			s_SettingPaths.Clear();
			using (Collections.AllocAutoList(out List<Type> result))
			{
				ReflectionUtilities.GetTypesDerivedFrom<TBaseClass>(result);
				foreach (Type type in result)
				{
					settings[type] = loadFunc.Invoke(type);
				}
			}
		}

		static UniverseManagedSetting LoadSetting(Type settingType)
		{
			string filePath = FileSystem.ToPath(s_RootPath.Value, FileSystem.ToFile(settingType.Name, "asset"));
			if (!File.Exists(filePath))
			{
				ScriptableObject setting = ScriptableObject.CreateInstance(settingType);
				if (setting is UniverseManagedSetting targetSetting)
				{
					EditorLog.Info($"Create Setting : {filePath}");
					targetSetting.MarkDirty = SetDirty;
					s_SettingPaths[filePath] = settingType;
					AssetDatabase.CreateAsset(setting, filePath);
					AssetDatabase.SaveAssets();
					return targetSetting;
				}

				throw new($"Setting Type {settingType.FullName} did not match : {typeof(UniverseManagedSetting).FullName}");
			}

			UniverseManagedSetting loadedSetting = AssetDatabase.LoadAssetAtPath(filePath, settingType) as UniverseManagedSetting;
			if (loadedSetting != null)
			{
				loadedSetting.MarkDirty = SetDirty;
				s_SettingPaths[filePath] = settingType;
			}

			return loadedSetting;
		}

		static void SetDirty(UniverseManagedSetting setting)
		{
			EditorUtility.SetDirty(setting);
			s_IsDirty = true;
		}
	}
}
