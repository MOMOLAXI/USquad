using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniverseEngine
{
	public static partial class UniverseEngine
	{
		public const string ENGINE_PREFIX = "[UniverseEngine::{0}]";

		static Universe Root { get; set; }
		static readonly Dictionary<string, GameObject> s_Globals = new();
		public static string BuildGuid { get; private set; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void Initialize()
		{
			Root = CreateEngineRoot();
			Kernel.Register(EngineUnits.CoreUnits);
			Kernel.Register(EngineUnits.CustomUnits);
			Kernel.Init();
			LoadApplicationFootPrint().Forget();
			UniTaskScheduler.UnobservedTaskException += UniTaskSchedulerOnUnobservedTaskException;
		}

		/// <summary>
		/// 引擎初始化
		/// </summary>
		public static void Initialize<T>(T localSystem) where T : InitializeArguments
		{
			if (Root != null)
			{
				return;
			}

			Root = CreateEngineRoot();
			Function.Run(Start, localSystem, out double s1);
			Log.Info($"Initialize [UniverseEngine] ... using {s1} seconds");
			LoadApplicationFootPrint().Forget();
		}

		/// <summary>
		/// 注册游戏系统
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public static void RegisterGameSystem<T>() where T : GameSystem, new()
		{
			Kernel.RegisterGameSystem<T>();
		}

		/// <summary>
		/// 获取游戏系统
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T GetSystem<T>() where T : GameSystem, new()
		{
			return GameSystem<T>.System;
		}

		internal static void Start<T>(T localSystem) where T : InitializeArguments
		{
			localSystem ??= new InitializeArguments() as T;
			Kernel.Register(EngineUnits.CoreUnits);
			Kernel.Register(EngineUnits.CustomUnits);
			localSystem.RegisterGameSystems();
			Kernel.Init();

			try
			{
				Log.LogLevel = localSystem.LogLevel;
				Application.targetFrameRate = localSystem.TargetFrameRate;
				localSystem.RetgisterUIWidgets();
				// InitializeUISystem(localSystem.UIPackageName, localSystem.Resolutions);
				InitializeSceneSystem(localSystem.ScenePackageName);
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
		}

		internal static void Reset()
		{
			Kernel.Reset();
		}

		internal static void Update()
		{
			Kernel.Update(Time.deltaTime);
		}

		internal static void FixedUpdate()
		{
			Kernel.FixedUpdate(Time.fixedTime);
		}

		internal static void LateUpdate()
		{
			Kernel.LateUpdate(Time.deltaTime);
		}

		internal static void Destroy()
		{
			Kernel.Destroy();
		}

		internal static void ApplicationFocus(bool hasFocus)
		{
			Kernel.ApplicationFocus(hasFocus);
		}

		internal static void ApplicationPause(bool pauseStatus)
		{
			Kernel.ApplicationPause(pauseStatus);
		}

		internal static void ApplicationQuit()
		{
			Kernel.ApplicationQuit();
		}

		static string FormatName(string target)
		{
			return $"[{target}]";
		}

		static Universe CreateEngineRoot()
		{
			GameObject result = new(FormatName(nameof(Universe)));
			Object.DontDestroyOnLoad(result);
			return result.AddComponent<Universe>();
		}

		static async UniTaskVoid LoadApplicationFootPrint()
		{
			BuildGuid = await LoadFootPrint();
			string targetFootPrint = string.Empty;
		#if UNITY_EDITOR
			targetFootPrint = Application.version;
		#else
			targetFootPrint = Application.buildGUID;
		#endif

			if (BuildGuid != targetFootPrint)
			{
				FileSystem.DeleteManifestRoot();
				await CoverageFootPrint(targetFootPrint);
				Log.Info($"Foot print updated [{BuildGuid}] => [{targetFootPrint}], delete manifest files");
				BuildGuid = targetFootPrint;
			}
		}

		static async UniTask<string> LoadFootPrint()
		{
			string footPrint = string.Empty;
			string footPrintPath = FileSystem.GetAppFootPrintFilePath();
			if (File.Exists(footPrintPath))
			{
				footPrint = await FileSystem.ReadAllTextAsync(footPrintPath);
				Log.Info($"Read Application foot print : {footPrint}");
			}
			else
			{
				footPrint = "Unknown";
				Log.Info($"Foot print not exist at path : {footPrintPath}");
			}

			return footPrint;
		}

		static void UniTaskSchedulerOnUnobservedTaskException(Exception exception)
		{
			Log.Exception(exception);
		}
		
		static async UniTask CoverageFootPrint(string footPrint)
		{
			string footPrintPath = FileSystem.GetAppFootPrintFilePath();
			await FileSystem.CreateFileAsync(footPrintPath, footPrint);
			Log.Info($"Save Application foot print : {footPrint} at : {footPrintPath}");
		}

		internal static T GetEngineSystem<T>() where T : EngineSystem, new()
		{
			return EngineSystem<T>.System;
		}

		static class EngineSystem<T> where T : EngineSystem, new()
		{
			static T s_Instance;
			public static T System => s_Instance ??= Kernel.GetEngineSystem<T>();
		}

		static class GameSystem<T> where T : GameSystem, new()
		{
			static T s_Instance;
			public static T System => s_Instance ??= Kernel.GetGameSystem<T>();
		}
	}
}
