using System.IO;
using UnityEngine;

namespace UniverseEngine
{
    public partial class FileSystem
    {
        public static T FromJson<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            string json = ReadAllText(filePath);
            return JsonUtility.FromJson<T>(json);
        }

        public static async UniTask<T> FromJsonAsync<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            string json = await ReadAllTextAsync(filePath);
            return JsonUtility.FromJson<T>(json);
        }

        public static void SaveToJson<T>(T data, string savePath)
        {
            if (data == null || string.IsNullOrEmpty(savePath))
            {
                return;
            }
            
            DeleteFile(savePath);
            string json = JsonUtility.ToJson(data, true);
            CreateFile(savePath, json, false);
        }
    }
}
