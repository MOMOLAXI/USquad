using System.IO;
using System.Text;

namespace UniverseEngine
{
    public partial class FileSystem
    {
        public static async UniTask<byte[]> ReadAllBytesAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Log.Error($"File not exist, can not read {filePath}");
                return null;
            }

            return await File.ReadAllBytesAsync(filePath);
        }

        public static async UniTask<string> ReadAllTextAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }

            return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        }

        /// <summary>
        /// 异步创建文件，适用于大文件的创建
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async UniTask<bool> CreateFileAsync(string filePath, string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            return await CreateFileAsync(filePath, bytes);
        }

        public static async UniTask<bool> CreateFileAsync(string filePath, byte[] content)
        {
            // 删除旧文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // 创建文件夹路径
            CreateFileDirectory(filePath);

            // 创建新文件
            await using (FileStream fs = File.Create(filePath))
            {
                await fs.WriteAsync(content, 0, content.Length);
            }

            return true;
        }
    }
}
