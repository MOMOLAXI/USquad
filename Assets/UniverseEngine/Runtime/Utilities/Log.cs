using System;
using UnityEngine;

namespace UniverseEngine
{
    public enum ELogLevel
    {
        None,
        Exception,
        Error,
        Warning,
        Info,
        Debug,
        Max,
    }

    public static class Log<T>
    {
        static readonly Log s_Console = Log.CreateSession<T>();

        public static void Info(object message)
        {
            s_Console.OutputInfo(message);
        }

        public static void Warning(object message)
        {
            s_Console.OutputWarning(message);
        }

        public static void Error(object message)
        {
            s_Console.OutputError(message);
        }

        public static void Exception(Exception exception)
        {
            s_Console.OutputException(exception);
        }
    }

    public class Log
    {
        const string OUTPUT_FORMAT = "{0}{1}";
        public readonly string Tag;

        /// <summary>
        /// 日志等级
        /// 修改等级来屏蔽日志
        /// </summary>
        public static ELogLevel LogLevel { get; set; } = ELogLevel.Max;

        /// <summary>
        /// 日志前缀
        /// </summary>
        const string RUNTIME_LOG_TAG = "[<color=#33CC33>UniverseEngine</color>]";

        public static Log CreateSession<T>()
        {
            Type type = typeof(T);
            return new(ColorHelper.GetStringRichColorText(type.Name));
        }

        internal Log(string tag)
        {
            Tag = tag;
        }

        public void OutputInfo(object message)
        {
            Info(Tag, message);
        }

        public void OutputWarning(object message)
        {
            Warning(Tag, message);
        }

        public void OutputError(object message)
        {
            Error(Tag, message);
        }

        public void OutputException(Exception exception)
        {
            Exception(Tag, exception);
        }

        public static void Info(object message)
        {
            Info(RUNTIME_LOG_TAG, message);
        }

        public static void Info(string tag, object message)
        {
            if (LogLevel < ELogLevel.Error)
            {
                return;
            }

            Debug.Log(StringUtilities.Format(OUTPUT_FORMAT, tag, message));
        }

        public static void Warning(object message)
        {
            Warning(RUNTIME_LOG_TAG, message);
        }

        public static void Warning(string tag, object message)
        {
            if (LogLevel < ELogLevel.Error)
            {
                return;
            }

            Debug.LogError(StringUtilities.Format(OUTPUT_FORMAT, tag, message));
        }

        public static void Error(object message)
        {
            Error(RUNTIME_LOG_TAG, message);
        }

        public static void Error(string tag, object message)
        {
            if (LogLevel < ELogLevel.Error)
            {
                return;
            }

            Debug.LogError(StringUtilities.Format(OUTPUT_FORMAT, tag, message));
        }

        public static void Exception(Exception exception)
        {
            if (LogLevel < ELogLevel.Exception)
            {
                return;
            }

            Exception(RUNTIME_LOG_TAG, exception);
        }

        public static void Exception(string tag, Exception exception)
        {
            Debug.LogError($"================={tag} Exception Start ===============");
            Debug.LogException(exception);
            Debug.LogError($"================={tag} Exception End ===============");
        }
    }

}
