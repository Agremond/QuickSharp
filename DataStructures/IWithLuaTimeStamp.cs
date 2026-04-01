// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    ///
    /// </summary>
    public interface IWithLuaTimeStamp
    {
        // TODO change to TimeStamp without refactoring and add cast to DateTime
        // then replace all assignments.
        /// <summary>
        /// Lua timestamp
        /// </summary>
        [JsonPropertyName("lua_timestamp")]
        double LuaTimeStamp { get; }
    }
}