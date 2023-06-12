using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UniverseEngine
{
	public partial class FileSystem : EngineSystem
	{
		/// <summary>
		/// 读二进制
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static byte[] ReadAllBytes(string filePath)
		{
			if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
			{
				Log.Error($"File not exist, can not read {filePath}");
				return null;
			}

			return File.ReadAllBytes(filePath);
		}

		/// <summary>
		/// 读取文件的文本数据
		/// </summary>
		public static string ReadAllText(string filePath)
		{
			if (!File.Exists(filePath))
			{
				return string.Empty;
			}

			return File.ReadAllText(filePath, Encoding.UTF8);
		}

		public static void CreateFile(string filePath, byte[] content, bool withDirectory = true)
		{
			// 删除旧文件
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}

			// 创建文件夹路径
			if (withDirectory)
			{
				CreateFileDirectory(filePath);
			}
			
			File.WriteAllBytes(filePath, content);
		}

		/// <summary>
		/// 创建文件（如果已经存在则删除旧文件）
		/// </summary>
		public static void CreateFile(string filePath, string content, bool withDirectory = true)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			CreateFile(filePath, bytes, withDirectory);
		}

		/// <summary>
		/// 创建文件的文件夹路径
		/// </summary>
		public static void CreateFileDirectory(string filePath)
		{
			// 获取文件的文件夹路径
			string directory = Path.GetDirectoryName(filePath);
			CreateDirectory(directory);
		}

		/// <summary>
		/// 创建文件夹路径
		/// </summary>
		public static bool CreateDirectory(string directory)
		{
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
				return true;
			}

			return false;
		}

		/// <summary>
		/// 删除文件夹及子目录
		/// </summary>
		public static bool DeleteDirectory(string directory, bool recrusive = true)
		{
			if (Directory.Exists(directory))
			{
				Directory.Delete(directory, recrusive);
				return true;
			}

			return false;
		}

		public static bool DeleteFileOwningDirectory(FileInfo fileInfo)
		{
			if (fileInfo == null)
			{
				return false;
			}

			if (fileInfo.Exists)
			{
				fileInfo.Directory.Delete(true);
			}

			return true;
		}

		/// <summary>
		/// 追溯文件夹
		/// </summary>
		/// <param name="path"></param>
		/// <param name="rootPath"></param>
		/// <param name="result"></param>
		public static void TrackDirectories(in string path, string rootPath, Stack<string> result)
		{
			if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(rootPath) || result == null)
			{
				return;
			}

			result.Clear();
			string targetPath = path;
			while (true)
			{
				if (targetPath == rootPath)
				{
					return;
				}

				string folder = Path.GetDirectoryName(targetPath);
				if (string.IsNullOrEmpty(folder))
				{
					return;
				}

				if (folder != rootPath)
				{
					result.Push(folder);
					targetPath = folder;
					continue;
				}

				break;
			}
		}

		/// <summary>
		/// 文件夹路径是否相同
		/// </summary>
		/// <param name="d1"></param>
		/// <param name="d2"></param>
		/// <returns></returns>
		public static bool IsSameDirectory(string d1, string d2)
		{
			string dir1 = Path.GetDirectoryName(d1);
			string dir2 = Path.GetDirectoryName(d2);
			if (string.IsNullOrEmpty(dir1) || string.IsNullOrEmpty(dir2))
			{
				return false;
			}

			return dir1 == dir2;
		}

		/// <summary>
		/// 递归查找目标文件夹路径
		/// </summary>
		/// <param name="root">搜索的根目录</param>
		/// <param name="folderName">目标文件夹名称</param>
		/// <returns>返回找到的文件夹路径，如果没有找到返回空字符串</returns>
		public static string FindFolder(string root, string folderName)
		{
			DirectoryInfo rootInfo = new(root);
			DirectoryInfo[] infoList = rootInfo.GetDirectories();
			for (int i = 0; i < infoList.Length; i++)
			{
				DirectoryInfo info = infoList[i];
				string fullPath = info.FullName;
				if (info.Name == folderName)
				{
					return fullPath;
				}

				string result = FindFolder(fullPath, folderName);
				if (!string.IsNullOrEmpty(result))
				{
					return result;
				}
			}

			return string.Empty;
		}

		/// <summary>
		/// 上n级目录
		/// </summary>
		/// <param name="dir"></param>
		/// <param name="n"></param>
		/// <returns></returns>
		public static string GetParentDir(string dir, int n = 1)
		{
			string subDir = dir;

			for (int i = 0; i < n; ++i)
			{
				int last = subDir.LastIndexOf('/');
				subDir = InterceptPath(subDir, 0, last);
			}

			return subDir;
		}

		/// <summary>
		/// 文件重命名
		/// </summary>
		public static void FileRename(string filePath, string newName)
		{
			string dirPath = Path.GetDirectoryName(filePath);
			string destPath;
			if (Path.HasExtension(filePath))
			{
				string extentsion = Path.GetExtension(filePath);
				destPath = $"{dirPath}/{newName}{extentsion}";
			}
			else
			{
				destPath = $"{dirPath}/{newName}";
			}

			FileInfo fileInfo = new(filePath);
			fileInfo.MoveTo(destPath);
		}

		/// <summary>
		/// 移动文件
		/// </summary>
		public static void MoveFile(string filePath, string destPath)
		{
			if (File.Exists(destPath))
			{
				File.Delete(destPath);
			}

			FileInfo fileInfo = new(filePath);
			fileInfo.MoveTo(destPath);
		}

		/// <summary>
		/// 拷贝文件夹
		/// 注意：包括所有子目录的文件
		/// </summary>
		public static void CopyDirectory(string sourcePath, string destPath)
		{
			sourcePath = GetRegularPath(sourcePath);

			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(destPath))
			{
				Directory.CreateDirectory(destPath);
			}

			string[] fileList = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
			foreach (string file in fileList)
			{
				string temp = GetRegularPath(file);
				string savePath = temp.Replace(sourcePath, destPath);
				CopyFile(file, savePath, true);
			}
		}

		/// <summary>
		/// 拷贝文件
		/// </summary>
		public static void CopyFile(string sourcePath, string destPath, bool overwrite)
		{
			if (!File.Exists(sourcePath))
			{
				Log.Error($"File not found at path : {sourcePath}");
				return;
			}

			// 创建目录
			CreateFileDirectory(destPath);

			// 复制文件
			File.Copy(sourcePath, destPath, overwrite);
		}

		/// <summary>
		/// 清空文件夹
		/// </summary>
		/// <param name="directoryPath">要清理的文件夹路径</param>
		public static void ClearFolder(string directoryPath)
		{
			if (!Directory.Exists(directoryPath))
			{
				return;
			}

			// 删除文件
			string[] allFiles = Directory.GetFiles(directoryPath);
			for (int i = 0; i < allFiles.Length; i++)
			{
				File.Delete(allFiles[i]);
			}

			// 删除文件夹
			string[] allFolders = Directory.GetDirectories(directoryPath);
			for (int i = 0; i < allFolders.Length; i++)
			{
				Directory.Delete(allFolders[i], true);
			}
		}

		/// <summary>
		/// 删除文件
		/// </summary>
		/// <param name="filePath"></param>
		public static void DeleteFile(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				return;
			}

			if (File.Exists(filePath))
			{
				File.Delete(filePath);
				Log.Info($"File {filePath} has been removed");
			}
		}

		/// <summary>
		/// 获取目录下所有文件
		/// </summary>
		/// <param name="directory"></param>
		/// <returns></returns>
		public static FileInfo[] GetAllFiles(string directory)
		{
			if (string.IsNullOrEmpty(directory))
			{
				return Array.Empty<FileInfo>();
			}

			if (!Directory.Exists(directory))
			{
				return Array.Empty<FileInfo>();
			}

			DirectoryInfo info = new(directory);
			return info.GetFiles(".", SearchOption.AllDirectories);
		}

		public static long GetFileSize(string filePath)
		{
			long temp = 0;
			if (!File.Exists(filePath))
			{
				string[] str1 = Directory.GetFileSystemEntries(filePath);
				foreach (string s1 in str1)
				{
					temp += GetFileSize(s1);
				}
			}
			else
			{
				FileInfo fileInfo = new(filePath);
				return fileInfo.Length;
			}
			return temp;
		}
	}
}
