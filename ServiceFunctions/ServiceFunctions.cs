// Copyright (c) 2026 Your Name / QUIKSharp Community
// Licensed under the Apache License, Version 2.0

using QuikSharp.DataStructures;
using QuikSharp.Transports;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuikSharp
{
    /// <summary>
    /// Реализация сервисных функций QUIK через любой транспорт (TCP / SHM)
    /// </summary>
    public class ServiceFunctions : IServiceFunctions
    {
        private readonly IQuikTransport _transport;

        public ServiceFunctions(IQuikTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public async Task<string> GetWorkingFolder()
        {
            var response = await _transport.SendAsync<Message<string>, Message<string>>(
                new Message<string>("", "getWorkingFolder"),
                "getWorkingFolder"
            ).ConfigureAwait(false);

            return response.Data;
        }

        public async Task<bool> IsConnected(int timeout = Timeout.Infinite)
        {
            var response = await _transport.SendAsync<Message<string>, Message<string>>(
                new Message<string>("", "isConnected"),
                "isConnected",
                CancellationToken.None // можно прокинуть таймаут через токен
            ).ConfigureAwait(false);

            return response.Data == "1";
        }

        public async Task<string> GetScriptPath()
        {
            var response = await _transport.SendAsync<Message<string>, Message<string>>(
                new Message<string>("", "getScriptPath"),
                "getScriptPath"
            ).ConfigureAwait(false);

            return response.Data;
        }

        public async Task<string> GetInfoParam(InfoParams param)
        {
            var response = await _transport.SendAsync<Message<string>, Message<string>>(
                new Message<string>(param.ToString(), "getInfoParam"),
                "getInfoParam"
            ).ConfigureAwait(false);

            return response.Data;
        }

        public async Task<bool> Message(string message, NotificationType iconType = NotificationType.Info)
        {
            string command = iconType switch
            {
                NotificationType.Info => "message",
                NotificationType.Warning => "warning_message",
                NotificationType.Error => "error_message",
                _ => throw new ArgumentOutOfRangeException(nameof(iconType))
            };

            await _transport.SendAsync<Message<string>, Message<string>>(
                new Message<string>(message, command),
                command
            ).ConfigureAwait(false);

            return true;
        }

        public async Task<bool> PrintDbgStr(string message)
        {
            await _transport.SendAsync<Message<string>, Message<string>>(
                new Message<string>(message, "PrintDbgStr"),
                "PrintDbgStr"
            ).ConfigureAwait(false);

            return true;
        }

        public async Task<double> AddLabel(double price, string curDate, string curTime, string hint, string path, string tag, string alignment, double backgnd)
        {
            string payload = $"{price}|{curDate}|{curTime}|{hint}|{path}|{tag}|{alignment}|{backgnd}";

            var response = await _transport.SendAsync<Message<string>, Message<double>>(
                new Message<string>(payload, "addLabel"),
                "addLabel"
            ).ConfigureAwait(false);

            return response.Data;
        }

        public async Task<double> AddLabel(string chartTag, decimal yValue, string strDate, string strTime, string text = "", string imagePath = "",
            string alignment = "", string hint = "", int r = -1, int g = -1, int b = -1, int transparency = -1,
            int tranBackgrnd = -1, string fontName = "", int fontHeight = -1)
        {
            string payload = $"{chartTag}|{yValue}|{strDate}|{strTime}|{text}|{imagePath}|{alignment}|{hint}|{r}|{g}|{b}|{transparency}|{tranBackgrnd}|{fontName}|{fontHeight}";

            var response = await _transport.SendAsync<Message<string>, Message<double>>(
                new Message<string>(payload, "addLabel2"),
                "addLabel2"
            ).ConfigureAwait(false);

            return response.Data;
        }

        public async Task<bool> SetLabelParams(string chartTag, int labelId, decimal yValue, string strDate, string strTime, string text = "", string imagePath = "",
            string alignment = "", string hint = "", int r = -1, int g = -1, int b = -1, int transparency = -1,
            int tranBackgrnd = -1, string fontName = "", int fontHeight = -1)
        {
            string payload = $"{chartTag}|{labelId}|{yValue}|{strDate}|{strTime}|{text}|{imagePath}|{alignment}|{hint}|{r}|{g}|{b}|{transparency}|{tranBackgrnd}|{fontName}|{fontHeight}";

            var response = await _transport.SendAsync<Message<string>, Message<bool>>(
                new Message<string>(payload, "setLabelParams"),
                "setLabelParams"
            ).ConfigureAwait(false);

            return response.Data;
        }

        public async Task<Label> GetLabelParams(string chartTag, int labelId)
        {
            string payload = $"{chartTag}|{labelId}";

            var response = await _transport.SendAsync<Message<string>, Message<Label>>(
                new Message<string>(payload, "getLabelParams"),
                "getLabelParams"
            ).ConfigureAwait(false);

            return response.Data;
        }

        public async Task<bool> DelLabel(string tag, double id)
        {
            string payload = $"{tag}|{id}";

            await _transport.SendAsync<Message<string>, Message<string>>(
                new Message<string>(payload, "delLabel"),
                "delLabel"
            ).ConfigureAwait(false);

            return true;
        }

        public async Task<bool> DelAllLabels(string tag)
        {
            await _transport.SendAsync<Message<string>, Message<string>>(
                new Message<string>(tag, "delAllLabels"),
                "delAllLabels"
            ).ConfigureAwait(false);

            return true;
        }

        public void InitializeCorrelationId(int startCorrelationId)
        {
            if (_transport is IQuikTransportEx transportEx)
            {
                transportEx.InitializeCorrelationId(startCorrelationId);
            }
            else
            {
                throw new NotSupportedException("Transport does not support correlation ID initialization.");
            }
        }
    }
}