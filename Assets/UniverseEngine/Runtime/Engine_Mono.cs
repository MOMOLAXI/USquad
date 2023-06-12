using System.Collections;
using UnityEngine;

namespace UniverseEngine
{
    public static partial class UniverseEngine
    {
        /// <summary>
        /// 获取全局游戏对象
        /// </summary>
        /// <param name="name"></param>
        /// <param name="createIfAbsent"></param>
        /// <returns></returns>
        public static GameObject CreateGlobalGameObject(string name, bool createIfAbsent = true)
        {
            return InternalCreateGlobalGameObject(name, createIfAbsent);
        }

        public static void Attach(Component component, GameObject target)
        {
            if (component == null || target == null)
            {
                return;
            }

            component.transform.SetParent(target.transform);
        }

        public static T GetOrAddComponent<T>(Component component) where T : Component
        {
            if (component == null)
            {
                return null;
            }

            return GetOrAddComponent<T>(component.gameObject);
        }

        public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            if (gameObject == null)
            {
                return null;
            }

            if (gameObject.TryGetComponent(out T component))
            {
                return component;
            }

            return gameObject.AddComponent<T>();
        }

        public static bool GetOrAddGlobalComponent<T>(string gName, out T result, bool createIfAbsent = true) where T : Component
        {
            GameObject go = InternalCreateGlobalGameObject(gName, createIfAbsent);
            if (go.TryGetComponent(out result))
            {
                return true;
            }

            result = go.AddComponent<T>();
            return true;
        }

        /// <summary>
        /// 开启全局协程
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public static Coroutine StartGlobalCoroutine(IEnumerator routine)
        {
            return Root.StartCoroutine(routine);
        }

        /// <summary>
        /// 停止全局协程
        /// </summary>
        /// <param name="routine"></param>
        public static void StopGlobalCoroutine(Coroutine routine)
        {
            Root.StopCoroutine(routine);
        }

        static GameObject InternalCreateGlobalGameObject(string name, bool createIfAbsent = true)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new();
            }

            if (!createIfAbsent && s_Globals.TryGetValue(FormatName(name), out GameObject result))
            {
                return result;
            }

            result = new(FormatName(name));
            result.transform.SetParent(Root.transform);
            s_Globals[name] = result;
            return result;
        }
    }
}