using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace UniverseEngine.Editor
{
	public class AssetCollectModule : UniverseEditorModule<AssetBundleCollectorSetting>
	{
		/// <summary>
		/// 忽略的文件类型
		/// </summary>
		static readonly HashSet<string> s_IgnoreFileExtensions = new()
		{
			"",
			".so", ".dll",
			".cs", ".js",
			".boo",
			".meta", ".cginc", ".hlsl"
		};

		/// <summary>
		/// AssetBundle文件的后缀名
		/// </summary>
		public const string ASSET_BUNDLE_FILE_EXTENSION = "bundle";

		/// <summary>
		/// 原生文件的后缀名
		/// </summary>
		public const string RAW_FILE_EXTENSION = "rawfile";

		/// <summary>
		/// Unity着色器资源包名称
		/// </summary>
		public const string SHADERS_BUNDLE_NAME = "unityshaders";

		static readonly Dictionary<Type, AssetBundleCollectRule> s_CollectRules = new();
		static readonly Dictionary<EShareAssetPackRule, IShareAssetPackRule> s_ShareAssetPackRules = new()
		{
			{ EShareAssetPackRule.Default, new DefaultShareAssetPackRule() }
		};

		protected override void OnInit()
		{
			RegisterCollectRule<IPackRule>();
			RegisterCollectRule<IFilterRule>();
			RegisterCollectRule<IAddressRule>();
			RegisterCollectRule<IActiveRule>();
		}

		public static IShareAssetPackRule GetShareAssetPackRule(EShareAssetPackRule rule)
		{
			if (s_ShareAssetPackRules.TryGetValue(rule, out IShareAssetPackRule shareAssetPackRule))
			{
				return shareAssetPackRule;
			}

			return s_ShareAssetPackRules[EShareAssetPackRule.Default];
		}

		public static T GetRuleInstance<T>(string ruleName) where T : class
		{
			AssetBundleCollectRule<T> container = GetCollectRule<T>();
			return container?.GetRuleInstance<T>(ruleName);
		}

		public static bool HasRuleName<T>(string ruleName) where T : class
		{
			AssetBundleCollectRule<T> container = GetCollectRule<T>();
			if (container == null)
			{
				return false;
			}

			return container.HasRuleName(ruleName);
		}

		public static RuleDisplayName GetRuleName<T>(string ruleName) where T : class
		{
			List<RuleDisplayName> names = GetRuleNames<T>();
			if (Collections.IsNullOrEmpty(names))
			{
				return RuleDisplayName.None;
			}

			foreach (RuleDisplayName name in names)
			{
				if (name.ClassName == ruleName)
				{
					return name;
				}
			}

			return RuleDisplayName.None;
		}

		public static int GetRuleIndex<T>(string ruleName) where T : class
		{
			List<RuleDisplayName> names = GetRuleNames<T>();
			if (Collections.IsNullOrEmpty(names))
			{
				return -1;
			}

			int index = names.FindIndex(name => name.ClassName == ruleName);
			if (index < 0)
			{
				index = 0;
			}

			return index;
		}

		public static List<RuleDisplayName> GetRuleNames<T>() where T : class
		{
			AssetBundleCollectRule<T> container = GetCollectRule<T>();
			if (container == null)
			{
				return Collections.EmptyList<RuleDisplayName>();
			}

			return container.GetDisplayRuleNames();
		}

		public static List<string> GetBuildPackageNames()
		{
			return Setting.Packages.Select(package => package.PackageName).ToList();
		}

		public static void FixFile()
		{
			bool isFixed = Setting.FixConfigError();
			if (isFixed)
			{
				Setting.Save();
			}
		}

		/// <summary>
		/// 清空所有数据
		/// </summary>
		public static void ClearAll()
		{
			Setting.ClearAll();
			Setting.Save();
		}

		// 公共参数编辑相关
		public static void ModifyPackageView(bool showPackageView)
		{
			Setting.ShowPackageView = showPackageView;
			Setting.Save();
		}

		public static void ModifyAddressable(bool enableAddressable)
		{
			Setting.EnableAddressable = enableAddressable;
			Setting.Save();
		}

		public static void ModifyUniqueBundleName(bool uniqueBundleName)
		{
			Setting.UniqueBundleName = uniqueBundleName;
			Setting.Save();
		}

		public static void ModifyShowEditorAlias(bool showAlias)
		{
			Setting.ShowEditorAlias = showAlias;
			Setting.Save();
		}

		// 资源包裹编辑相关
		public static AssetBundleCollectorPackage CreatePackage(string packageName)
		{
			AssetBundleCollectorPackage package = new()
			{
				PackageName = packageName
			};
			Setting.Packages.Add(package);
			Setting.Save();
			return package;
		}

		public static void RemovePackage(AssetBundleCollectorPackage package)
		{
			if (Setting.Packages.Remove(package))
			{
				Setting.Save();
			}
			else
			{
				EditorLog.Warning($"Failed remove package : {package.PackageName}");
			}
		}

		public static void ModifyPackage(AssetBundleCollectorPackage package)
		{
			if (package != null)
			{
				Setting.Save();
			}
		}

		// 资源分组编辑相关
		public static AssetBundleCollectorGroup CreateGroup(AssetBundleCollectorPackage package, string groupName)
		{
			AssetBundleCollectorGroup group = new()
			{
				GroupName = groupName
			};
			package.Groups.Add(group);
			Setting.Save();
			return group;
		}

		public static void RemoveGroup(AssetBundleCollectorPackage package, AssetBundleCollectorGroup group)
		{
			if (package.Groups.Remove(group))
			{
				Setting.Save();
			}
			else
			{
				EditorLog.Warning($"Failed remove group : {group.GroupName}");
			}
		}
		public static void ModifyGroup(AssetBundleCollectorPackage package, AssetBundleCollectorGroup group)
		{
			if (package != null && group != null)
			{
				Setting.Save();
			}
		}

		// 资源收集器编辑相关
		public static void CreateCollector(AssetBundleCollectorGroup group, AssetBundleCollector collector)
		{
			group.Collectors.Add(collector);
			Setting.Save();
		}

		public static void RemoveCollector(AssetBundleCollectorGroup group, AssetBundleCollector collector)
		{
			if (group.Collectors.Remove(collector))
			{
				Setting.Save();
			}
			else
			{
				EditorLog.Warning($"Failed remove collector : {collector.CollectPath}");
			}
		}
		public static void ModifyCollector(AssetBundleCollectorGroup group, AssetBundleCollector collector)
		{
			if (group != null && collector != null)
			{
				Setting.Save();
			}
		}

		/// <summary>
		/// 获取所有的资源标签
		/// </summary>
		public static string GetPackageAllTags(string packageName)
		{
			List<string> allTags = Setting.GetPackageAllTags(packageName);
			return string.Join(";", allTags);
		}

		public static PackRuleResult CreateShadersPackRuleResult()
		{
			PackRuleResult result = new(SHADERS_BUNDLE_NAME, ASSET_BUNDLE_FILE_EXTENSION);
			return result;
		}

		/// <summary>
		/// 导入XML配置表
		/// </summary>
		public static void ImportFromXml(string filePath)
		{
			if (!FileSystem.IsXml(filePath))
			{
				throw new($"Only support xml : {filePath}");
			}

			AssetCollectorSerializeData data = FileSystem.DeserializeFromXml(filePath, () => new AssetCollectorSerializeData());
			Setting.ShowPackageView = data.ShowPackageView;
			Setting.ShowEditorAlias = data.ShowEditorAlias;
			Setting.EnableAddressable = data.EnableAddressable;
			Setting.UniqueBundleName = data.UniqueBundleName;
			Setting.Packages = data.Packages;
			Setting.Save();
			EditorLog.Info($"导入Collector配置完成 [{filePath}]");
		}

		public static void ExportToXml(string savePath)
		{
			AssetCollectorSerializeData data = new()
			{
				ShowPackageView = Setting.ShowPackageView,
				EnableAddressable = Setting.EnableAddressable,
				ShowEditorAlias = Setting.ShowEditorAlias,
				UniqueBundleName = Setting.UniqueBundleName,
				Packages = Setting.Packages
			};

			FileSystem.SerializeToXml(data, savePath);
			EditorLog.Info($"导出Collector配置完成 [{savePath}]");
		}

		public static bool IsIgnoreFile(string fileExtension)
		{
			return s_IgnoreFileExtensions.Contains(fileExtension);
		}
		
		/// <summary>
		/// 搜集资源
		/// </summary>
		/// <param name="searchType">搜集的资源类型</param>
		/// <param name="searchInFolders">指定搜索的文件夹列表</param>
		/// <returns>返回搜集到的资源路径列表</returns>
		public static string[] FindAssets(EAssetSearchType searchType, string[] searchInFolders)
		{
			// 注意：AssetDatabase.FindAssets()不支持末尾带分隔符的文件夹路径
			for (int i = 0; i < searchInFolders.Length; i++)
			{
				string folderPath = searchInFolders[i];
				searchInFolders[i] = folderPath.TrimEnd('/');
			}

			// 注意：获取指定目录下的所有资源对象（包括子文件夹）
			string[] guids = AssetDatabase.FindAssets(searchType == EAssetSearchType.All ? string.Empty : $"t:{searchType}", searchInFolders);

			// 注意：AssetDatabase.FindAssets()可能会获取到重复的资源
			HashSet<string> result = new();
			for (int i = 0; i < guids.Length; i++)
			{
				string guid = guids[i];
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				result.Add(assetPath);
			}

			// 返回结果
			return result.ToArray();
		}

		/// <summary>
		/// 搜集资源
		/// </summary>
		/// <param name="searchType">搜集的资源类型</param>
		/// <param name="searchInFolder">指定搜索的文件夹</param>
		/// <returns>返回搜集到的资源路径列表</returns>
		public static string[] FindAssets(EAssetSearchType searchType, string searchInFolder)
		{
			return FindAssets(searchType, new[] { searchInFolder });
		}

		static AssetBundleCollectRule<T> GetCollectRule<T>() where T : class
		{
			if (s_CollectRules.TryGetValue(typeof(T), out AssetBundleCollectRule container))
			{
				return container as AssetBundleCollectRule<T>;
			}

			return null;
		}

		static void RegisterCollectRule<T>() where T : class
		{
			Type type = typeof(T);
			AssetBundleCollectRule<T> ruleContainer = new();
			ruleContainer.Init();
			s_CollectRules[type] = ruleContainer;
		}
	}
}
