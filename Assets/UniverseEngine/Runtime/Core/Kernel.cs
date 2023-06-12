using System.Collections.Generic;

namespace UniverseEngine
{
    internal static class Kernel
    {
        static readonly SystemContainer<EngineSystem> s_EngineSystems = new();
        static readonly SystemContainer<GameSystem> s_GameplaySystems = new();

        internal static T Create<T>() where T : ISystem, new()
        {
            return new();
        }

        internal static void Register(IList<EngineSystem> systems)
        {
            s_EngineSystems.Register(systems);
        }

        public static T RegisterEngineSystem<T>() where T : EngineSystem, new()
        {
            return s_EngineSystems.Register<T>();
        }

        internal static T GetEngineSystem<T>() where T : EngineSystem, new()
        {
            return s_EngineSystems.Get<T>();
        }

        public static void Register(IList<GameSystem> systems)
        {
            s_GameplaySystems.Register(systems);
        }

        public static T RegisterGameSystem<T>() where T : GameSystem, new()
        {
            return s_GameplaySystems.Register<T>();
        }

        internal static T GetGameSystem<T>() where T : GameSystem, new()
        {
            return s_GameplaySystems.Get<T>();
        }

        internal static void Init()
        {
            s_EngineSystems.PreInit();
            s_GameplaySystems.PreInit();
            s_EngineSystems.Init();
            s_GameplaySystems.Init();
        }

        internal static void Update(float dt)
        {
            s_EngineSystems.Update(dt);
            s_GameplaySystems.Update(dt);
        }

        internal static void FixedUpdate(float dt)
        {
            s_EngineSystems.FixedUpdate(dt);
            s_GameplaySystems.FixedUpdate(dt);
        }

        internal static void LateUpdate(float dt)
        {
            s_EngineSystems.LateUpdate(dt);
            s_GameplaySystems.LateUpdate(dt);
        }

        internal static void Reset()
        {
            s_EngineSystems.Reset();
            s_GameplaySystems.Reset();
        }

        internal static void Destroy()
        {
            s_EngineSystems.Destroy();
            s_GameplaySystems.Destroy();
        }

        internal static void ApplicationFocus(bool hasFocus)
        {
            s_EngineSystems.ApplicationFocus(hasFocus);
            s_GameplaySystems.ApplicationFocus(hasFocus);
        }

        internal static void ApplicationPause(bool pauseStatus)
        {
            s_EngineSystems.ApplicationPause(pauseStatus);
            s_GameplaySystems.ApplicationPause(pauseStatus);
        }

        internal static void ApplicationQuit()
        {
            s_EngineSystems.ApplicationQuit();
            s_GameplaySystems.ApplicationQuit();
        }
    }
}
