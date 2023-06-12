using System.IO;

namespace UniverseEngine
{
    /// <summary>
    /// 缓存文件验证元素
    /// </summary>
    internal struct FAssetCacheFile
    {
        public readonly string PackageName;
        public readonly string CacheGuid;
        public readonly string FileRootPath;
        public readonly string DataFilePath;
        public readonly string InfoFilePath;

        public EAssetFileCacheResult Result;
        public string DataFileCRC;
        public long DataFileSize;

        public FAssetCacheFile(string packageName, string cacheGuid, string fileRootPath, string dataFilePath, string infoFilePath)
        {
            PackageName = packageName;
            CacheGuid = cacheGuid;
            FileRootPath = fileRootPath;
            DataFilePath = dataFilePath;
            InfoFilePath = infoFilePath;
            Result = EAssetFileCacheResult.Succeed;
            DataFileCRC = string.Empty;
            DataFileSize = 0;
        }

        public EAssetFileCacheResult LowLevelVerify()
        {
            if (!File.Exists(InfoFilePath))
            {
                return EAssetFileCacheResult.InfoFileNotExisted;
            }

            if (!File.Exists(DataFilePath))
            {
                return EAssetFileCacheResult.DataFileNotExisted;
            }

            return EAssetFileCacheResult.Succeed;
        }

        public void ReadFromFileBuffer()
        {
            byte[] bytes = FileSystem.ReadAllBytes(InfoFilePath);
            BufferReader buffer = new(bytes);
            DataFileCRC = buffer.ReadUTF8();
            DataFileSize = buffer.ReadInt64();
        }

        public void Delete()
        {
            if (File.Exists(DataFilePath))
            {
                File.Delete(DataFilePath);
            }

            if (File.Exists(InfoFilePath))
            {
                File.Delete(InfoFilePath);
            }
        }
    }
}
