// Copyright (c) 2014-2026 QUIKSharp Community
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;
using System;

namespace QuikSharp
{
    /// <summary>
    /// Интерфейс сообщения для транспорта
    /// </summary>
    internal interface IMessage
    {
        /// <summary>
        /// Уникальный идентификатор сообщения для сопоставления запрос/ответ
        /// </summary>
        long? Id { get; set; }

        /// <summary>
        /// Команда или функция, к которой относится сообщение
        /// </summary>
        string Command { get; set; }

        /// <summary>
        /// Время создания в миллисекундах (как в Lua socket.gettime()*1000)
        /// </summary>
        long CreatedTime { get; set; }

        /// <summary>
        /// Сообщение действительно до указанного времени (необязательно)
        /// </summary>
        DateTime? ValidUntil { get; set; }
    }

    /// <summary>
    /// Базовый класс сообщения
    /// </summary>
    internal abstract class BaseMessage : IMessage
    {
        protected static readonly long Epoch = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks / 10000L;

        protected BaseMessage(string command = "", DateTime? validUntil = null)
        {
            Command = command;
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ValidUntil = validUntil;
        }

        [JsonProperty(PropertyName = "id")]
        public long? Id { get; set; }

        [JsonProperty(PropertyName = "cmd")]
        public string Command { get; set; }

        [JsonProperty(PropertyName = "t")]
        public long CreatedTime { get; set; }

        [JsonProperty(PropertyName = "v")]
        public DateTime? ValidUntil { get; set; }
    }

    /// <summary>
    /// Универсальное сообщение с произвольными данными
    /// </summary>
    internal class Message : BaseMessage
    {
        public Message()
        {
        }

        public Message(object data, string command, DateTime? validUntil = null)
        {
            Command = command;
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ValidUntil = validUntil;
            Data = data;
        }

        /// <summary>
        /// Данные сообщения (любой тип)
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }

        /// <summary>
        /// Ошибка Lua (если есть)
        /// </summary>
        [JsonProperty(PropertyName = "luaError")]
        public string? LuaError { get; set; }

        /// <summary>
        /// Удобный метод для десериализации Data в нужный тип
        /// </summary>
        public T GetData<T>()
        {
            if (Data is T t) return t;

            // Сериализация через JSON и десериализация в нужный тип
            string json = JsonConvert.SerializeObject(Data);
            return JsonConvert.DeserializeObject<T>(json)!;
        }
    }
}