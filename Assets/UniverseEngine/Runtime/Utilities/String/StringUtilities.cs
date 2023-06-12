using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace UniverseEngine
{
    public static class StringUtilities
    {
        /// <summary>
        /// 不能为readonly
        /// </summary>
        [ThreadStatic]
        static StringBuilder s_CacheBuilder;

        public static bool HasChinese(this string str)
        {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }

        public static string Format(string format, object arg0)
        {
            if (string.IsNullOrEmpty(format))
            {
                return string.Empty;
            }
            
            return GetBuilder().AppendFormat(format, arg0).ToString();
        }

        public static string Format(string format, object arg0, object arg1)
        {
            if (string.IsNullOrEmpty(format))
            {
                return string.Empty;
            }

            return GetBuilder().AppendFormat(format, arg0, arg1).ToString();
        }

        public static string Format(string format, object arg0, object arg1, object arg2)
        {
            if (string.IsNullOrEmpty(format))
            {
                return string.Empty;
            }
    
            return GetBuilder().AppendFormat(format, arg0, arg1, arg2).ToString();
        }

        public static string Format(string format, params object[] args)
        {
            if (string.IsNullOrEmpty(format))
            {
                return string.Empty;
            }

            if (args == null)
            {
                return string.Empty;
            }
            
            return GetBuilder().AppendFormat(format, args).ToString();
        }

        public static string RemoveFirstChar(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            return str.Substring(1);
        }
        
        public static string RemoveExtension(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            int index = str.LastIndexOf(".", StringComparison.Ordinal);
            if (index == -1)
            {
                return str;
            }

            //"assets/config/test.unity3d" --> "assets/config/test"
            return str.Remove(index);
        }

        public static bool IsNullOrWhiteSpace(this string target)
        {
            if (target == null)
            {
                return true;
            }

            for (int i = 0; i < target.Length; i++)
            {
                if (target[i] != ' ')
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取paths列表对应路径
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string ToPath(params string[] paths)
        {
            if (paths == null)
            {
                return string.Empty;
            }

            string ret = string.Empty;
            for (int i = 0, next = 1; i < paths.Length && next < paths.Length; i++, next++)
            {
                string path = paths[i];
                if (next < paths.Length)
                {
                    ret = Path.Combine(path, paths[next]);
                }
            }

            return ret;
        }

        public static string Between(this string targetString, string firstString, string lastString)
        {
            int start = targetString.IndexOf(firstString, StringComparison.Ordinal) + firstString.Length;
            int end = targetString.IndexOf(lastString, StringComparison.Ordinal);
            if (end - start < 0)
            {
                return string.Empty;
            }

            return targetString.Substring(start, end - start);
        }

        public static int ParseInt(this string value)
        {
            int.TryParse(value, out int ret);
            return ret;
        }

        public static uint ParseUInt(this string value)
        {
            uint.TryParse(value, out uint ret);
            return ret;
        }

        public static bool ParseBool(this string value)
        {
            if (value.ToInt() > 0)
            {
                return true;
            }

            bool.TryParse(value, out bool ret);
            return ret;
        }

        public static long ParseInt64(this string value)
        {
            long.TryParse(value, out long ret);
            return ret;
        }

        public static ulong ParseUInt64(this string value)
        {
            ulong.TryParse(value, out ulong ret);
            return ret;
        }

        public static float ParseFloat(this string value)
        {
            float.TryParse(value, out float ret);
            return ret;
        }

        public static string SafeFormat(this string format, object args)
        {
            string ret = format;

            try
            {
                ret = string.Format(format, args);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + ex.StackTrace);
            }

            return ret;
        }

        public static string SafeFormat(this string format, object arg1, object arg2)
        {
            string ret = format;

            try
            {
                ret = string.Format(format, arg1, arg2);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + ex.StackTrace);
            }

            return ret;
        }

        public static string SafeFormat(this string format, object arg1, object arg2, object arg3)
        {
            string ret = format;

            try
            {
                ret = string.Format(format, arg1, arg2, arg3);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + ex.StackTrace);
            }

            return ret;
        }

        public static string SafeFormat(this string format, object arg1, object arg2, object arg3, object arg4)
        {
            string ret = format;

            try
            {
                ret = string.Format(format, arg1, arg2, arg3, arg4);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + ex.StackTrace);
            }

            return ret;
        }

        public static string BytesToString(byte[] buffer)
        {
            if (buffer == null)
            {
                return string.Empty;
            }

            string res = string.Empty;

            foreach (byte b in buffer)
            {
                res += string.Format("{0:X}", b / 16);
                res += string.Format("{0:X}", b % 16);
            }

            return res;
        }

        public static bool ToEnum<T>(string value, out T result) where T : struct, Enum
        {
            return value.ParseEnum(out result);
        }

        public static int ToInt(this string value)
        {
            return value.ParseInt();
        }

        public static bool ToBool(this string value)
        {
            return value.ParseBool();
        }

        public static long ToInt64(this string value)
        {
            return value.ParseInt64();
        }

        public static float ToFloat(this string value)
        {
            return value.ParseFloat();
        }

        public static List<string> StringToStringList(this string str, char separator)
        {
            List<string> result = new();
            if (string.IsNullOrEmpty(str))
            {
                return result;
            }

            string[] splits = str.Split(separator);
            foreach (string split in splits)
            {
                string value = split.Trim(); //移除首尾空格
                if (!string.IsNullOrEmpty(value))
                {
                    result.Add(value);
                }
            }
            return result;
        }

        /// <summary>
        /// 截取字符串
        /// 获取匹配到的后面内容
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="key">关键字</param>
        /// <param name="includeKey">分割的结果里是否包含关键字</param>
        /// <param name="firstMatch">是否使用初始匹配的位置，否则使用末尾匹配的位置</param>
        public static string Substring(string content, string key, bool includeKey, bool firstMatch = true)
        {
            if (string.IsNullOrEmpty(key))
                return content;

            int startIndex = firstMatch ? content.IndexOf(key, StringComparison.Ordinal) : //返回子字符串第一次出现位置		
                                 content.LastIndexOf(key, StringComparison.Ordinal);       //返回子字符串最后出现的位置

            // 如果没有找到匹配的关键字
            if (startIndex == -1)
                return content;

            if (includeKey)
            {
                return content[startIndex..];
            }

            return content[(startIndex + key.Length)..];
        }

        public static string ReadString(IReadOnlyList<byte> data, int maxLength)
        {
            List<byte> bytes = new();
            for (int i = 0; i < data.Count; i++)
            {
                if (i >= maxLength)
                {
                    break;
                }

                byte b = data[i];
                if (b == 0)
                {
                    break;
                }

                bytes.Add(b);
            }

            if (bytes.Count == 0)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        static StringBuilder GetBuilder()
        {
            s_CacheBuilder ??= new();
            s_CacheBuilder.Length = 0;
            return s_CacheBuilder;
        }
    }
}
