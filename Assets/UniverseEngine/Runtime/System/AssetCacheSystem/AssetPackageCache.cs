using System.Collections.Generic;

namespace UniverseEngine
{
    internal class AssetPackageCache
    {
        /// <summary>
        /// Guid -> AssetCacheFileMeta
        /// </summary>
        readonly Dictionary<string, FileCacheMeta> m_CacheMetas = new();

        public readonly string PackageName;
        
        public AssetPackageCache(string packageName)
        {
            PackageName = packageName;
        }
        
        public void Clear()
        {
            m_CacheMetas.Clear();
        }

        public int GetFileCount() => m_CacheMetas.Count;

        public bool IsCached(string cacheGuid) => !string.IsNullOrEmpty(cacheGuid) && m_CacheMetas.ContainsKey(cacheGuid);

        public bool TryGetMeta(string cacheGuid, out FileCacheMeta meta)
        {
            meta = default;
            if (string.IsNullOrEmpty(cacheGuid))
            {
                return false;
            }

            return m_CacheMetas.TryGetValue(cacheGuid, out meta);
        }

        public void Add(string cacheGuid, FileCacheMeta meta)
        {
            if (string.IsNullOrEmpty(cacheGuid))
            {
                return;
            }
            
            if (!m_CacheMetas.TryAdd(cacheGuid, meta))
            {
                Log<AssetCacheSystem>.Error($"Internal error... Package cache already contains guid : {cacheGuid}, file path : {meta.DataFilePath}");
            }
        }

        public void Remove(string cacheGuid)
        {
            if (m_CacheMetas.ContainsKey(cacheGuid))
            {
                m_CacheMetas.Remove(cacheGuid);
            }
        }

        public void GetCacheGuids(List<string> result)
        {
            if (result == null)
            {
                return;
            }
            
            result.Clear();
            result.AddRange(m_CacheMetas.Keys);
        }
    }
}
