using UnityEngine;

namespace UniverseEngine
{
    public class HeartBeat : ICacheAble
    {
        /// <summary>
        /// 心跳名称
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// 心跳函数
        /// </summary>
        public GlobalHeartBeatFunction Function;

        /// <summary>
        /// 记录开始的时间点
        /// </summary>
        public float LastTime = -1;

        /// <summary>
        /// 运行总时长
        /// </summary>
        public float Duration;

        /// <summary>
        /// 心跳间隔
        /// </summary>
        public float Interval;

        /// <summary>
        /// 已经走过的间隔时间
        /// </summary>
        public float IntervalRunningTime;

        /// <summary>
        /// 执行状态
        /// </summary>
        public EHearBeatState State = EHearBeatState.Idle;

        public void Invoke()
        {
            Function?.Invoke(Time.time - LastTime);
        }

        public bool IsInCache { get; set; }
        public void Reset()
        {
            Name = string.Empty;
            Function = null;
            LastTime = -1;
            Duration = default;
            Interval = default;
            IntervalRunningTime = default;
            State = EHearBeatState.Idle;
        }
    }
}
