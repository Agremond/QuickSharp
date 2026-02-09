// Copyright (c) 2014-2020 QUIKSharp Authors https://github.com/finsight/QUIKSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// Представляет дату и время в формате, используемом таблицами Quik.
    /// Все параметры должны быть заданы для корректного отображения даты и времени.
    /// </summary>
    public class QuikDateTime
    {
        private int _mcs;
        private int _ms;
        private int _sec;
        private int _min;
        private int _hour;
        private int _day;
        private int _month;
        private int _year;

        /// <summary>
        /// Получает или задает микросекунды (игнорируются в текущей версии).
        /// </summary>
        public int mcs
        {
            get => _mcs;
            set => _mcs = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value), "Микросекунды не могут быть отрицательными");
        }

        /// <summary>
        /// Получает или задает миллисекунды (0–999).
        /// </summary>
        public int ms
        {
            get => _ms;
            set => _ms = value is >= 0 and <= 999 ? value : throw new ArgumentOutOfRangeException(nameof(value), "Миллисекунды должны быть в диапазоне 0–999");
        }

        /// <summary>
        /// Получает или задает секунды (0–59).
        /// </summary>
        public int sec
        {
            get => _sec;
            set => _sec = value is >= 0 and <= 59 ? value : throw new ArgumentOutOfRangeException(nameof(value), "Секунды должны быть в диапазоне 0–59");
        }

        /// <summary>
        /// Получает или задает минуты (0–59).
        /// </summary>
        public int min
        {
            get => _min;
            set => _min = value is >= 0 and <= 59 ? value : throw new ArgumentOutOfRangeException(nameof(value), "Минуты должны быть в диапазоне 0–59");
        }

        /// <summary>
        /// Получает или задает часы (0–23).
        /// </summary>
        public int hour
        {
            get => _hour;
            set => _hour = value is >= 0 and <= 23 ? value : throw new ArgumentOutOfRangeException(nameof(value), "Часы должны быть в диапазоне 0–23");
        }

        /// <summary>
        /// Получает или задает день месяца (1–31).
        /// </summary>
        public int day
        {
            get => _day;
            set => _day = value is >= 1 and <= 31 ? value : throw new ArgumentOutOfRangeException(nameof(value), "День должен быть в диапазоне 1–31");
        }

        /// <summary>
        /// Получает или задает день недели (1 – понедельник, 7 – воскресенье).
        /// </summary>
        public int week_day { get; set; }

        /// <summary>
        /// Получает или задает месяц (1–12).
        /// </summary>
        public int month
        {
            get => _month;
            set => _month = value is >= 1 and <= 12 ? value : throw new ArgumentOutOfRangeException(nameof(value), "Месяц должен03 должен быть в диапазоне 1–12");
        }

        /// <summary>
        /// Получает или задает год (например, 2025).
        /// </summary>
        public int year
        {
            get => _year;
            set => _year = value >= 1970 ? value : throw new ArgumentOutOfRangeException(nameof(value), "Год должен быть не ранее 1970");
        }

        /// <summary>
        /// Преобразует QuikDateTime в DateTime.
        /// </summary>
        public static explicit operator DateTime(QuikDateTime qdt)
        {
            return new DateTime(qdt.year, qdt.month, qdt.day, qdt.hour, qdt.min, qdt.sec, qdt.ms);
        }

        /// <summary>
        /// Преобразует DateTime в QuikDateTime.
        /// </summary>
        public static explicit operator QuikDateTime(DateTime dt)
        {
            return new QuikDateTime
            {
                year = dt.Year,
                month = dt.Month,
                day = dt.Day,
                hour = dt.Hour,
                min = dt.Minute,
                sec = dt.Second,
                ms = dt.Millisecond,
                mcs = 0,
                week_day = dt.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)dt.DayOfWeek
            };
        }
    }
}