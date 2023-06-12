using UnityEngine.SceneManagement;

namespace UniverseEngine
{
    internal class SceneSystem : EngineSystem
    {
        public static string ScenePackageName = string.Empty;

        public static EntityID CurrentScene { get; private set; }

        public override void OnInit()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        public static async UniTask<bool> CreateScene(string sceneName)
        {
            if (string.IsNullOrEmpty(ScenePackageName))
            {
                Log.Error("Scene package name is null or empty, please set scene package name before CreateScene");
                return false;
            }

            if (string.IsNullOrEmpty(sceneName))
            {
                Log.Error("SceneName is null or empty , can not create");
                return false;
            }

            if (CurrentScene.IsInvalid)
            {
                CurrentScene.Destroy();
            }

            // Scene scene = await Engine.LoadSceneAsync(ScenePackageName, sceneName);
            // CurrentScene = Engine.CreateEntity("Scene", int.MaxValue, true);
            // CurrentScene.SetString("Name", scene.name);
            // CurrentScene.SetInt("Handle", scene.handle);
            // CurrentScene.AddPropertyHook("Name","OnSceneNameChange", OnSceneNameChange);
            return true;
        }
        static void OnSceneNameChange(EntityID self, string property, Var last, Var cur)
        {
            Log.Info($"{cur.GetString()}");
        }

        static void OnSceneUnloaded(Scene scene)
        {
            Log.Info($"Scene unloaded {scene.name}");
            UISystem.CloseAll(true);
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log.Info($"Scene loaded {scene.name}, load mode {mode.ToString()}");
        }

        static void OnActiveSceneChanged(Scene from, Scene to)
        {
            if (!string.IsNullOrEmpty(from.name))
            {
                Log.Info($"Scene changed from {from.name} to {to.name}");
            }
        }
    }
}
