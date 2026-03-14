// Copyright (c) 2014-2020 QUIKSharp Authors https://github.com/finsight/QUIKSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

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
        /// Unique correlation id to match requests and responses
        /// </summary>
        long? Id { get; set; }

        /// <summary>
        /// A name of a function to call for requests
        /// </summary>
        string cmd { get; set; }

        /// <summary>
        /// Timestamp in milliseconds, same as in Lua `socket.gettime() * 1000`
        /// </summary>
        long CreatedTime { get; set; }

        /// <summary>
        /// Some messages are valid only for a short time, e.g. buy/sell orders
        /// </summary>
        DateTime? ValidUntil { get; set; }
    }

    /// <summary>
    /// Базовый класс сообщения
    /// </summary>
    internal abstract class BaseMessage : IMessage
    {
        protected static readonly long Epoch = (new DateTime(1970, 1, 1, 3, 0, 0, 0)).Ticks / 10000L;

        protected BaseMessage(string cmd = "", DateTime? validUntil = null)
        {
            cmd = cmd;
            CreatedTime = DateTime.Now.Ticks / 10000L - Epoch;
            ValidUntil = validUntil;
        }

        /// <summary>
        /// Unique correlation id to match requests and responses
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public long? Id { get; set; }

        /// <summary>
        /// A name of a function to call for requests
        /// </summary>
        [JsonProperty(PropertyName = "cmd")]
        public string cmd { get; set; }

        /// <summary>
        /// Timestamp in milliseconds, same as in Lua `socket.gettime() * 1000`
        /// </summary>
        [JsonProperty(PropertyName = "t")]
        public long CreatedTime { get; set; }

        /// <summary>
        /// Some messages are valid only for a short time, e.g. buy/sell orders
        /// </summary>
        [JsonProperty(PropertyName = "v")]
        public DateTime? ValidUntil { get; set; }
    }

    /// <summary>
    /// Default generic implementation
    /// </summary>
    internal class Message : BaseMessage
    {
        public Message()
        {
        }

        public Message(object message, string cmd, DateTime? validUntil = null)
        {
            cmd = cmd;
            CreatedTime = DateTime.Now.Ticks / 10000L - Epoch;
            ValidUntil = validUntil;
            Data = message;
        }

        /// <summary>
        /// String message
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