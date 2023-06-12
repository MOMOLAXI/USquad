using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace UniverseEngine
{
	public partial class FileSystem
	{
		static readonly Dictionary<Type, XmlSerializer> s_XmlSerializers = new();

		public static async UniTask<XElement> OpenXmlAsync(string xml)
		{
			try
			{
				byte[] bytes = await ReadAllBytesAsync(xml);
				return XElement.Load(new MemoryStream(bytes));
			}
			catch (Exception e)
			{
				throw new($"打开Xml文件:{xml} 失败 --> {e.Message}");
			}
		}

		/// <summary>
		/// 保存到xml
		/// </summary>
		/// <param name="data"></param>
		/// <param name="path"></param>
		public static void SerializeToXml(object data, string path)
		{
			if (string.IsNullOrEmpty(path) || data == null)
			{
				return;
			}

			string directory = Path.GetDirectoryName(path);
			if (string.IsNullOrEmpty(directory))
			{
				return;
			}

			if (!string.IsNullOrEmpty(directory))
			{
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}
			}

			using (StreamWriter stream = new(path))
			{
				XmlSerializer serializer = GetXmlSerializer(data.GetType());
				serializer.Serialize(stream, data);
			}
		}

		/// <summary>
		/// 写入XML
		/// </summary>
		/// <param name="data"></param>
		/// <param name="path"></param>
		/// <typeparam name="T"></typeparam>
		public static void SerializeToXml<T>(T data, string path) where T : class
		{
			if (string.IsNullOrEmpty(path) || data == null)
			{
				return;
			}

			string directory = Path.GetDirectoryName(path);
			if (string.IsNullOrEmpty(directory))
			{
				return;
			}

			if (!string.IsNullOrEmpty(directory))
			{
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}
			}

			using (StreamWriter stream = new(path))
			{
				XmlSerializer serializer = GetXmlSerializer<T>();
				serializer.Serialize(stream, data);
			}
		}

		/// <summary>
		/// 写入XML
		/// </summary>
		/// <param name="data"></param>
		/// <param name="path"></param>
		/// <typeparam name="T"></typeparam>
		public static async UniTaskVoid SerializeToXmlAsync<T>(T data, string path) where T : class
		{
			if (string.IsNullOrEmpty(path) || data == null)
			{
				return;
			}

			string directory = Path.GetDirectoryName(path);
			if (string.IsNullOrEmpty(directory))
			{
				return;
			}

			if (!string.IsNullOrEmpty(directory))
			{
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}
			}

			await using (StreamWriter stream = new(path))
			{
				XmlSerializer serializer = GetXmlSerializer<T>();
				serializer.Serialize(stream, data);
			}
		}


		/// <summary>
		/// 从Xml加载对象，没有则会创建xml
		/// </summary>
		/// <param name="absPath">绝对路径</param>
		/// <param name="createFunc"></param>
		/// <param name="autoCreate">是否自动创建</param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T DeserializeFromXml<T>(string absPath, Func<T> createFunc, bool autoCreate = true) where T : class, new()
		{
			if (string.IsNullOrEmpty(absPath))
			{
				return null;
			}

			XmlSerializer serializer = GetXmlSerializer<T>();
			if (autoCreate)
			{
				string directory = Path.GetDirectoryName(absPath);
				if (directory != null)
				{
					//检查文件夹
					if (!File.Exists(directory))
					{
						Directory.CreateDirectory(directory);
					}

					//检查文件
					if (!File.Exists(absPath) && createFunc != null)
					{
						using (StreamWriter stream = new(absPath))
						{
							serializer.Serialize(stream, createFunc.Invoke());
						}
					}

					if (!File.Exists(absPath))
					{
						Log.Error($"Can no read file : {absPath}");
						return null;
					}
				}
			}

			T ret = null;
			using (StreamReader stream = new(absPath))
			{
				ret = serializer.Deserialize(stream) as T;
			}

			return ret;
		}

		/// <summary>
		/// 从Xml加载对象，没有则会创建xml
		/// </summary>
		/// <param name="absPath">绝对路径</param>
		/// <param name="autoCreate">是否自动创建</param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static async UniTask<T> DeserializeFromXmlAsync<T>(string absPath, bool autoCreate = true) where T : class, new()
		{
			if (string.IsNullOrEmpty(absPath))
			{
				return null;
			}

			XmlSerializer serializer = GetXmlSerializer<T>();
			if (autoCreate)
			{
				string directory = Path.GetDirectoryName(absPath);
				if (directory != null)
				{
					//检查文件夹
					if (!File.Exists(directory))
					{
						Directory.CreateDirectory(directory);
					}

					//检查文件
					if (!File.Exists(absPath))
					{
						await using (StreamWriter stream = new(absPath))
						{
							serializer.Serialize(stream, new T());
						}
					}
				}
			}

			T ret = null;
			using (StreamReader stream = new(absPath))
			{
				ret = serializer.Deserialize(stream) as T;
			}

			return ret;
		}

		public static XmlSerializer GetXmlSerializer(Type type)
		{
			if (type == null)
			{
				return default;
			}

			XmlSerializer serializer;
			if (s_XmlSerializers.TryGetValue(type, out XmlSerializer xmlSerializer))
			{
				serializer = xmlSerializer;
			}
			else
			{
				serializer = new(type);
				s_XmlSerializers[type] = serializer;
			}

			return serializer;
		}

		/// <summary>
		/// 获取对象Xml序列化器
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		static XmlSerializer GetXmlSerializer<T>()
		{
			return GetXmlSerializer(typeof(T));
		}

		public static XElement Open(string xmlFile)
		{
			try
			{
				return XElement.Load(xmlFile);
			}
			catch (Exception e)
			{
				throw new($"打开定义文件:{xmlFile} 失败 --> {e.Message}");
			}
		}

		public static XElement Open(string xmlFile, byte[] content)
		{
			try
			{
				return XElement.Load(new MemoryStream(content));
			}
			catch (Exception e)
			{
				throw new($"打开定义文件:{xmlFile} 失败 --> {e.Message}");
			}
		}

		public static string GetRequiredAttribute(XElement element, string key)
		{
			if (element.Attribute(key) != null)
			{
				string value = element.Attribute(key).Value.Trim();
				if (value.Length != 0)
				{
					return value;
				}
			}
			throw new ArgumentException($"ele:{element} key {key} 为空或未定义");
		}

		public static string GetOptionalAttribute(XElement element, string key)
		{
			return element.Attribute(key)?.Value ?? string.Empty;
		}

		public static bool GetOptionBoolAttribute(XElement element, string key, bool defaultValue = false)
		{
			string attr = element.Attribute(key)?.Value.ToLower();
			if (attr == null)
			{
				return defaultValue;
			}
			return attr is "1" or "true";
		}

		public static int GetOptionIntAttribute(XElement element, string key, int defaultValue = 0)
		{
			if (element.Attribute(key) == null)
			{
				return defaultValue;
			}
			return int.Parse(element.Attribute(key).Value);
		}

		public static int GetRequiredIntAttribute(XElement element, string key)
		{
			XAttribute attr = element.Attribute(key);
			try
			{
				return int.Parse(attr.Value);
			}
			catch (Exception)
			{
				throw new FormatException($"{element} 属性:{key}=>{attr?.Value} 不是整数");
			}
		}

		public static XElement OpenRelate(string relatePath, string toOpenXmlFile)
		{
			return Open(StandardizePathCombine(relatePath, toOpenXmlFile));
		}
	}
}
