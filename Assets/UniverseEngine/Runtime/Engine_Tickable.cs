namespace UniverseEngine
{
    public static partial class UniverseEngine
    {
        /// <summary>
        /// 添加心跳时长函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <param name="function"></param>
        /// <param name="interval">间隔时间(秒)</param>
        /// <param name="duration">总时长(秒)</param>
        /// <returns></returns>
        public static void StartGlobalHeartBeat(
            string callbackName,
            GlobalHeartBeatFunction function,
            float interval,
            float duration = -1)
        {
            EngineSystem<HeartBeatSystem>.System.RegisterHeartBeat(callbackName, function, interval, duration);
        }

        /// <summary>
        /// 添加心跳计数函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <param name="function"></param>
        /// <param name="interval"></param>
        /// <param name="count"></param>
        public static void StartGlobalCountBeat(
            string callbackName,
            GlobalCountBeatFunction function,
            float interval,
            int count = -1)
        {
            EngineSystem<HeartBeatSystem>.System.RegisterCountBeat(callbackName, function, count, interval);
        }

        /// <summary>
        /// 查找心跳函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <returns></returns>
        public static HeartBeat FindHeartBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error("callback is can not be null or empty");
                return null;
            }

            return EngineSystem<HeartBeatSystem>.System.FindHeartBeat(callbackName);
        }

        /// <summary>
        /// 查找心跳计数函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <returns></returns>
        public static CountBeat FindCountBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error("callback is can not be null or empty");
                return null;
            }

            return EngineSystem<HeartBeatSystem>.System.FindCountBeat(callbackName);
        }

        /// <summary>
        /// 存在心跳函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <returns></returns>
        public static bool ContainsHeartBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                return false;
            }

            return EngineSystem<HeartBeatSystem>.System.ContainsHeartBeat(callbackName);
        }

        /// <summary>
        /// 查找心跳计数函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <returns></returns>
        public static bool ContainsCountBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                return false;
            }

            return EngineSystem<HeartBeatSystem>.System.ContainsCountBeat(callbackName);
        }

        /// <summary>
        /// 暂停心跳函数
        /// </summary>
        /// <param name="callbackName"></param>
        public static void PauseHeartBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error("callback is can not be null or empty");
                return;
            }

            EngineSystem<HeartBeatSystem>.System.PauseHeartBeat(callbackName);
        }

        /// <summary>
        /// 暂停心跳计数函数
        /// </summary>
        /// <param name="callbackName"></param>
        public static void PauseCountBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error("callback is can not be null or empty");
                return;
            }

            EngineSystem<HeartBeatSystem>.System.PauseCountBeat(callbackName);
        }

        /// <summary>
        /// 移除心跳函数
        /// </summary>
        /// <param name="callbackName"></param>
        public static void RemoveHeartBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error("callback is can not be null or empty");
                return;
            }

            EngineSystem<HeartBeatSystem>.System.RemoveHeartBeat(callbackName);
        }

        /// <summary>
        /// 移除心跳计数函数
        /// </summary>
        /// <param name="callbackName"></param>
        public static void RemoveCountBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error("callback is can not be null or empty");
                return;
            }

            EngineSystem<HeartBeatSystem>.System.RemoveCountBeat(callbackName);
        }
    }
}