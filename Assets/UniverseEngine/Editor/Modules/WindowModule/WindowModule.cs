using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace UniverseEngine.Editor
{
	public class WindowModule : UniverseEditorModule
	{
		static readonly Dictionary<Type, string> s_Uxmls = new();

		/// <summary>
		/// 清空控制台
		/// </summary>
		public static void ClearUnityConsole()
		{
			ReflectionUtilities.Public(nameof(UnityEditor), "LogEntries", "Clear");
		}

		public static void FocusUnitySceneWindow()
		{
			EditorWindow.FocusWindowIfItsOpen<SceneView>();
		}

		public static void CloseUnityGameWindow()
		{
			CloseUnityWindow("GameView");
		}

		public static void FocusUnityGameWindow()
		{
			FocusUnityWindow("GameView");
		}

		public static void FocueUnityProjectWindow()
		{
			FocusUnityWindow("ProjectBrowser");
		}

		public static void FocusUnityHierarchyWindow()
		{
			FocusUnityWindow("SceneHierarchyWindow");
		}

		public static void FocusUnityInspectorWindow()
		{
			FocusUnityWindow("InspectorWindow");
		}

		public static void FocusUnityConsoleWindow()
		{
			FocusUnityWindow("ConsoleWindow");
		}

		public static T Open<T>(string title, bool focus, params Type[] dockTypes) where T : UniverseEditorWindow
		{
			T window = Open<T>(title, dockTypes);
			if (focus)
			{
				window.Focus();
			}

			return window;
		}

		public static T Open<T>(string title, params Type[] dockTypes) where T : UniverseEditorWindow
		{
			T window = Open<T>(dockTypes);
			window.titleContent = new(title);
			return window;
		}

		public static T Open<T>(params Type[] dockTypes) where T : UniverseEditorWindow
		{
			return Open<T>(false, dockTypes);
		}

		public static T Open<T>(bool immediateDisplay = false, params Type[] dockTypes) where T : UniverseEditorWindow
		{
			T window = FindWindow<T>(dockTypes);
			window.Show(immediateDisplay);
			return window;
		}

		public static void Close<T>() where T : UniverseEditorWindow
		{
			T window = FindWindow<T>();
			window.Close();
		}

		public static void Focus<T>() where T : EditorWindow
		{
			EditorWindow.FocusWindowIfItsOpen<T>();
		}

		public static void Focus(Type type)
		{
			EditorWindow.FocusWindowIfItsOpen(type);
		}

		public static void FocusUnityWindow(string windowClassName)
		{
			if (string.IsNullOrEmpty(windowClassName))
			{
				return;
			}

			Type windowType = GetUnityWindowType(windowClassName);
			if (windowType == null)
			{
				return;
			}

			EditorWindow.GetWindow(windowType, false, windowClassName, true);
		}

		public static void CloseUnityWindow(string windowClassName)
		{
			if (string.IsNullOrEmpty(windowClassName))
			{
				return;
			}

			Type windowType = GetUnityWindowType(windowClassName);
			if (windowType == null)
			{
				return;
			}

			EditorWindow.GetWindow(windowType, false, windowClassName, true).Close();
		}

		/// <summary>
		/// 打开搜索面板
		/// </summary>
		/// <param name="title">标题名称</param>
		/// <param name="defaultPath">默认搜索路径</param>
		/// <param name="defaultName"></param>
		/// <returns>返回选择的文件夹绝对路径，如果无效返回NULL</returns>
		public static string OpenFolderPanel(string title, string defaultPath, string defaultName = "")
		{
			string openPath = EditorUtility.OpenFolderPanel(title, defaultPath, defaultName);
			if (string.IsNullOrEmpty(openPath))
				return null;

			if (openPath.Contains("/Assets") == false)
			{
				EditorLog.Warning("Please select unity assets folder.");
				return null;
			}
			return openPath;
		}

		/// <summary>
		/// 打开搜索面板
		/// </summary>
		/// <param name="title">标题名称</param>
		/// <param name="defaultPath">默认搜索路径</param>
		/// <param name="extension"></param>
		/// <returns>返回选择的文件绝对路径，如果无效返回NULL</returns>
		public static string OpenFilePath(string title, string defaultPath, string extension = "")
		{
			string openPath = EditorUtility.OpenFilePanel(title, defaultPath, extension);
			if (string.IsNullOrEmpty(openPath))
				return null;

			if (!openPath.Contains("/Assets"))
			{
				EditorLog.Warning("Please select unity assets file.");
				return null;
			}
			return openPath;
		}

		/// <summary>
		/// 显示进度框
		/// </summary>
		public static void DisplayProgressBar(string tips, int progressValue, int totalValue)
		{
			EditorUtility.DisplayProgressBar("进度", $"{tips} : {progressValue}/{totalValue}", (float)progressValue / totalValue);
		}

		/// <summary>
		/// 隐藏进度框
		/// </summary>
		public static void ClearProgressBar()
		{
			EditorUtility.ClearProgressBar();
		}

		/// <summary>
		/// 加载窗口的布局文件
		/// </summary>
		public static VisualTreeAsset LoadWindowUxml<TWindow>() where TWindow : class
		{
			Type windowType = typeof(TWindow);

			// 缓存里查询并加载
			if (s_Uxmls.TryGetValue(windowType, out string uxmlGuid))
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(uxmlGuid);
				if (string.IsNullOrEmpty(assetPath))
				{
					s_Uxmls.Clear();
					throw new($"Invalid UXML GUID : {uxmlGuid} ! Please close the window and open it again !");
				}

				VisualTreeAsset treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
				return treeAsset;
			}

			// 全局搜索并加载
			string[] guids = AssetDatabase.FindAssets(windowType.Name);
			if (guids.Length == 0)
			{
				throw new($"Not found any assets : {windowType.Name}");
			}

			foreach (string assetGuid in guids)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
				Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(VisualTreeAsset))
				{
					s_Uxmls.Add(windowType, assetGuid);
					VisualTreeAsset treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
					return treeAsset;
				}
			}

			throw new($"Not found UXML file : {windowType.Name}");
		}

		static T FindWindow<T>(params Type[] dockWindows) where T : UniverseEditorWindow
		{
			return EditorWindow.GetWindow<T>(dockWindows);
		}

		static Type GetUnityWindowType(string windowClassName)
		{
			if (string.IsNullOrEmpty(windowClassName))
			{
				return null;
			}

			return ReflectionUtilities.GetTypeInAssembly("UnityEditor", $"UnityEditor.{windowClassName}");
		}
	}
}
