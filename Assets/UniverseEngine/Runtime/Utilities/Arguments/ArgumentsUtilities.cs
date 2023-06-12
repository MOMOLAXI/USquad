using System;

namespace UniverseEngine
{
    public static class ArgumentsUtilities
    {
        /// <summary>
        /// 查询对象参数
        /// </summary>
        /// <param name="args"></param>
        /// <param name="index"></param>
        /// <param name="defaultVal"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T QueryClassArgs<T>(object[] args, int index, T defaultVal = default(T)) where T : class
        {
            if (args == null || !Collections.IsValidIndex(args, index))
            {
                return defaultVal;
            }

            if (args[index] is T t)
            {
                return t;
            }

            return defaultVal;
        }

        public static string ToString(this object obj, string defaultVal = "")
        {
            return obj switch
            {
                null => defaultVal,
                string stringValue => stringValue,
                _ => obj.ToString()
            };
        }

        public static float ToFloat(this object obj, float defaultVal = 0f)
        {
            return obj switch
            {
                null => defaultVal,
                float result => float.IsNaN(result) ? defaultVal : result,
                int i => i,
                long l => l,
                string s => s.ParseFloat(),
                char c => c,
                short sh => sh,
                byte b => b,
                sbyte sb => sb,
                uint u => u,
                ulong ul => ul,
                _ => defaultVal
            };
        }

        public static long ToLong(this object obj, long defaultVal = 0)
        {
            return obj switch
            {
                long l => l,
                int i => i,
                string s => s.ParseInt64(),
                char c => c,
                short sh => sh,
                byte b => b,
                sbyte sb => sb,
                uint u => u,
                _ => defaultVal
            };
        }

        public static int ToInt(this object obj, int defaultVal = 0)
        {
            return obj switch
            {
                int i => i,
                Enum enumValue => Convert.ToInt32(enumValue),
                string s1 => s1.ParseInt(),
                char c => c,
                short s => s,
                byte b => b,
                sbyte @sbyte => @sbyte,
                long l => (int)l,
                _ => defaultVal
            };
        }
    }
}
