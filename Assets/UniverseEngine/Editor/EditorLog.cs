using System;
using UnityEngine;

namespace UniverseEngine.Editor
{
    public static class EditorLog
    {
        public static void Info(object message)
        {
            Log<UniverseEditor>.Info(message);
        }

        public static void Warning(object message)
        {
            Log<UniverseEditor>.Warning(message);
        }

        public static void Error(object message)
        {
            Log<UniverseEditor>.Error(message);
        }

        public static void Exception(Exception ex)
        {
            Log<UniverseEditor>.Exception(ex);
        }
    }
}