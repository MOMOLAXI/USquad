using System;
using System.Diagnostics;
using System.Globalization;

namespace UniverseEngine
{
    public static class TimeUtilities
    {
        public const int TIME_TYPE_DAY = 1;
        public const int TIME_TYPE_WEEK = 2;
        public const string TEXT_YEAR = "年";
        public const string TEXT_MONTH = "月";
        public const string TEXT_DAY = "日";
        public const string TEXT_ONE = "一";
        public const string TEXT_TWO = "二";
        public const string TEXT_THREE = "三";
        public const string TEXT_FOUR = "四";
        public const string TEXT_FIVE = "五";
        public const string TEXT_SIX = "六";
        public const string TEXT_SEVEN = "七";
        public const string TEXT_EIGHT = "八";
        public const string TEXT_NINE = "九";
        public const string TEXT_TEN = "十";
        public const string YEAR_FORMAT = "{0}年";
        public const string MONTH_FORMAT = "{0}月";
        public const string DAY_FORMAT = "{0}日";
        public const string HOUR_FORMAT = "{0}时";
        public const string MINUTE_FORMAT = "{0}分";
        public const string SECOND_FORMAT = "{0}秒";
        public const string YEAR_BEFORE_FORMAT = "{0}年前";
        public const string MONTH_BEFORE_FORMAT = "{0}月前";
        public const string DAY_BEFORE_FORMAT = "{0}日前";
        public const string HOUR_BEFORE_FORMAT = "{0}小时前";
        public const string MINUTE_BEFORE_FORMAT = "{0}分前";
        public const string SECOND_BEFORE_FORMAT = "{0}秒前";

        static readonly ParamReader s_ParamReader = new();

        public static string Now()
        {
            return $"{DateTime.Now:yyyy-MM-dd hh:mm:ss}";
        }

        public static double GetSeconds(long ticks)
        {
            double seconds = TimeSpan.FromTicks(ticks).TotalSeconds;
            return seconds;
        }

        public static int GetSeconds(Stopwatch stopwatch)
        {
            float seconds = stopwatch.ElapsedMilliseconds / 1000f;
            return (int)seconds;
        }

        /// <summary>
        /// 大写一到十
        /// </summary>
        /// <param name="dayNum"></param>
        /// <returns></returns>
        public static string FormatCaseOneToTen(int dayNum)
        {
            return dayNum switch
            {
                1 => TEXT_ONE,
                2 => TEXT_TWO,
                3 => TEXT_THREE,
                4 => TEXT_FOUR,
                5 => TEXT_FIVE,
                6 => TEXT_SIX,
                7 => TEXT_SEVEN,
                8 => TEXT_EIGHT,
                9 => TEXT_NINE,
                10 => TEXT_TEN,
                _ => string.Empty
            };
        }

        /// <summary>
        /// 格式化天，大写 一，二，三，四，十一， 二十, 最大支持99, 从一开始
        /// </summary>
        /// <param name="dayNum"></param>
        /// <returns></returns>
        public static string FormatCaseDayNum(int dayNum)
        {
            int oneDigit = dayNum / 10;
            int twoDigit = dayNum % 10;
            if (dayNum > 10)
            {
                if (oneDigit == 1)
                {
                    return string.Format("{0}{1}", FormatCaseOneToTen(10), FormatCaseOneToTen(twoDigit));
                }
                if (twoDigit == 0)
                {
                    return string.Format("{0}{1}", FormatCaseOneToTen(oneDigit), FormatCaseOneToTen(10));
                }
                return string.Format("{0}{1}", FormatCaseOneToTen(oneDigit), FormatCaseOneToTen(twoDigit));
            }
            return string.Format("{0}", FormatCaseOneToTen(dayNum));
        }

        /// <summary>
        /// 最大单位是天，输出1位(x天、x小时, x分, x秒）
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string FormatMaxUnitDayOutOne(long timestamp)
        {
            timestamp /= 1000;
            long day = timestamp / 86400;
            long hours = timestamp % 86400 / 3600;
            long minute = timestamp % 3600 / 60;
            long second = timestamp % 60;
            if (day >= 1)
            {
                // X天
                return string.Format("{0}{1}", day, DAY_FORMAT);
            }
            if (hours >= 1)
            {
                // X小时
                return string.Format("{0}{1}", hours, HOUR_FORMAT);
            }
            if (minute >= 1)
            {
                // X分
                return string.Format("{0}{1}", minute, MINUTE_FORMAT);
            }
            // X秒
            return string.Format("{0}{1}", second, SECOND_FORMAT);
        }

        /// <summary>
        /// 最大单位是天，输出2位(x天y小时、x小时y分、x分y秒）
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string FormatMaxUnitDayOutTwo(long timestamp)
        {
            timestamp /= 1000;
            long day = timestamp / 86400;
            long hours = timestamp % 86400 / 3600;
            long minute = timestamp % 3600 / 60;
            long second = timestamp % 60;
            if (day >= 1)
            {
                // X天Y小时
                return string.Format("{0}{1}{2}{3}", day, DAY_FORMAT, hours, HOUR_FORMAT);
            }
            if (hours >= 1)
            {
                // X小时Y分
                return string.Format("{0}{1}{2}{3}", hours, HOUR_FORMAT, minute, MINUTE_FORMAT);
            }
            // X分Y秒
            return string.Format("{0}{1}{2}{3}", minute, MINUTE_FORMAT, second, SECOND_FORMAT);
        }

        /// <summary>
        /// 最大单位是天，
        ///     时间格式(大于1天) ：x天y小时z分
        ///     时间格式(小于1天) ：x小时y分z秒
        ///     时间格式（小于1小时）：x分y秒
        ///     时间格式（小于1小时）：x秒
        /// </summary>
        public static string FormatRemainTimeDHM(long timestamp)
        {
            timestamp /= 1000;
            long day = timestamp / 86400;
            long hours = timestamp % 86400 / 3600;
            long minute = timestamp % 3600 / 60;
            long second = timestamp % 60;
            if (day >= 1)
            {
                // a天b时c分d秒
                return string.Format("{0}{1}{2}{3}{4}{5}",
                                     day, DAY_FORMAT,
                                     hours, HOUR_FORMAT
                                   , minute, MINUTE_FORMAT);
            }
            if (hours >= 1)
            {
                // b时c分d秒
                return string.Format("{0}{1}{2}{3}{4}{5}",
                                     hours, HOUR_FORMAT,
                                     minute, MINUTE_FORMAT,
                                     second, SECOND_FORMAT);
            }
            if (minute >= 1)
            {
                // c分d秒
                return string.Format("{0}{1}{2}{3}", minute, MINUTE_FORMAT, second, SECOND_FORMAT);
            }
            // d秒
            return string.Format("{0}{1}", second, SECOND_FORMAT);
        }

        /// <summary>
        /// 最大单位是天，
        ///     时间格式(大于1天) ：7天3时6分10秒
        ///     时间格式(小于1天) ：3时6分10秒
        ///     时间格式（小于1小时）：6分10秒
        ///     时间格式（小于1小时）：10秒
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string FormatRemainTime(long timestamp)
        {
            timestamp /= 1000;
            long day = timestamp / 86400;
            long hours = timestamp % 86400 / 3600;
            long minute = timestamp % 3600 / 60;
            long second = timestamp % 60;
            if (day >= 1)
            {
                // a天b时c分d秒
                return string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", day, DAY_FORMAT, hours, HOUR_FORMAT
                                   , minute, MINUTE_FORMAT, second, SECOND_FORMAT);
            }
            if (hours >= 1)
            {
                // b时c分d秒
                return string.Format("{0}{1}{2}{3}{4}{5}", hours, HOUR_FORMAT
                                   , minute, MINUTE_FORMAT, second, SECOND_FORMAT);
            }
            if (minute >= 1)
            {
                // c分d秒
                return string.Format("{0}{1}{2}{3}", minute, MINUTE_FORMAT, second, SECOND_FORMAT);
            }
            // d秒
            return string.Format("{0}{1}", second, SECOND_FORMAT);
        }

        /// <summary>
        /// 聊天时间显示
        /// </summary>
        /// <param name="serverTimeStamp"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string GetFormatDateTimeChat(long serverTimeStamp, long timestamp)
        {
            DateTime nowDt = TimestampToDateTime(serverTimeStamp);
            DateTime endDt = TimestampToDateTime(timestamp);

            TimeSpan span = nowDt - endDt;
            int addDay = span.TotalHours % 24 + endDt.Hour >= 24 ? 1 : 0;
            double totalDays = span.TotalDays + addDay;

            if (totalDays > 3.0f)
            {
                //一年前
                if (PrimitiveUtilities.FloatEquals(nowDt.Year, nowDt.Year))
                {
                    return string.Format("{0}{1}{2}{3}{4}{5}{6:D2}:{7:D2}",
                                         endDt.Year.ToString(), YEAR_FORMAT,
                                         endDt.Month.ToString(), MONTH_FORMAT,
                                         endDt.Day, DAY_FORMAT,
                                         endDt.Hour,
                                         endDt.Minute);
                }
                //同一年
                return string.Format("{0}{1}{2}{3}{4:D2}:{5:D2}",
                                     endDt.Month.ToString(), MONTH_FORMAT,
                                     endDt.Day, DAY_FORMAT,
                                     endDt.Hour,
                                     endDt.Minute);
            }
            //前天
            if (totalDays > 2.0f)
            {
                return string.Format("{0}{1:D2}:{2:D2}", "前天", endDt.Hour, endDt.Minute);
            }
            //昨天
            if (totalDays > 1.0f)
            {
                return string.Format("{0}{1:D2}:{2:D2}", "昨天", endDt.Hour, endDt.Minute);
            }
            //今天
            return string.Format("{0:D2}:{1:D2}", endDt.Hour, endDt.Minute);
        }

        /// <summary>
        /// 最大单位是天，输出2位(x天y小时、x小时y分、x分y秒）
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string FormatColorMaxUnitDayOutTwo(long timestamp)
        {
            timestamp /= 1000;
            long day = timestamp / 86400;
            long hours = timestamp % 86400 / 3600;
            long minute = timestamp % 3600 / 60;
            long second = timestamp % 60;
            string dayFormat = DAY_FORMAT;
            string hoursFormat = HOUR_FORMAT;
            string minuteFormat = MINUTE_FORMAT;
            string secondFormat = SECOND_FORMAT;
            if (day >= 1)
            {
                // X天Y小时
                return string.Format("<color=#a1ee3e>{0}</color>{1}<color=#a1ee3e>{2}</color>{3}", day, dayFormat, hours, hoursFormat);
            }
            if (hours >= 1)
            {
                // X小时Y分
                return string.Format("<color=#a1ee3e>{0}</color>{1}<color=#a1ee3e>{2}</color>{3}", hours, hoursFormat, minute, minuteFormat);
            }
            // X分Y秒
            return string.Format("<color=#a1ee3e>{0}</color>{1}<color=#a1ee3e>{2}</color>{3}", minute, minuteFormat, second, secondFormat);
        }

        /// <summary>
        /// 最大单位是天，输出2位(x天y时、x时y分、x分y秒）
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string FormatColorMaxUnitDayOutTwoEx(long timestamp)
        {
            timestamp /= 1000;
            long day = timestamp / 86400;
            long hours = timestamp % 86400 / 3600;
            long minute = timestamp % 3600 / 60;
            long second = timestamp % 60;
            string dayFormat = DAY_FORMAT;
            string hoursFormat = HOUR_FORMAT;
            string minuteFormat = MINUTE_FORMAT;
            string secondFormat = SECOND_FORMAT;
            if (day >= 1)
            {
                // X天Y小时
                return string.Format("<color=#a1ee3e>{0}{1}{2}{3}</color>", day, dayFormat, hours, hoursFormat);
            }
            if (hours >= 1)
            {
                // X小时Y分
                return string.Format("<color=#a1ee3e>{0}{1}{2}{3}</color>", hours, hoursFormat, minute, minuteFormat);
            }
            // X分Y秒
            return string.Format("<color=#a1ee3e>{0}{1}{2}{3}</color>", minute, minuteFormat, second, secondFormat);
        }

        /// <summary>
        /// 最大单位是天，输出2位(x天y时、x时y分、x分y秒）
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string FormatUnitDayOutTwo(long timestamp)
        {
            timestamp /= 1000;
            long day = timestamp / 86400;
            long hours = timestamp % 86400 / 3600;
            long minute = timestamp % 3600 / 60;
            long second = timestamp % 60;
            string dayFormat = DAY_FORMAT;
            string hoursFormat = HOUR_FORMAT;
            string minuteFormat = MINUTE_FORMAT;
            string secondFormat = SECOND_FORMAT;
            if (day >= 1)
            {
                // X天Y小时
                return "{0}{1}{2}{3}".SafeFormat(day, dayFormat, hours, hoursFormat);
            }
            if (hours >= 1)
            {
                // X小时Y分
                return "{0}{1}{2}{3}".SafeFormat(hours, hoursFormat, minute, minuteFormat);
            }
            // X分Y秒
            return "{0}{1}{2}{3}".SafeFormat(minute, minuteFormat, second, secondFormat);
        }

        /// <summary>
        /// 最大单位是天，输出1位(x天、x时）,
        /// 天数，小时数向上取整（0-1小时，显示1小时，1-2小时，显示2小时; 1天10小时，显示2天）
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string FormatUnitDayOutOne(long timestamp)
        {
            timestamp /= 1000;
            long day = timestamp / 86400;
            long hours = timestamp % 86400 / 3600;

            string dayFormat = DAY_FORMAT;
            string hoursFormat = HOUR_FORMAT;
            if (day >= 1)
            {
                if (hours > 0)
                {
                    day++;
                }

                // X天
                return "{0}{1}".SafeFormat(day, dayFormat);
            }
            if (timestamp % 3600 > 0)
            {
                hours += 1;
            }

            if (hours >= 1)
            {
                // X小时
                return "{0}{1}".SafeFormat(hours, hoursFormat);
            }

            return string.Empty;
        }

        /// <summary>
        /// 输出已经过了多久了  （几天前， 几小时前，几分钟前，几秒前）
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="colorStr"></param>
        /// <returns></returns>
        public static string FormatDiffAgoOutOne(long timestamp, string colorStr = "#52505E")
        {
            timestamp /= 1000;

            long year = timestamp / 31104000;
            long month = timestamp / 2592000;
            long day = timestamp / 86400;
            long hours = timestamp % 86400 / 3600;
            long minute = timestamp % 3600 / 60;

            if (year >= 1)
            {
                return $"<color={colorStr}>{YEAR_BEFORE_FORMAT}</color>"; // <color={1}>{0}年前</color>
            }

            if (month >= 1)
            {
                return $"<color={colorStr}>{MONTH_BEFORE_FORMAT}</color>"; // <color={1}>{0}个月前</color>
            }

            if (day >= 1)
            {
                return $"<color={colorStr}>{DAY_BEFORE_FORMAT}</color>"; // <color={1}>{0}天前</color>
            }
            if (hours >= 1)
            {
                return $"<color={colorStr}>{HOUR_BEFORE_FORMAT}</color>"; // <color={1}>{0}小时前</color>
            }
            if (minute >= 1)
            {
                return $"<color={colorStr}>{MINUTE_BEFORE_FORMAT}</color>"; // <color={1}>{0}分钟前</color>
            }
            return $"<color={colorStr}>{SECOND_BEFORE_FORMAT}</color>"; // <color={1}>{0}秒前</color>
        }

        /// <summary>
        /// 输出自动长度的时间显示
        /// </summary>
        public static string FormatTimeAuto(long ms)
        {
            ms /= 1000;
            long hour = ms / 3600;
            long min = ms % 3600 / 60;
            long sec = ms % 60;

            if (hour > 0)
            {
                return string.Format("{0:D2}:{1:D2}:{2:D2}", hour, min, sec);
            }
            if (min > 0)
            {
                return string.Format("{0:D2}:{1:D2}", min, sec);
            }
            return sec.ToString(); // 秒这里不要加D2，是多少就显示多少
        }

        // 输出格式化时间 00:00:00
        public static string FormatTimeHms(long ms)
        {
            ms /= 1000;
            long hour = ms / 3600;
            long min = ms % 3600 / 60;
            long sec = ms % 60;
            return string.Format("{0:D2}:{1:D2}:{2:D2}", hour, min, sec);
        }

        // 输出格式化时间 00:00:00 or 00:00 (小时:分钟  分钟:秒)
        public static string FormatTimeHmsEx(long ms)
        {
            ms /= 1000;
            long hour = ms / 3600;
            long min = ms % 3600 / 60;
            long sec = ms % 60;
            if (hour <= 0)
            {
                return string.Format("{0:D2}:{1:D2}", min, sec);
            }
            return string.Format("{0:D2}:{1:D2}:{2:D2}", hour, min, sec);
        }

        // 输出格式化时间 00:00 (小时:分钟)
        public static string FormatTimeHm(long ms)
        {
            ms /= 1000;
            long hour = ms / 3600;
            long min = ms % 3600 / 60;
            return string.Format("{0:D2}:{1:D2}", hour, min);
        }

        // 输出格式化时间 00:00 (小时:分钟)
        public static string FormatTimeHmWithSec(long s)
        {
            long hour = s / 3600;
            long min = s % 3600 / 60;
            return string.Format("{0:D2}:{1:D2}", hour, min);
        }

        // 输出格式化时间 00:00
        public static string FormatTimeMS(long ms)
        {
            ms /= 1000;
            long min = ms / 60;
            long sec = ms % 60;
            return string.Format("{0:D2}:{1:D2}", min, sec);
        }

        // 输出格式化时间
        // 超过1天输出 xx天
        // 不足1天输出 xx小时
        // 不足1小时输出 不足1小时
        public static string FormatTimeD(long ms)
        {
            ms /= 1000;
            long hour = (int)ms / 3600;
            long day = 0;
            long min = ms % 3600 / 60;
            day = hour / 24;
            if (day >= 1)
            {
                return "{0}{1}".SafeFormat(day, DAY_FORMAT);
            }
            if (hour >= 1)
            {
                return "{0}{1}".SafeFormat(hour, HOUR_FORMAT);
            }
            return "{0}{1}".SafeFormat(min, MINUTE_FORMAT);
        }

        /// <summary>
        ///  时间戳转为2001-1-2
        /// </summary>
        /// <param name="serverTimeStamp"></param>
        /// <returns></returns>
        public static string GetCurrentFormatDateTime(long serverTimeStamp)
        {
            DateTime dateTime = TimestampToDateTime(serverTimeStamp);
            int year = dateTime.Year;
            int month = dateTime.Month;
            int day = dateTime.Day;
            return string.Format("{0}-{1}-{2}", year, month, day);
        }

        /// <summary>
        ///  时间戳转为 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string GetFormatDateTime(string format, long timestamp)
        {
            DateTime dt = TimestampToDateTime(timestamp);
            return dt.ToString(format);
        }

        /// <summary>
        /// 显示格式 2017-12-10 12:30
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="isTime"></param>
        /// <returns></returns>
        public static string GetDateTimeFormatG(long timestamp, bool isTime = true)
        {
            DateTime dt = TimestampToDateTime(timestamp);

            string dateStr = string.Empty;
            dateStr = isTime
                          ? string.Format("{0}-{1:D2}-{2:D2} {3:D2}:{4:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute)
                          : string.Format("{0}-{1:D2}-{2:D2}", dt.Year, dt.Month, dt.Day);
            return dateStr;
        }

        /// <summary>
        /// 显示格式 2017.12.10 12:30:59
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="isTime"></param>
        /// <returns></returns>
        public static string GetDateTimeFormat(DateTime dt, bool isTime = true)
        {
            string dateStr = string.Empty;

            if (isTime)
            {
                dateStr = string.Format("{0}.{1:D2}.{2:D2} {3:D2}:{4:D2}:{5:D2}",
                                        dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            }
            else
            {
                dateStr = string.Format("{0}.{1:D2}.{2:D2}", dt.Year, dt.Month, dt.Day);
            }

            return dateStr;
        }

        /// <summary>
        /// 显示格式 2017年12月10日 12:30:59
        /// </summary>
        public static string GetDateTimeFormat(long timestamp, bool isTime = true)
        {
            DateTime dt = TimestampToDateTime(timestamp);
            string dateStr;
            if (isTime)
            {
                dateStr = string.Format("{0}{1}{2:D2}{3}{4:D2}{5}\u3000{6:D2}:{7:D2}:{8:D2}",
                                        dt.Year, YEAR_FORMAT,
                                        dt.Month, MONTH_FORMAT,
                                        dt.Day, DAY_FORMAT,
                                        dt.Hour,
                                        dt.Minute,
                                        dt.Second);
            }
            else
            {
                dateStr = string.Format("{0}{1}{2:D2}{3}{4:D2}{5}",
                                        dt.Year, YEAR_FORMAT,
                                        dt.Month, MONTH_FORMAT,
                                        dt.Day, DAY_FORMAT);
            }

            return dateStr;
        }

        /// <summary>
        ///  时间戳转为2001-1-2
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string GetFormatDateTime(long timestamp)
        {
            DateTime dt = TimestampToDateTime(timestamp);
            return string.Format("{0:d}", dt);
        }

        /// <summary>
        /// 转换成服务器时间戳
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static long LocalDateTimeToTimestamp(int year,
                                                    int month,
                                                    int day,
                                                    int hour = 0,
                                                    int minute = 0,
                                                    int second = 0)
        {
            DateTime date = new(year, month, day, hour, minute, second);
            DateTime dateTime = UniverseTimeZone.ConvertToUtc(date, UniverseTimeZone.Local);
            DateTime startTime = new(1970, 1, 1);
            return (long)(dateTime - startTime).TotalMilliseconds;
        }

        /// <summary>
        /// 转换成服务器时间戳
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long LocalDateTimeToTimestamp(DateTime date)
        {
            DateTime dateTime = UniverseTimeZone.ConvertToUtc(date, UniverseTimeZone.Local);
            DateTime startTime = new(1970, 1, 1);
            return (long)(dateTime - startTime).TotalMilliseconds;
        }

        /// <summary>
        /// 获取当天凌晨时间
        /// </summary>
        public static DateTime GetBeginningOfDayDate(long timestamp)
        {
            DateTime date = TimestampToDateTime(timestamp);
            DateTime beginningOfDay = date.Subtract(date.TimeOfDay);
            return beginningOfDay;
        }

        /// <summary>
        /// 转换成服务器时间
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime TimestampToDateTime(long timestamp)
        {
            DateTime dt = new(1970, 1, 1, 0, 0, 0);
            dt = dt.AddMilliseconds(timestamp);
            dt = UniverseTimeZone.ConvertFromUtc(dt, UniverseTimeZone.Local);
            return dt;
        }

        /// <summary>
        /// 使用指定格式字符串转换成服务器时间
        /// </summary>
        public static DateTime ConvertServerDateTime(string timeStr, string format = "yyyy-MM-dd-HH:mm:ss")
        {
            DateTime.TryParseExact(timeStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime);
            return dateTime;
        }

        /// <summary>
        /// 字符串转时间戳
        /// </summary>
        /// <param name="timeStr">DateTime字符串</param>
        /// <param name="format">DateTime字符串的时间格式</param>
        /// <returns>时间戳</returns>
        public static long StrTimeToServerTimestamp(string timeStr, string format = "yyyy-MM-dd-HH:mm:ss")
        {
            DateTime dateTime = ConvertServerDateTime(timeStr, format);
            if (dateTime == default(DateTime))
            {
                return 0;
            }

            return LocalDateTimeToTimestamp(dateTime);
        }

        /// <summary>
        /// 字符串转时间戳
        /// </summary>
        /// <param name="strTime"> yyyy-MM-dd-HH:mm:ss </param>
        /// <returns> 转换后的时间戳 </returns>
        public static long StrTimeToTimestamp(string strTime)
        {
            if (string.IsNullOrEmpty(strTime))
            {
                return 0;
            }

            string str = strTime.Replace("-", "|");
            s_ParamReader.SetStr(str);

            if (s_ParamReader.Count() < 6)
            {
                return 0;
            }

            int year = s_ParamReader.ReadInt();
            int month = s_ParamReader.ReadInt();
            int day = s_ParamReader.ReadInt();
            int hour = s_ParamReader.ReadInt();
            int minute = s_ParamReader.ReadInt();
            int second = s_ParamReader.ReadInt();
            return LocalDateTimeToTimestamp(year, month, day, hour, minute, second);
        }

        /// <summary>
        /// 获取周几字符串
        /// </summary>
        /// <param name="weekDay"> (0-6) 0表示周日 </param>
        /// <returns> 周一/周二/..../周日  </returns>
        public static string GetWeekDayInfo(int weekDay)
        {
            return $"周{GetWeekDayStr(weekDay)}";
        }


        /// <summary>
        /// 获取周几字符串 (0-6) 0表示周日
        /// </summary>
        /// <param name="weekDay"></param>
        /// <returns></returns>
        public static string GetWeekDayStr(int weekDay)
        {
            return weekDay switch
            {
                >= 1 and <= 6 => FormatCaseOneToTen(weekDay),
                0 => "周x",
                _ => string.Empty
            };

        }

        /// <summary>
        /// 时间戳返回天数 最小为1天
        /// </summary>
        /// <param name="timeLength"></param>
        /// <returns></returns>
        public static long GetDayCount(long timeLength)
        {
            timeLength /= 1000;
            long day = timeLength / 86400;
            long mod = timeLength % 86400;

            if (mod > 0)
            {
                day++;
            }

            if (day < 1)
            {
                day = 1;
            }

            return day;
        }

        /// <summary>
        ///  时间戳转为 13:30 (只保留时分)
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string GetFormatDateTimeHourMinute(long timestamp)
        {
            DateTime dt = TimestampToDateTime(timestamp);
            return string.Format("{0:D2}:{1:D2}", dt.Hour, dt.Minute);
        }

        /// <summary>
        /// 时间戳转为yyyy年x月x日
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string GetLanguageFormatDateYearMonthDay(long timestamp)
        {
            DateTime dt = TimestampToDateTime(timestamp);
            return string.Format("{0:D4}{1}{2:D2}{3}{4:D2}{5}", dt.Year, "年", dt.Month, "月", dt.Day, "日");
        }

        /// <summary>
        /// 制定时间转为时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <param name="weekDayIndex"> 1 2 3 4 5 6 7</param>
        /// <param name="dayTime">11:00</param>
        public static long ConvertServerTime(long time, int weekDayIndex, string dayTime)
        {
            DateTime dt = TimestampToDateTime(time);
            int nDayOfWeek = (int)dt.DayOfWeek;
            if (nDayOfWeek < 1)
            {
                nDayOfWeek = 7;
            }
            time -= (dt.Hour * 3600 + dt.Minute * 60 + dt.Second) * 1000;
            int diff = weekDayIndex - nDayOfWeek;
            time += diff * 24 * 3600 * 1000;
            s_ParamReader.SetStr(dayTime, ':');
            if (s_ParamReader.Count() > 1)
            {
                time += (s_ParamReader.ReadInt() * 3600 + s_ParamReader.ReadInt() * 60) * 1000;
            }
            return time;
        }

        /// <summary>
        /// 时间戳转周X
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string TimestampToWeekDay(long timestamp)
        {
            DateTime dt = TimestampToDateTime(timestamp);

            return GetWeekDayInfo((int)dt.DayOfWeek);
        }


        /// <summary>
        /// 时间戳转周X XX:XX
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns> 周X XX:XX </returns>
        public static string TimestampToWeekDayAndTime(long timestamp)
        {
            if (timestamp <= 0)
            {
                return string.Empty;
            }

            DateTime dt = TimestampToDateTime(timestamp);
            return "{0} {1}".SafeFormat(GetWeekDayInfo((int)dt.DayOfWeek), GetFormatDateTimeHourMinute(timestamp));
        }

        /// <summary>
        /// 获取小时和分钟时间 00:00
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static string GetDateTimeHm(long timeStamp)
        {
            DateTime dt = TimestampToDateTime(timeStamp);
            return string.Format("{0:D2}:{1:D2}", dt.Hour, dt.Minute);
        }
    }
}
