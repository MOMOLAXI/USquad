namespace UniverseEngine
{
    public class CountBeat : ICacheAble
    {
        /// <summary>
        /// 心跳名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 心跳计数函数
        /// </summary>
        public GlobalCountBeatFunction Function;

        /// <summary>
        /// 记录开始的时间点
        /// </summary>
        public float LastTime = -1;

        /// <summary>
        /// 心跳次数
        /// </summary>
        public int BeatCount = -1;

        /// <summary>
        /// 当前计数
        /// </summary>
        public int CurCount;

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
            Function?.Invoke(CurCount);
        }

        public bool IsInCache { get; set; }

        public void Reset()
        {
            Name = string.Empty;
            Function = null;
            LastTime = -1;
            BeatCount = -1;
            CurCount = default;
            Interval = default;
            IntervalRunningTime = default;
            State = EHearBeatState.Idle;
        }
    }
}
