using System.Collections.Generic;
using System.IO;

namespace UniverseEngine
{
	public partial class FileSystem
	{
		const string EMPTY = "";
		const string FILE_NAME_FORMAT = "{0}{1}";
		const string FILE_NAME_EXTENSION_FORMAT = "{0}.{1}";

		const string FILE_PATH_FORMAT_2 = "{0}/{1}";
		const string FILE_PATH_FORMAT_3 = "{0}/{1}/{2}";

		const string FILE_REMOTE_NAME_FORMAT = "{0}_{1}{2}";
		const string FILE_VERSION_NAME_FORMAT = "{0}_{1}.version";
		const string FILE_REPORT_NAME_FORMAT = "{0}_{1}_{2}.json";

		const string FILE_MANIFEST_JSON_NAME_FORMAT = "{0}_{1}_{2}.json";
		const string FILE_MANIFEST_HASH_NAME_FORMAT = "{0}_{1}_{2}.hash";
		const string FILE_MANIFEST_BINRAY_NAME_FORMAT = "{0}_{1}_{2}.bytes";


		public const int GB = 1024 * 1024 * 1024;
		public const int MB = 1024 * 1024;
		public const int KB = 1024;

		public const string EXTENSION_EXE = ".exe";
		public const string EXTENSION_DLL = ".dll";
		public const string EXTENSION_XML = ".xml";
		public const string EXTENSION_JSON = ".json";

		const string CACHED_FOLDER_NAME = "AssetCacheFiles";
		const string CACHED_BUNDLE_FILE_FOLDER = "AssetBundleFiles";
		const string CACHED_RAW_FILE_FOLDER = "AssetRawFiles";
		const string CACHED_MANIFEST_FOLDER_NAME = "AssetManifestFiles";
		const string APP_FOOT_PRINT_FILE_NAME = "ApplicationFootPrint.bytes";

		const string STREAMING_ASSETS_FOLDER = "StreamingAssets";
		public const string STREAMING_ASSETS_BUILTIN_FOLDER = "BuiltInFiles";

		const string FILTER_MANIFEST_SEARCH = "*.manifest";
		const string FILTER_META_SEARCH = "*.meta";

		const string CACHE_SYSTEM_PATH_FORMAT = "{0}/UniverseCacheSystem";
		const string BUNDLE_OUTPUT_DIRECTORY_FORMAT = "{0}/UniverseBundles";

		static readonly HashSet<string> s_Extensions = new()
		{
			"prefab",
			"unity",
			"fbx",
			"anim",
			"controller",
			"png",
			"jpg",
			"mat",
			"shader",
			"ttf",
			"cs",
		};

		public static bool IsXml(string fileName)
		{
			return File.Exists(fileName) && !string.IsNullOrEmpty(fileName) && Path.GetExtension(fileName) == EXTENSION_XML;
		}

		public static bool IsJson(string fileName)
		{
			return File.Exists(fileName) && !string.IsNullOrEmpty(fileName) && Path.GetExtension(fileName) == EXTENSION_JSON;
		}

		public static string ToXmlPath(string directory, string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				return string.Empty;
			}

			return StringUtilities.Format(FILE_PATH_FORMAT_2, directory, GetXmlFileName(fileName));
		}

		public static string ToJsonPath(string directory, string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				return string.Empty;
			}

			return StringUtilities.Format(FILE_PATH_FORMAT_2, directory, GetJsonFileName(fileName));
		}

		public static string ToPath(params string[] paths)
		{
			if (paths == null || paths.Length == 0)
			{
				return string.Empty;
			}

			return Path.Combine(paths);
		}

		public static string ToFile(string name, string extension)
		{
			return StringUtilities.Format(FILE_NAME_EXTENSION_FORMAT, name, extension);
		}

		public static string ToPath(string path1, string path2)
		{
			return StringUtilities.Format(FILE_PATH_FORMAT_2, path1, path2);
		}

		public static string ToPath(string path1, string path2, string path3)
		{
			return StringUtilities.Format(FILE_PATH_FORMAT_3, path1, path2, path3);
		}

		public static string GetExeFileName(string name)
		{
			return StringUtilities.Format(FILE_NAME_FORMAT, name, EXTENSION_EXE);
		}

		public static string GetDynamicLinkLibraryFileName(string name)
		{
			return StringUtilities.Format(FILE_NAME_FORMAT, name, EXTENSION_DLL);
		}

		public static string GetXmlFileName(string name)
		{
			return StringUtilities.Format(FILE_NAME_FORMAT, name, EXTENSION_XML);
		}

		public static string GetJsonFileName(string name)
		{
			return StringUtilities.Format(FILE_NAME_FORMAT, name, EXTENSION_JSON);
		}

		public static long GetSizeKb(long length)
		{
			return length / KB;
		}

		public static long GetSizeMb(long length)
		{
			return length / MB;
		}

		public static long GetSizeGb(long length)
		{
			return length / GB;
		}

		public static string AppendExtension(string name, string extension)
		{
			if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(extension))
			{
				return string.Empty;
			}

			return StringUtilities.Format(FILE_NAME_EXTENSION_FORMAT, name, extension);
		}

		public static bool IsValidAssetFile(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}

			string extension = Path.GetExtension(path);
			extension = StringUtilities.RemoveFirstChar(extension);
			return s_Extensions.Contains(extension);
		}
	}
}
