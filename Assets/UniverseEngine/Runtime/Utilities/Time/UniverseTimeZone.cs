using System;
using System.Collections.Generic;

namespace UniverseEngine
{
    public class UniverseTimeZone
    {
        static UniverseTimeZone s_Local;
        static readonly ParamReader s_TeamParamReader = new();
        const string DEFAULT_TIME_ZONE_TEXT = "China Standard Time;480;(UTC+08:00) 北京，重庆，香港特别行政区，乌鲁木齐;中国标准时间;中国夏令时;;";
        readonly ParamReader m_ParamReader = new();

        TimeSpan m_BaseUtcOffset;

        readonly List<TimeZoneInfo.AdjustmentRule> m_Rules = new();

        public static UniverseTimeZone Local
        {
            get
            {
                if (s_Local == null)
                {
                    InitLocal(string.Empty);
                }

                return s_Local;
            }
        }

        public string ID { get; private set; } = string.Empty;
        public string DisplayName { get; private set; } = string.Empty;
        public string StandardDisplayName { get; private set; } = string.Empty;
        public string DaylightDisplayName { get; private set; } = string.Empty;

        public static void InitLocal(string serializedText)
        {
            if (string.IsNullOrEmpty(serializedText))
            {
                // 使用默认时区
                serializedText = DEFAULT_TIME_ZONE_TEXT;
            }

            s_Local = FromSerializedString(serializedText);
        }

        public static UniverseTimeZone FromSerializedString(string source)
        {
            UniverseTimeZone zone = new();
            zone.Parse(source);
            return zone;
        }

        public static DateTime ConvertToUtc(DateTime dt, UniverseTimeZone timeZone)
        {
            if (timeZone == null)
            {
                return dt;
            }

            dt = dt.Add(-timeZone.m_BaseUtcOffset);
            dt = dt.Add(-timeZone.GetRuleApplyTimeSpan(dt));

            return dt;
        }

        public static DateTime ConvertFromUtc(DateTime dt, UniverseTimeZone timeZone)
        {
            if (timeZone == null)
            {
                return dt;
            }

            dt = dt.Add(timeZone.m_BaseUtcOffset);
            dt = dt.Add(timeZone.GetRuleApplyTimeSpan(dt));

            return dt;
        }

        void Parse(string str)
        {
            string newValue = str.Replace("[", "");
            newValue = newValue.Replace("]", "");

            m_ParamReader.SetStr(newValue, ';');

            ID = m_ParamReader.ReadString();
            m_BaseUtcOffset = ReadTimeSpan();
            DisplayName = m_ParamReader.ReadString();
            StandardDisplayName = m_ParamReader.ReadString();
            DaylightDisplayName = m_ParamReader.ReadString();

            m_Rules.Clear();

            while (true)
            {
                TimeZoneInfo.AdjustmentRule rule = ReadRule();
                if (rule == null)
                {
                    break;
                }

                m_Rules.Add(rule);
            }
        }

        TimeZoneInfo.AdjustmentRule ReadRule()
        {
            if (m_ParamReader.remainCount < 16)
            {
                return null;
            }

            DateTime dateStart = ReadDateTimeOfYear();
            DateTime dateEnd = ReadDateTimeOfYear();
            TimeSpan daylightDelta = ReadTimeSpan();
            TimeZoneInfo.TransitionTime daylightTransitionStart = ReadTransitionTime();
            TimeZoneInfo.TransitionTime daylightTransitionEnd = ReadTransitionTime();

            return TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(dateStart, dateEnd,
                                                                    daylightDelta, daylightTransitionStart, daylightTransitionEnd);
        }

        DateTime ReadDateTimeOfYear()
        {
            s_TeamParamReader.SetStr(m_ParamReader.ReadString(), ':');

            int month = s_TeamParamReader.ReadInt();
            int day = s_TeamParamReader.ReadInt();
            int year = s_TeamParamReader.ReadInt();

            return new(year, month, day);
        }

        DateTime ReadDateTimeOfDay()
        {
            s_TeamParamReader.SetStr(m_ParamReader.ReadString(), ':');

            int hour = s_TeamParamReader.ReadInt();
            int min = s_TeamParamReader.ReadInt();
            int sec = s_TeamParamReader.ReadInt();

            return new(1, 1, 1, hour, min, sec);
        }

        TimeZoneInfo.TransitionTime ReadTransitionTime()
        {
            int day = m_ParamReader.ReadInt();
            DateTime timeOfDay = ReadDateTimeOfDay();
            int month = m_ParamReader.ReadInt();
            int week = m_ParamReader.ReadInt();
            DayOfWeek dayOfWeek = (DayOfWeek)m_ParamReader.ReadInt();
            m_ParamReader.ReadString();

            if (day > 0)
            {
                return TimeZoneInfo.TransitionTime.CreateFixedDateRule(timeOfDay, month, day);
            }
            else
            {
                return TimeZoneInfo.TransitionTime.CreateFloatingDateRule(timeOfDay, month, week, dayOfWeek);
            }
        }

        TimeSpan ReadTimeSpan()
        {
            return new(0, m_ParamReader.ReadInt(), 0);
        }

        TimeSpan GetRuleApplyTimeSpan(DateTime dt)
        {
            for (int i = 0; i < m_Rules.Count; ++i)
            {
                TimeZoneInfo.AdjustmentRule rule = m_Rules[i];

                if (dt < rule.DateStart || dt > rule.DateEnd)
                {
                    continue;
                }

                if (IsInDaylight(rule.DaylightTransitionStart, rule.DaylightTransitionEnd, dt))
                {
                    return rule.DaylightDelta;
                }
            }

            return TimeSpan.Zero;
        }

        bool IsInDaylight(TimeZoneInfo.TransitionTime start, TimeZoneInfo.TransitionTime end, DateTime dt)
        {
            DateTime timeOfDay = new(1, 1, 1, dt.Hour, dt.Minute, dt.Second);

            // 检测月
            if (start.Month <= end.Month)
            {
                // 不跨年
                if (dt.Month < start.Month || dt.Month > end.Month)
                {
                    return false;
                }
            }
            else
            {
                // 跨年，比如从10月到次年4月
                if (dt.Month < start.Month && dt.Month > end.Month)
                {
                    return false;
                }
            }

            // 检测起始日
            if (dt.Month == start.Month)
            {
                if (start.IsFixedDateRule)
                {
                    if (dt.Day < start.Day)
                    {
                        return false;
                    }
                    else if (dt.Day == start.Day)
                    {
                        if (timeOfDay < start.TimeOfDay)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    int weekOfMonth = GetWeekOfMonth(dt);
                    if (weekOfMonth < start.Week)
                    {
                        return false;
                    }
                    else if (weekOfMonth == start.Week)
                    {
                        if (dt.DayOfWeek < start.DayOfWeek)
                        {
                            return false;
                        }
                        else if (dt.DayOfWeek == start.DayOfWeek)
                        {
                            if (timeOfDay < start.TimeOfDay)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            // 检测结束时间
            if (dt.Month == end.Month)
            {
                if (end.IsFixedDateRule)
                {
                    if (dt.Day > end.Day)
                    {
                        return false;
                    }
                    else if (dt.Day == end.Day)
                    {
                        if (timeOfDay > end.TimeOfDay)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    int weekOfMonth = GetWeekOfMonth(dt);
                    if (weekOfMonth > end.Week)
                    {
                        return false;
                    }
                    else if (weekOfMonth == end.Week)
                    {
                        if (dt.DayOfWeek > end.DayOfWeek)
                        {
                            return false;
                        }
                        else if (dt.DayOfWeek == end.DayOfWeek)
                        {
                            if (timeOfDay > end.TimeOfDay)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        int GetWeekOfMonth(DateTime dt)
        {
            int dayOfWeek = ((dt.Day - 1) % 7);

            int diff = (int)dt.DayOfWeek - dayOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return (dt.Day - 1 + diff) / 7 + 1;
        }
    }
}
