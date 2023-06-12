using System.Collections.Generic;
using UnityEngine;

namespace UniverseEngine
{
    public class AndroidSystem : EngineSystem
    {
        public const string UNITY_PLAYER_CLASS_NAME = "com.unity3d.player.UnityPlayer";
        public const string UNITY_CURRENT_ANDROID_ACTIVITY = "currentActivity";
        
        static readonly Dictionary<string, AndroidJavaClass> s_AndroidClasses = new();

        public override void OnInit()
        {
            if (!FileSystem.IsAndroidPlatform())
            {
                return;
            }

            RegisterAndroidClass(UNITY_PLAYER_CLASS_NAME);
        }

        public static T InvokeCurrentActivityStaticMethod<T>(string methodName, params object[] args)
        {
            return InvokeUnityPlayerStaticMethod<T>(UNITY_CURRENT_ANDROID_ACTIVITY, methodName, args);
        }

        public static T InvokeCurrentActivityInstanceMethod<T>(string methodName, params object[] args)
        {
            return InvokeUnityPlayerInstanceMethod<T>(UNITY_CURRENT_ANDROID_ACTIVITY, methodName, args);
        }

        public static T InvokeUnityPlayerInstanceMethod<T>(string objectName, string methodName, params object[] args)
        {
            return Call<T>(UNITY_PLAYER_CLASS_NAME, objectName, methodName, args);
        }

        public static T InvokeUnityPlayerStaticMethod<T>(string objectName, string methodName, params object[] args)
        {
            return CallStatic<T>(UNITY_PLAYER_CLASS_NAME, objectName, methodName, args);
        }

        public static T Call<T>(string className, string objectName, string methodName, params object[] args)
        {
            if (string.IsNullOrEmpty(className) || string.IsNullOrEmpty(methodName) || string.IsNullOrEmpty(objectName))
            {
                return default;
            }

            AndroidJavaObject targetObject = GetAndroidObject<AndroidJavaObject>(className, objectName);
            if (targetObject == null)
            {
                return default;
            }

            return targetObject.Call<T>(methodName, args);
        }

        public static T CallStatic<T>(string className, string objectName, string methodName, params object[] args)
        {
            if (string.IsNullOrEmpty(className) || string.IsNullOrEmpty(methodName) || string.IsNullOrEmpty(objectName))
            {
                return default;
            }

            AndroidJavaObject targetObject = GetAndroidObject<AndroidJavaObject>(className, objectName);
            if (targetObject == null)
            {
                return default;
            }

            return targetObject.CallStatic<T>(methodName, args);
        }

        public static AndroidJavaClass GetClass(string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                Log<AndroidSystem>.Error("Android class name is null or empty");
                return null;
            }

            if (s_AndroidClasses.TryGetValue(className, out AndroidJavaClass androidJavaClass))
            {
                return androidJavaClass;
            }

            return null;
        }

        static void RegisterAndroidClass(string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                Log<AndroidSystem>.Error("Android class name is null or empty, can not register");
                return;
            }

            AndroidJavaClass androidJavaClass = new(className);
            s_AndroidClasses[className] = androidJavaClass;
        }

        static T GetAndroidObject<T>(string className, string objectName)
        {
            if (string.IsNullOrEmpty(className))
            {
                return default;
            }

            if (s_AndroidClasses.TryGetValue(className, out AndroidJavaClass androidJavaClass))
            {
                return androidJavaClass.GetStatic<T>(objectName);
            }

            return default;
        }
    }
}
