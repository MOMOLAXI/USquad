using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UniverseEngine
{
    public partial class FileSystem
    {
        static readonly Dictionary<string, bool> s_CacheData = new(1000);

        /// <summary>
        /// 获取流文件夹路径
        /// </summary>
        public static string GetStreamingAssetsBuiltInFolderPath()
        {
            return StringUtilities.Format(FILE_PATH_FORMAT_3,
                                          Application.dataPath, STREAMING_ASSETS_FOLDER,
                                          STREAMING_ASSETS_BUILTIN_FOLDER);
        }

        /// <summary>
        /// 清空流文件夹
        /// </summary>
        public static void ClearStreamingAssetsBuiltInFolder()
        {
            string streamingFolderPath = GetStreamingAssetsBuiltInFolderPath();
            ClearFolder(streamingFolderPath);
        }

        /// <summary>
        /// 删除流文件夹内无关的文件
        /// 删除.manifest文件和.meta文件
        /// </summary>
        public static void DeleteStreamingAssetsIgnoreFiles()
        {
            string streamingFolderPath = GetStreamingAssetsBuiltInFolderPath();
            if (Directory.Exists(streamingFolderPath))
            {
                string[] files = Directory.GetFiles(streamingFolderPath, FILTER_MANIFEST_SEARCH, SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    FileInfo info = new(file);
                    info.Delete();
                }

                files = Directory.GetFiles(streamingFolderPath, FILTER_META_SEARCH, SearchOption.AllDirectories);
                foreach (string item in files)
                {
                    FileInfo info = new(item);
                    info.Delete();
                }
            }
        }

        public static string GetBuiltInPackageVersionFilePath(string packageName)
        {
            string versionFileName = GetPackageVersionFileName(packageName);
            return ToStreamingLoadPath(versionFileName);
        }

        public static string GetBuiltInPackageHashFilePath(string packageName, string version)
        {
            string hashFileName = GetPackageHashFileName(packageName, version);
            return ToStreamingLoadPath(hashFileName);
        }

        public static string GetBuiltInManifestBinaryFilePath(string packageName, string version)
        {
            string manifestFileName = GetManifestBinaryFileName(packageName, version);
            return ToStreamingLoadPath(manifestFileName);
        }

        /// <summary>
        /// 利用安卓原生接口查询内置文件是否存在
        /// </summary>
        public static bool FileExistsWithAndroid(string filePath)
        {
            if (!s_CacheData.TryGetValue(filePath, out bool result))
            {
                result = IsAndroidPlatform()
                             ? AndroidSystem.InvokeCurrentActivityInstanceMethod<bool>("CheckAssetExist", filePath)
                             : File.Exists(Path.Combine(Application.streamingAssetsPath, filePath));

                s_CacheData.Add(filePath, result);
            }
            return result;
        }
    }
}
