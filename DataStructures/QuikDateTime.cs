// Copyright (c) 2014-2020 QuikSharp Authors ...
using System;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// Формат даты и времени, используемый в таблицах QUIK.
    /// </summary>
    public class QuikDateTime
    {
        // ReSharper disable InconsistentNaming
        public int mcs { get; set; }
        public int ms { get; set; }
        public int sec { get; set; }
        public int min { get; set; }
        public int hour { get; set; }
        public int day { get; set; }
        public int week_day { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Явное преобразование QuikDateTime → DateTime (с учётом микросекунд)
        /// </summary>
        public static explicit operator DateTime(QuikDateTime qdt)
        {
            if (qdt == null)
                throw new ArgumentNullException(nameof(qdt));

            var dt = new DateTime(qdt.year, qdt.month, qdt.day, qdt.hour, qdt.min, qdt.sec);

            // Приоритет: микросекунды (mcs), если они есть
            long ticks = qdt.mcs > 0 ? qdt.mcs * 10L : qdt.ms * 1000L;
            return dt.AddTicks(ticks);
        }

        /// <summary>
        /// Явное преобразование DateTime → QuikDateTime
        /// </summary>
        public static explicit operator QuikDateTime(DateTime dt)
        {
            long fractionalTicks = dt.Ticks % TimeSpan.TicksPerSecond;
            int totalMicroseconds = (int)(fractionalTicks / 10);

            return new QuikDateTime
            {
                year = dt.Year,
                month = dt.Month,
                day = dt.Day,
                hour = dt.Hour,
                min = dt.Minute,
                sec = dt.Second,
                ms = dt.Millisecond,
                mcs = totalMicroseconds,
                week_day = dt.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)dt.DayOfWeek
            };
        }
    }

    /// <summary>
    /// Методы расширения для QuikDateTime
    /// </summary>
    public static class QuikDateTimeExtensions
    {
        /// <summary>
        /// Безопасное преобразование QuikDateTime в DateTime
        /// </summary>
        public static DateTime ToDateTime(this QuikDateTime qdt)
        {
            if (qdt == null)
                throw new ArgumentNullException(nameof(qdt));

            // Можно вызвать explicit operator
            return (DateTime)qdt;

            // Или напрямую (если не хотите использовать operator):
            // var dt = new DateTime(qdt.year, qdt.month, qdt.day, qdt.hour, qdt.min, qdt.sec);
            // long ticks = qdt.mcs > 0 ? qdt.mcs * 10L : qdt.ms * 1000L;
            // return dt.AddTicks(ticks);
        }
    }
}