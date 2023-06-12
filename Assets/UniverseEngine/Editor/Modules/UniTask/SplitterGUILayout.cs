#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UniverseEngine.Editor
{
    // reflection call of UnityEditor.SplitterGUILayout
    internal static class SplitterGUILayout
    {
        const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        static readonly Lazy<Type> s_SplitterStateType = new(() =>
        {
            Type type = typeof(EditorWindow).Assembly.GetTypes().First(x => x.FullName == "UnityEditor.SplitterState");
            return type;
        });

        static readonly Lazy<ConstructorInfo> s_SplitterStateCtor = new(() =>
        {
            Type type = s_SplitterStateType.Value;
            return type.GetConstructor(FLAGS, null, new[] { typeof(float[]), typeof(int[]), typeof(int[]) }, null);
        });

        static readonly Lazy<Type> s_SplitterGUILayoutType = new(() =>
        {
            Type type = typeof(EditorWindow).Assembly.GetTypes().First(x => x.FullName == "UnityEditor.SplitterGUILayout");
            return type;
        });

        static readonly Lazy<MethodInfo> s_BeginVerticalSplit = new(() =>
        {
            Type type = s_SplitterGUILayoutType.Value;
            return type.GetMethod("BeginVerticalSplit", FLAGS, null, new[] { s_SplitterStateType.Value, typeof(GUILayoutOption[]) }, null);
        });

        static readonly Lazy<MethodInfo> s_EndVerticalSplit = new(() =>
        {
            Type type = s_SplitterGUILayoutType.Value;
            return type.GetMethod("EndVerticalSplit", FLAGS, null, Type.EmptyTypes, null);
        });

        public static object CreateSplitterState(float[] relativeSizes, int[] minSizes, int[] maxSizes)
        {
            return s_SplitterStateCtor.Value.Invoke(new object[] { relativeSizes, minSizes, maxSizes });
        }

        public static void BeginVerticalSplit(object splitterState, params GUILayoutOption[] options)
        {
            s_BeginVerticalSplit.Value.Invoke(null, new[] { splitterState, options });
        }

        public static void EndVerticalSplit()
        {
            s_EndVerticalSplit.Value.Invoke(null, Type.EmptyTypes);
        }
    }
}
