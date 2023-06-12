using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniverseEngine
{
	public static class ReflectionUtilities
	{
		static readonly Dictionary<string, Assembly> s_Assemblies = new();

		public static Assembly GetLoadedAssembly(string assemblyName)
		{
			if (string.IsNullOrEmpty(assemblyName))
			{
				return null;
			}

			return Assembly.Load(assemblyName);
		}

		public static Type GetTypeInAssembly(string assemblyName, string typeName)
		{
			if (string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(typeName))
			{
				return null;
			}

			Assembly assembly = GetLoadedAssembly(assemblyName);
			return assembly.GetType(typeName);
		}

		public static object Public(string assemblyName, string typeName, string methodName, params object[] args)
		{
			if (string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(methodName))
			{
				return default;
			}

			Type type = GetTypeInAssembly(assemblyName, typeName);
			if (type == null)
			{
				return default;
			}

			return Public(type, methodName, args);
		}

		public static T Public<T>(string assemblyName, string typeName, string methodName, params object[] args)
		{
			return (T)Public(assemblyName, typeName, methodName, args);
		}

		public static object PublicStatic(string assemblyName, string typeName, string methodName, params object[] args)
		{
			if (string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(methodName))
			{
				return default;
			}

			Type type = GetTypeInAssembly(assemblyName, typeName);
			if (type == null)
			{
				return default;
			}

			return PublicStatic(type, methodName, args);
		}

		public static T PublicStatic<T>(string assemblyName, string typeName, string methodName, params object[] args)
		{
			return (T)PublicStatic(assemblyName, typeName, methodName, args);
		}

		public static object Public(Type type, string methodName, params object[] args)
		{
			if (type == null || string.IsNullOrEmpty(methodName))
			{
				return default;
			}

			return InvokeMethod(type, methodName, BindingFlags.Public | BindingFlags.Instance, args);
		}

		public static TResult Public<TResult>(Type type, string methodName, params object[] args)
		{
			if (type == null || string.IsNullOrEmpty(methodName))
			{
				return default;
			}

			return (TResult)Public(type, methodName, args);
		}

		public static object PublicStatic(Type type, string methodName, params object[] args)
		{
			if (type == null || string.IsNullOrEmpty(methodName))
			{
				return default;
			}

			return InvokeMethod(type, methodName, BindingFlags.Public | BindingFlags.Static, args);
		}

		public static TResult PublicStatic<TResult>(Type type, string methodName, params object[] args)
		{
			if (type == null || string.IsNullOrEmpty(methodName))
			{
				return default;
			}

			return (TResult)PublicStatic(type, methodName, args);
		}

		public static TResult NonPublicStatic<TClass, TResult>(string method, params object[] args)
		{
			return (TResult)NonPublicStatic(typeof(TClass), method, args);
		}

		public static object NonPublicStatic<T>(string method, params object[] args)
		{
			return NonPublicStatic(typeof(T), method, args);
		}

		public static object NonPublicStatic(Type type, string method, params object[] args)
		{
			MethodInfo methodInfo = type.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static);
			if (methodInfo == null)
			{
				Log.Error($"{type.FullName} not found method : {method}");
				return null;
			}

			return methodInfo.Invoke(null, args);
		}

		public static void FindInterfacesInScene<T>(List<T> interfaces)
		{
			if (interfaces == null)
			{
				return;
			}

			interfaces.Clear();
			GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			foreach (GameObject rootGameObject in rootGameObjects)
			{
				T[] childrenInterfaces = rootGameObject.GetComponentsInChildren<T>();
				foreach (T childInterface in childrenInterfaces)
				{
					interfaces.Add(childInterface);
				}
			}
		}

		public static void GetAllDerivedObjects<T>(List<Type> result, bool allowAbstract = true) where T : Component
		{
			if (result == null)
			{
				return;
			}

			result.Clear();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (type.IsSubclassOf(typeof(T)))
					{
						if (type.IsAbstract && allowAbstract)
						{
							result.Add(type);
						}
						else
						{
							result.Add(type);
						}
					}
				}
			}
		}

		public static T GetInterface<T>(this MonoBehaviour monoBehaviour)
		{
			T[] walkInterfaces = monoBehaviour.GetComponents<T>();
			for (int i = 0; i < walkInterfaces.Length; i++)
			{
				T inter = walkInterfaces[i];
				object interfaceObject = inter;
				if (ReferenceEquals(interfaceObject, monoBehaviour))
				{
					return inter;
				}
			}

			return default;
		}

		public static Type[] GetAllDerivedComponents(List<Type> result, Type componentType, bool allowAbstract = true)
		{
			if (result == null || componentType == null || !componentType.IsSubclassOf(typeof(Component)))
			{
				return null;
			}

			result.Clear();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (type.IsSubclassOf(componentType))
					{
						if (type.IsAbstract)
						{
							if (allowAbstract)
								result.Add(type);
						}
						else
						{
							result.Add(type);
						}
					}
				}
			}

			return result.ToArray();
		}

		public static void GetAllDerivedObjectsClass<T>(List<Type> result, bool allowAbstract = true) where T : class
		{
			if (result == null)
			{
				return;
			}

			result.Clear();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (type.IsSubclassOf(typeof(T)))
					{
						if (type.IsAbstract)
						{
							if (allowAbstract)
								result.Add(type);
						}
						else
						{
							result.Add(type);
						}
					}
				}
			}
		}

		const string EDITOR_ASSEMBLY_NAME = "UniverseEngine.Editor";

		public static Type GetEditorType(string className)
		{
			if (string.IsNullOrEmpty(className))
			{
				return null;
			}

			Assembly assembly = GetAssembly(EDITOR_ASSEMBLY_NAME);
			if (assembly == null)
			{
				return null;
			}

			string typeFullName = $"{EDITOR_ASSEMBLY_NAME}.{className}";
			return assembly.GetType(typeFullName);
		}

	#if UNITY_EDITOR

		/// <summary>
		/// 获取带继承关系的所有类的类型
		/// </summary>
		public static void GetTypesDerivedFrom<T>(List<Type> result)
		{
			GetTypesDerivedFrom(typeof(T), result);
		}

		/// <summary>
		/// 获取带继承关系的所有类的类型
		/// </summary>
		public static void GetTypesDerivedFrom(Type parentType, List<Type> result)
		{
			if (parentType == null || result == null)
			{
				return;
			}

			UnityEditor.TypeCache.TypeCollection collection = UnityEditor.TypeCache.GetTypesDerivedFrom(parentType);
			result.Clear();
			result.AddRange(collection);
		}

		public static MethodInfo[] GetMethods(this MonoBehaviour monoBehaviour)
		{
			UnityEditor.MonoScript monoScript = UnityEditor.MonoScript.FromMonoBehaviour(monoBehaviour);
			MethodInfo[] methods = monoScript.GetClass().GetMethods();
			return methods;
		}

		public static MethodInfo[] GetMethods(this ScriptableObject scriptableObject)
		{
			UnityEditor.MonoScript monoScript = UnityEditor.MonoScript.FromScriptableObject(scriptableObject);
			MethodInfo[] methods = monoScript.GetClass().GetMethods();
			return methods;
		}

	#endif

		public static TAttribute GetAttribute<TAttribute>(object from) where TAttribute : Attribute
		{
			Type fromType = from?.GetType();
			return (TAttribute)fromType?.GetCustomAttribute(typeof(TAttribute), false);
		}

		public static TAttribute GetAttribute<TAttribute, TFrom>() where TAttribute : Attribute
		{
			Type fromType = typeof(TFrom);
			return (TAttribute)fromType.GetCustomAttribute(typeof(TAttribute), false);
		}

		public static TAttribute GetAttribute<TAttribute>(Type fromType) where TAttribute : Attribute
		{
			return (TAttribute)fromType.GetCustomAttribute(typeof(TAttribute), false);
		}

		public static TAttribute GetAttribute<TAttribute>(MethodInfo fromMethod) where TAttribute : Attribute
		{
			return (TAttribute)fromMethod.GetCustomAttribute(typeof(TAttribute), false);
		}

		public static TAttribute GetAttribute<TAttribute>(FieldInfo fromField) where TAttribute : Attribute
		{
			return (TAttribute)fromField.GetCustomAttribute(typeof(TAttribute), false);
		}

		static Assembly GetAssembly(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}

			if (s_Assemblies.TryGetValue(name, out Assembly assembly))
			{
				return assembly;
			}

			assembly = Assembly.Load(name);
			if (assembly == null)
			{
				return null;
			}

			s_Assemblies[name] = assembly;
			return assembly;
		}

		static object InvokeMethod(Type type, string method, BindingFlags flags, params object[] args)
		{
			MethodInfo methodInfo = type.GetMethod(method, flags);
			if (methodInfo == null)
			{
				Log.Error($"{type.FullName} not found method : {method}");
				return null;
			}

			return methodInfo.Invoke(null, args);
		}
	}
}
