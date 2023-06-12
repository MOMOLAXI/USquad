using System;
using System.IO;
using UnityEngine;

namespace UniverseEngine
{
	public partial class FileSystem
	{
		static string s_BuiltInPath;

		static readonly Lazy<string> s_BuiltInRoot = new(() =>
		{
			if (string.IsNullOrEmpty(s_BuiltInPath))
			{
				s_BuiltInPath = StringUtilities.Format(FILE_PATH_FORMAT_2, Application.streamingAssetsPath, STREAMING_ASSETS_BUILTIN_FOLDER);
			}

			return s_BuiltInPath;
		});

		static string s_CacheSystemPath;

		static readonly Lazy<string> s_CacheSystemRoot = new(() =>
		{
			if (!string.IsNullOrEmpty(s_CacheSystemPath))
			{
				return s_CacheSystemPath;
			}

			if (IsEditorPlatform())
			{
				string directory = Path.GetDirectoryName(Application.dataPath);
				string projectPath = GetRegularPath(directory);
				s_CacheSystemPath = StringUtilities.Format(CACHE_SYSTEM_PATH_FORMAT, projectPath);
				return s_CacheSystemPath;
			}

			s_CacheSystemPath = StringUtilities.Format(CACHE_SYSTEM_PATH_FORMAT, Application.persistentDataPath);
			return s_CacheSystemPath;
		});


		/// <summary>
		/// 获取沙盒文件夹路径
		/// </summary>
		public static string GetPersistentRootPath() => s_CacheSystemRoot.Value;

		/// <summary>
		/// 获取包裹的版本文件完整名称
		/// </summary>
		public static string GetPackageVersionFileName(string packageName)
		{
			return StringUtilities.Format(FILE_VERSION_NAME_FORMAT, UniverseConstant.MANIFEST_FILE_HEADER, packageName);
		}

		/// <summary>
		/// 获取构建报告文件名
		/// </summary>
		public static string GetReportFileName(string packageName, string packageVersion)
		{
			return StringUtilities.Format(FILE_REPORT_NAME_FORMAT, UniverseConstant.REPORT_FILE_NAME, packageName, packageVersion);
		}

		/// <summary>
		/// 获取清单文件完整名称
		/// </summary>
		public static string GetManifestBinaryFileName(string packageName, string packageVersion)
		{
			return StringUtilities.Format(FILE_MANIFEST_BINRAY_NAME_FORMAT, UniverseConstant.MANIFEST_FILE_HEADER, packageName, packageVersion);
		}

		/// <summary>
		/// 获取清单文件完整名称
		/// </summary>
		public static string GetManifestJsonFileName(string packageName, string packageVersion)
		{
			return StringUtilities.Format(FILE_MANIFEST_JSON_NAME_FORMAT, UniverseConstant.MANIFEST_FILE_HEADER, packageName, packageVersion);
		}

		/// <summary>
		/// 获取包裹的哈希文件完整名称
		/// </summary>
		public static string GetPackageHashFileName(string packageName, string packageVersion)
		{
			return StringUtilities.Format(FILE_MANIFEST_HASH_NAME_FORMAT, UniverseConstant.MANIFEST_FILE_HEADER, packageName, packageVersion);
		}

		/// <summary>
		/// 获取基于流文件夹的加载路径
		/// </summary>
		public static string ToStreamingLoadPath(string path)
		{
			return Path.Combine(s_BuiltInRoot.Value, path);
		}

		/// <summary>
		/// 获取基于沙盒文件夹的加载路径
		/// </summary>
		public static string ToPersistentLoadPath(string path)
		{
			return Path.Combine(s_CacheSystemRoot.Value, path);
		}

		/// <summary>
		/// 获取WWW加载本地资源的路径
		/// </summary>
		public static string ToWWWPath(string path)
		{
			if (IsEditorPlatform())
			{
				return StringUtilities.Format("file:///{0}", path);
			}

			if (IsIPhonePlatform())
			{
				return StringUtilities.Format("file://{0}", path);
			}

			if (IsAndroidPlatform())
			{
				return path;
			}

			if (IsStandAlonePlatform())
			{
				return StringUtilities.Format("file:///{0}", path);
			}

			if (IsWebGLPlatform())
			{
				return path;
			}

			return path;
		}

		public static bool IsEditorPlatform()
		{
			RuntimePlatform platform = Application.platform;
			return platform is RuntimePlatform.WindowsEditor or RuntimePlatform.LinuxEditor or RuntimePlatform.OSXEditor;
		}

		public static bool IsIPhonePlatform()
		{
			RuntimePlatform platform = Application.platform;
			return platform == RuntimePlatform.IPhonePlayer;
		}

		public static bool IsAndroidPlatform()
		{
			RuntimePlatform platform = Application.platform;
			return platform == RuntimePlatform.Android;
		}

		public static bool IsStandAlonePlatform()
		{
			RuntimePlatform platform = Application.platform;
			return platform is RuntimePlatform.WindowsPlayer or RuntimePlatform.LinuxPlayer or RuntimePlatform.OSXPlayer;
		}

		public static bool IsWebGLPlatform()
		{
			RuntimePlatform platform = Application.platform;
			return platform == RuntimePlatform.WebGLPlayer;
		}

		/// <summary>
		/// 额外封装，避免外部使用range indexer写法的boxing
		/// </summary>
		/// <param name="path"></param>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static string InterceptPath(string path, int from, int to)
		{
			if (string.IsNullOrEmpty(path))
			{
				return string.Empty;
			}

			return path.Substring(from, to);
		}

		public static string GetParent(string path)
		{
			int index = path.Replace('\\', '/').LastIndexOf('/');
			string parentPath = InterceptPath(path, 0, index);
			return index >= 0 ? parentPath : ".";
		}

		/// <summary>
		/// 获取项目工程路径
		/// </summary>
		public static string GetProjectPath()
		{
			string projectPath = Path.GetDirectoryName(Application.dataPath);
			return GetRegularPath(projectPath);
		}

		/// <summary>
		/// 获取默认的输出根路录
		/// </summary>
		public static string GetBundleOutputDirectory()
		{
			string projectPath = GetProjectPath();
			return StringUtilities.Format(BUNDLE_OUTPUT_DIRECTORY_FORMAT, projectPath);
		}

		/// <summary>
		/// 替换为Linux路径格式
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string GetRegularPath(string path)
		{
			return path.Replace('\\', '/').Replace("\\", "/");
		}

		public static string Standardize(string path)
		{
			return path.Replace('\\', '/');
		}

		public static string StandardizePathCombine(string parent, string sub)
		{
			return Standardize(Path.Combine(parent, sub));
		}
	}
}
