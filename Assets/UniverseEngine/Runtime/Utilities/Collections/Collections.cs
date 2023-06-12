using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace UniverseEngine
{
	public static class Collections
	{
		public static List<T> EmptyList<T>()
		{
			return Empty<T>.List;
		}

		public static bool IsNullOrEmpty<T>(T[] target)
		{
			return target == null || target.Length == 0;
		}

		public static bool IsNullOrEmpty<T>(IList<T> target)
		{
			return target == null || target.Count == 0;
		}
		
		public static bool IsValidIndex(int index, int count)
		{
			return index > -1 && index < count;
		}

		public static bool IsValidIndex<T>(T[] collection, int index)
		{
			return index > -1 && index < collection.Length;
		}

		public static bool IsValidIndex<T>(List<T> collection, int index)
		{
			return index > -1 && index < collection.Count;
		}

		public static bool IsValidIndex<T>(IReadOnlyList<T> collection, int index)
		{
			return index > -1 && index < collection.Count;
		}

		public static void Swap<T>(List<T> array, int left, int right)
		{
			if (!IsValidIndex(array, left) || !IsValidIndex(array, right))
			{
				return;
			}

			(array[left], array[right]) = (array[right], array[left]);
		}

		/// <summary>
		/// 安全获取数据
		/// </summary>
		public static T SafeGet<T>(this IReadOnlyList<T> dataList, int index)
		{
			return !Collections.IsValidIndex(dataList, index) ? default : dataList[index];
		}

		public static bool AllTrue(this IReadOnlyList<bool> target)
		{
			return target.All(x => x);
		}

		public static bool AllTrue<T>(this IReadOnlyList<(bool state, T _)> target)
		{
			return target.All(tuple => tuple.state);
		}

		public static T Get<T>(this T[] target, int index)
		{
			return Collections.IsValidIndex(target.Length, index) ? target[index] : default;
		}

		public static void Filter<T>(this List<T> target, Func<T, bool> filter)
		{
			if (filter == null || target == null)
			{
				return;
			}

			using (AllocAutoList(out List<T> temp))
			{
				for (int i = 0; i < target.Count; i++)
				{
					T t = target[i];
					if (filter.Invoke(t))
					{
						temp.Add(t);
					}
				}

				target.Clear();
				target.AddRange(temp);
			}
		}

		public static void Filter<T>(this List<T> target, Func<T, bool> filter, List<T> result)
		{
			if (target == null || result == null || filter == null)
			{
				return;
			}

			result.Clear();
			for (int i = 0; i < target.Count; i++)
			{
				T t = target[i];
				if (filter.Invoke(t))
				{
					result.Add(t);
				}
			}
		}

		public static string QueryStringArgs(object[] args, int index, string defaultVal = "")
		{
			if (args == null || !Collections.IsValidIndex(args, index))
			{
				return defaultVal;
			}

			return args[index].ToString(defaultVal);
		}

		public static int QueryIntArgs(object[] args, int index, int defaultVal = 0)
		{
			if (args == null || !Collections.IsValidIndex(args, index))
			{
				return defaultVal;
			}

			return args[index].ToInt(defaultVal);
		}

		public static int QueryIntArgs(string[] args, int index, int defaultVal = 0)
		{
			if (args == null || !Collections.IsValidIndex(args, index))
			{
				return defaultVal;
			}

			return args[index].ToInt();
		}

		public static long QueryInt64Args(object[] args, int index, long defaultVal = 0)
		{
			if (args == null || !Collections.IsValidIndex(args, index))
			{
				return defaultVal;
			}

			return args[index].ToLong(defaultVal);
		}

		public static bool QueryBoolArgs(object[] args, int index, bool defaultVal = false)
		{
			if (args == null || !Collections.IsValidIndex(args, index))
			{
				return defaultVal;
			}

			object obj = args[index];
			switch (obj)
			{
				case bool b:
				{
					return b;
				}
				case int v:
				{
					return v != 0;
				}
				case string s:
				{
					return s.ParseBool();
				}
				default:
				{
					float value = obj.ToFloat();
					return Math.Abs(value) > float.Epsilon || defaultVal;
				}
			}
		}

		public static T QueryStructArgs<T>(object[] args, int index, T defaultVal = default(T)) where T : struct
		{
			if (args == null || !Collections.IsValidIndex(args, index))
			{
				return defaultVal;
			}

			return args[index] is T ? (T)args[index] : defaultVal;
		}

		public static bool QueryBoolArgs(string[] args, int index, bool defaultVal = false)
		{
			if (args == null || !Collections.IsValidIndex(args, index))
			{
				return defaultVal;
			}

			return args[index].ToBool();
		}

		public static float QueryFloatArgs(object[] args, int index, float defaultVal = 0)
		{
			if (args == null || !Collections.IsValidIndex(args, index))
			{
				return defaultVal;
			}

			object obj = args[index];
			return obj.ToFloat(defaultVal);
		}

		public static void EnsureArray<T>(ref T[] array, int size, T initialValue = default(T))
		{
			if (array != null && array.Length == size)
			{
				return;
			}

			array = new T[size];
			for (int i = 0; i != size; i++)
			{
				array[i] = initialValue;
			}
		}

		public static List<T> AllocList<T>()
		{
			return ListPool<T>.Get();
		}

		public static void Release<T>(List<T> list)
		{
			ListPool<T>.Release(list);
		}

		public static HashSet<T> AllocSet<T>()
		{
			return HashSetPool<T>.Get();
		}

		public static void Release<T>(HashSet<T> set)
		{
			HashSetPool<T>.Release(set);
		}

		public static Dictionary<TK, TV> AllocMap<TK, TV>()
		{
			return DictionaryPool<TK, TV>.Get();
		}

		public static void Release<TK, TV>(Dictionary<TK, TV> dictionary)
		{
			DictionaryPool<TK, TV>.Release(dictionary);
		}

		public static LinkedPool<List<T>>.ObjectHandle<List<T>> AllocAutoList<T>(out List<T> result)
		{
			return ListPool<T>.Get(out result);
		}

		public static LinkedPool<HashSet<T>>.ObjectHandle<HashSet<T>> AllocAutoSet<T>(out HashSet<T> result)
		{
			return HashSetPool<T>.Get(out result);
		}

		public static LinkedPool<Dictionary<TK, TV>>.ObjectHandle<Dictionary<TK, TV>> AllocAutoMap<TK, TV>(out Dictionary<TK, TV> result)
		{
			return DictionaryPool<TK, TV>.Get(out result);
		}

		/// <summary>
		/// 集合缓存池
		/// </summary>
		/// <typeparam name="TCollection"></typeparam>
		/// <typeparam name="TItem"></typeparam>
		private class CollectionPool<TCollection, TItem> where TCollection : class, ICollection<TItem>, new()
		{
			static readonly LinkedPool<TCollection> s_Pool = new(() => new(), null, l => l.Clear());
			public static TCollection Get() => s_Pool.Get();
			public static LinkedPool<TCollection>.ObjectHandle<TCollection> Get(out TCollection value) => s_Pool.Get(out value);
			public static void Release(TCollection toRelease) => s_Pool.Release(toRelease);
		}

		[UsedImplicitly]
		private class ListPool<T> : CollectionPool<List<T>, T>
		{
		}

		[UsedImplicitly]
		private class HashSetPool<T> : CollectionPool<HashSet<T>, T>
		{
		}

		[UsedImplicitly]
		private class DictionaryPool<TKey, TValue> : CollectionPool<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>
		{
		}

		private static class Empty<T>
		{
			static List<T> s_EmptyList;
			public static List<T> List => s_EmptyList ??= new();
		}
	}
}
