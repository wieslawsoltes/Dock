// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Diagnostics;

namespace Dock.Model
{
    /// <summary>
    /// Dock logger.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        [Conditional("DEBUG")]
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
