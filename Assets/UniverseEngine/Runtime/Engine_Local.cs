﻿using UnityEngine;

namespace UniverseEngine
{
    public static partial class UniverseEngine
    {
        public static int GetInt(string key)
        {
            return string.IsNullOrEmpty(key) ? default : PlayerPrefs.GetInt(Key(key));
        }

        public static float GetFloat(string key)
        {
            return string.IsNullOrEmpty(key) ? default : PlayerPrefs.GetFloat(Key(key));
        }

        public static string GetString(string key)
        {
            return string.IsNullOrEmpty(key) ? default : PlayerPrefs.GetString(Key(key));
        }

        public static void Set(string key, int value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            PlayerPrefs.SetInt(Key(key), value);
        }

        public static void Set(string key, float value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            PlayerPrefs.SetFloat(Key(key), value);
        }

        public static void Set(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            PlayerPrefs.SetString(Key(key), value);
        }

        static string Key(string value)
        {
            return ENGINE_PREFIX.SafeFormat(string.IsNullOrEmpty(value) ? "Null" : value);
        }

    }
}