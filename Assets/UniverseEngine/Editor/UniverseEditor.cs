using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.SceneManagement;

namespace UniverseEngine.Editor
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    [InitializeOnLoad]
    public partial class UniverseEditor
    {
        public const string EDITOR_ROOT_PATH = "Assets/Editor";
        static readonly Stopwatch s_Watcher = Stopwatch.StartNew();

        static readonly Queue<Action> s_QueuedActions = new();
        static readonly Dictionary<Type, UniverseEditorModule> s_EditorModules = new();

        static UniverseEditor()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        [InitializeOnLoadMethod]
        static void InitializeWorkSpace()
        {
            FileSystem.CreateDirectory(EDITOR_ROOT_PATH);
            InitializeEditorModules();
        }

        static void InitializeEditorModules()
        {
            s_EditorModules.Clear();
            RegisterModule<WindowModule>();
            RegisterModule<SettingModule>();
            RegisterModule<ShaderModule>();
            RegisterModule<AssetBuildModule>();
            RegisterModule<AssetCollectModule>();
            RegisterModule<AssetDebugModule>();
            RegisterModule<AssetDebugModule>();
        }
        
        static void OnEditorUpdate()
        {
            while (s_QueuedActions.Count > 0)
            {
                Action action = s_QueuedActions.Dequeue();
                action?.Invoke();
            }

            foreach (UniverseEditorModule module in s_EditorModules.Values)
            {
                module.Update();
            }
        }

        [DidReloadScripts]
        static void OnReloadeAssembly()
        {
            foreach (UniverseEditorModule module in s_EditorModules.Values)
            {
                module.ReloadAssembly();
            }

            EditorLog.Info($"Compile Successfully {TimeUtilities.Now()}");
        }

        /// <summary>
        /// 延迟执行, 有些操作会引起编辑器(Inspector, EditorWindow的绘制)报错, 类似EditorApplication.delayCall)
        /// </summary>
        /// <param name="action"></param>
        public static void ExecuteDelay(Action action)
        {
            if (action == null)
            {
                return;
            }

            s_QueuedActions.Enqueue(action);
        }

        public static string CreateEditorDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
            {
                return string.Empty;
            }

            string directory = Path.Combine(EDITOR_ROOT_PATH, directoryName);
            FileSystem.CreateDirectory(directory);
            return directory;
        }

        public static string GetEditorDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
            {
                return string.Empty;
            }
            
            string directory = FileSystem.ToPath(EDITOR_ROOT_PATH, directoryName);
            if (!Directory.Exists(directory))
            {
                CreateEditorDirectory(directoryName);
            }

            return directory;
        }

        public static T GetModule<T>() where T : UniverseEditorModule
        {
            return UniverseEditorSystem<T>.Module;
        }

        public static void Run(Run run, out float seconds)
        {
            seconds = 0;
            s_Watcher.Reset();
            s_Watcher.Start();
            try
            {
                run?.Invoke();
            }
            catch (Exception ex)
            {
                EditorLog.Exception(ex);
            }
            s_Watcher.Stop();
            seconds = s_Watcher.ElapsedMilliseconds / 1000.0f;
        }

        static void RegisterModule<T>() where T : UniverseEditorModule, new()
        {
            T module = UniverseEditorModule.Create<T>();
            module.Init();
            s_EditorModules[typeof(T)] = module;
            UniverseEditorSystem<T>.Module = module;
        }

        public static bool HasDirtyScenes()
        {
            int sceneCount = SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                {
                    return true;
                }
            }

            return false;
        }

        private static class UniverseEditorSystem<T> where T : UniverseEditorModule
        {
            public static T Module;
        }
    }
}
