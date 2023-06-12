using System;

namespace UniverseEngine
{
    public enum EFileHash
    {
        MD5,
        CRC32,
        SHA1,
    }
    
    public partial class FileSystem
    {
        public static string GetHash(string filePath, EFileHash hashFunction = EFileHash.MD5)
        {
            return hashFunction switch
            {
                EFileHash.MD5 => HashUtilities.FileMD5(filePath),
                EFileHash.CRC32 => HashUtilities.FileCRC32(filePath),
                EFileHash.SHA1 => HashUtilities.FileSHA1(filePath),
                _ => throw new ArgumentOutOfRangeException(nameof(hashFunction), hashFunction, null)
            };
        }
    }
}
