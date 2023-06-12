using System.Diagnostics;

namespace UniverseEngine
{
    /// <summary>
    /// 生命周期由MessageSystem控制
    /// </summary>
    public struct Args
    {
        /// <summary>
        /// 最大缓存数量100个, 每帧消息传递参数批次在100次, 超出的直接丢弃去GC
        /// </summary>
        const int MAX_CACHE_COUNT = 100;
        static readonly LinkedPool<Variables> s_VariablePool = new(() => new(), null, OnVariableRelease, null, true, MAX_CACHE_COUNT);

        bool m_IsEmpty;
        Variables m_VariableList;
        Variables VariableList
        {
            get
            {
                if (m_VariableList != null)
                {
                    return m_VariableList;
                }

                m_VariableList = Alloc();
                return m_VariableList;
            }
        }

        public static Args Empty = new();
        public bool IsValid => !m_IsEmpty;
        public int Count => VariableList.Count;

        public static Args InStream()
        {
            return new();
        }

        public Args Clone()
        {
            Args messageArgs = new();
            messageArgs.SetContent(VariableList.Clone());
            return messageArgs;
        }

        public bool Insert(int index, Var value)
        {
            if (index < 0 || index > VariableList.Count)
            {
                return false;
            }

            VariableList.Insert(index, value);
            return true;
        }

        public void RemoveAt(int index)
        {
            VariableList.RemoveAt(index);
        }

        public VarType GetVarType(int index)
        {
            return VariableList.GetVarType(index);
        }

        public Var GetVar(int index)
        {
            return VariableList.GetVar(index);
        }

        public bool GetBool(int index)
        {
            return VariableList.GetBool(index);
        }

        public int GetInt32(int index)
        {
            return VariableList.GetInt32(index);
        }

        public long GetInt64(int index)
        {
            return VariableList.GetInt64(index);
        }

        public float GetFloat(int index)
        {
            return VariableList.GetFloat(index);
        }

        public double GetDouble(int index)
        {
            return VariableList.GetDouble(index);
        }

        public string GetString(int index)
        {
            return VariableList.GetString(index);
        }

        public EntityID GetEntity(int index)
        {
            return VariableList.GetEntity(index);
        }

        public T GetObject<T>(int index) where T : class
        {
            return VariableList.GetObject<T>(index);
        }

        public byte[] GetBinary(int index)
        {
            return VariableList.GetBinary(index);
        }

        public static Args operator <(Args messageArgs, object value)
        {
            messageArgs.VariableList.AddObject(value);
            return messageArgs;
        }

        public static Args operator <(Args messageArgs, Var value)
        {
            messageArgs.VariableList.AddVariable(value);
            return messageArgs;
        }

        public static Args operator <(Args messageArgs, bool value)
        {
            messageArgs.VariableList.AddBool(value);
            return messageArgs;
        }

        public static Args operator <(Args messageArgs, int value)
        {
            messageArgs.VariableList.AddInt(value);
            return messageArgs;
        }

        public static Args operator <(Args messageArgs, long value)
        {
            messageArgs.VariableList.AddInt64(value);
            return messageArgs;
        }

        public static Args operator <(Args messageArgs, float value)
        {
            messageArgs.VariableList.AddFloat(value);
            return messageArgs;
        }

        public static Args operator <(Args messageArgs, double value)
        {
            messageArgs.VariableList.AddDouble(value);
            return messageArgs;
        }

        public static Args operator <(Args messageArgs, string value)
        {
            messageArgs.VariableList.AddString(value);
            return messageArgs;
        }

        public static Args operator <(Args messageArgs, EntityID value)
        {
            messageArgs.VariableList.AddEntityID(value);
            return messageArgs;
        }

        public static Args operator <(Args messageArgs, byte[] value)
        {
            messageArgs.VariableList.AddBinary(value);
            return messageArgs;
        }

        public static Args operator <(Args messageArgs, Args value)
        {
            messageArgs.VariableList.AddVariableList(value.VariableList);
            return messageArgs;
        }

        public static Args operator >(Args messageArgs, Var value)
        {
            messageArgs.VariableList.AddVariable(value);
            return messageArgs;
        }

        public static Args operator >(Args messageArgs, bool value)
        {
            messageArgs.VariableList.AddBool(value);
            return messageArgs;
        }

        public static Args operator >(Args messageArgs, int value)
        {
            messageArgs.VariableList.AddInt(value);
            return messageArgs;
        }

        public static Args operator >(Args messageArgs, long value)
        {
            messageArgs.VariableList.AddInt64(value);
            return messageArgs;
        }

        public static Args operator >(Args messageArgs, float value)
        {
            messageArgs.VariableList.AddFloat(value);
            return messageArgs;
        }

        public static Args operator >(Args messageArgs, double value)
        {
            messageArgs.VariableList.AddDouble(value);
            return messageArgs;
        }

        public static Args operator >(Args messageArgs, string value)
        {
            messageArgs.VariableList.AddString(value);
            return messageArgs;
        }

        public static Args operator >(Args messageArgs, EntityID value)
        {
            messageArgs.VariableList.AddEntityID(value);
            return messageArgs;
        }

        public static Args operator >(Args messageArgs, byte[] value)
        {
            messageArgs.VariableList.AddBinary(value);
            return messageArgs;
        }

        public static Args operator >(Args messageArgs, Args value)
        {
            messageArgs.VariableList.AddVariableList(value.VariableList);
            return messageArgs;
        }

        public static Args operator >(Args messageArgs, object value)
        {
            messageArgs.VariableList.AddObject(value);
            return messageArgs;
        }

        public void Clear()
        {
            VariableList.Clear();
        }

        public void Release()
        {
            //必须判空，没有注入参数的时候是没有从缓存取Variables实例的
            if (m_VariableList != null)
            {
                s_VariablePool.Release(m_VariableList);
            }
        }

        public override string ToString()
        {
            return VariableList.ToString();
        }

        static Variables Alloc()
        {
            Variables variables = s_VariablePool.Get();
            OnAllocCheck();
            return variables;
        }

        static void OnVariableRelease(Variables variables)
        {
            OnReleaseCheck();
            variables.Clear();
        }

        internal void SetContent(Variables variables)
        {
            m_VariableList = variables;
        }

        [Conditional("UNIVERSE_DEBUG_MESSAGE_ARGS")]
        static void OnAllocCheck()
        {
            Log.Info("Alloc variables, "
                   + $"cache count {s_VariablePool.CountAll.ToString()},"
                   + $" inactive {s_VariablePool.CountInactive.ToString()}"
                   + $" active {(s_VariablePool.CountAll - s_VariablePool.CountInactive).ToString()}");
        }

        [Conditional("UNIVERSE_DEBUG_MESSAGE_ARGS")]
        static void OnReleaseCheck()
        {
            Log.Info("Release variables,  "
                   + $"cache count {s_VariablePool.CountAll.ToString()},"
                   + $" inactive {s_VariablePool.CountInactive.ToString()}"
                   + $" active {(s_VariablePool.CountAll - s_VariablePool.CountInactive).ToString()}");
        }
    }
}
