using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UniverseEngine
{
	public static partial class HashUtilities
	{
		public const string SPLITER = "-";

		/// <summary>
		/// 获取字符串的CRC32
		/// </summary>
		public static string StringCRC32(string str)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(str);
			return BytesCRC32(buffer);
		}

		/// <summary>
		/// 获取文件的CRC32
		/// </summary>
		public static string FileCRC32(string filePath)
		{
			try
			{
				using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					return StreamCRC32(fs);
				}
			}
			catch (Exception e)
			{
				Log.Exception(e);
				return string.Empty;
			}
		}

		/// <summary>
		/// 获取数据流的CRC32
		/// </summary>
		public static string StreamCRC32(Stream stream)
		{
			CRC32Algorithm hash = new();
			byte[] hashBytes = hash.ComputeHash(stream);
			return ToString(hashBytes, SPLITER);
		}

		/// <summary>
		/// 获取字节数组的CRC32
		/// </summary>
		public static string BytesCRC32(byte[] buffer)
		{
			CRC32Algorithm hash = new();
			byte[] hashBytes = hash.ComputeHash(buffer);
			return ToString(hashBytes, SPLITER);
		}

		/// <summary>
		/// 获取字符串的MD5
		/// </summary>
		public static string StringMD5(string str)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(str);
			return BytesMD5(buffer);
		}

		/// <summary>
		/// 获取文件的MD5
		/// </summary>
		public static string FileMD5(string filePath)
		{
			try
			{
				using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					return StreamMD5(fs);
				}
			}
			catch (Exception e)
			{
				Log.Exception(e);
				return string.Empty;
			}
		}

		/// <summary>
		/// 获取数据流的MD5
		/// </summary>
		public static string StreamMD5(Stream stream)
		{
			MD5CryptoServiceProvider provider = new();
			byte[] hashBytes = provider.ComputeHash(stream);
			return ToString(hashBytes);
		}

		/// <summary>
		/// 获取字节数组的MD5
		/// </summary>
		public static string BytesMD5(byte[] buffer)
		{
			MD5CryptoServiceProvider provider = new();
			byte[] hashBytes = provider.ComputeHash(buffer);
			return ToString(hashBytes);
		}

		/// <summary>
		/// 获取字符串的Hash值
		/// </summary>
		public static string StringSHA1(string str)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(str);
			return BytesSHA1(buffer);
		}

		/// <summary>
		/// 获取文件的Hash值
		/// </summary>
		public static string FileSHA1(string filePath)
		{
			try
			{
				using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					return StreamSHA1(fs);
				}
			}
			catch (Exception e)
			{
				Log.Exception(e);
				return string.Empty;
			}
		}

		/// <summary>
		/// 获取数据流的Hash值
		/// </summary>
		public static string StreamSHA1(Stream stream)
		{
			// 说明：创建的是SHA1类的实例，生成的是160位的散列码
			HashAlgorithm hash = HashAlgorithm.Create();
			byte[] hashBytes = hash.ComputeHash(stream);
			return ToString(hashBytes, SPLITER);
		}

		/// <summary>
		/// 获取字节数组的Hash值
		/// </summary>
		public static string BytesSHA1(byte[] buffer)
		{
			// 说明：创建的是SHA1类的实例，生成的是160位的散列码
			HashAlgorithm hash = HashAlgorithm.Create();
			byte[] hashBytes = hash.ComputeHash(buffer);
			return ToString(hashBytes, SPLITER);
		}

		static string ToString(byte[] hashBytes, string replaceTarget = SPLITER)
		{
			string result = BitConverter.ToString(hashBytes).Replace(replaceTarget, string.Empty);
			return result.ToLower();
		}
	}
}
