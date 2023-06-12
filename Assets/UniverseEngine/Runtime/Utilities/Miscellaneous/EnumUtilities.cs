using System;
using System.Collections.Generic;
using System.Linq;

namespace UniverseEngine
{
	public static class EnumUtilities
	{
		/// <summary>
		/// 使用频度高的地方慎用！
		/// 转换成枚举，因为当前C#版本没法限定为枚举，暂时限制为值类型
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool ParseEnum<T>(this string value, out T result) where T : Enum
		{
			result = default;
			try
			{
				result = (T)Enum.Parse(typeof(T), value);
				return true;
			}
			catch (Exception e)
			{
				Log.Error(e);
				return false;
			}
		}

		public static T NameToEnum<T>(string name)
		{
			if (!Enum.IsDefined(typeof(T), name))
			{
				Log.Error($"Enum {typeof(T)} is not defined name {name}");
				return default;
			}

			return (T)Enum.Parse(typeof(T), name);
		}

		public static List<string> GetNames<T>()
		{
			return Enum.GetNames(typeof(T)).ToList();
		}

		public static int GetIndex<T>(T value) where T : Enum
		{
			List<string> names = GetNames<T>();
			if (Collections.IsNullOrEmpty(names))
			{
				return -1;
			}

			return names.FindIndex(name => name == value.ToString());
		}

		public static int GetIndex<T>(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return -1;
			}

			List<string> names = GetNames<T>();
			if (Collections.IsNullOrEmpty(names))
			{
				return -1;
			}

			return names.FindIndex(name => name == value);
		}
	}
}
