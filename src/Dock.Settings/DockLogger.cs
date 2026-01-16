// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;

namespace Dock.Settings;

/// <summary>
/// Provides helper methods for diagnostics logging across Dock components.
/// </summary>
public static class DockLogger
{
    /// <summary>
    /// Logs the specified message in all configurations when diagnostics logging is enabled.
    /// </summary>
    /// <param name="category">A short category name describing the log source.</param>
    /// <param name="message">The log message.</param>
    public static void Log(string category, string message)
    {
        LogInternal(category, message);
    }

    /// <summary>
    /// Logs the specified message only in DEBUG builds when diagnostics logging is enabled.
    /// </summary>
    /// <param name="category">A short category name describing the log source.</param>
    /// <param name="message">The log message.</param>
    /// <summary>
    /// Logs the specified message when diagnostics logging is enabled.
    /// This method is not DEBUG-only so diagnostics can be enabled in release builds via DockSettings.
    /// </summary>
    public static void LogDebug(string category, string message)
    {
        LogInternal(category, message);
    }

    private static void LogInternal(string category, string message)
    {
        if (!DockSettings.EnableDiagnosticsLogging)
        {
            return;
        }

        var formattedMessage = $"[Dock] {category}: {message}";

        if (DockSettings.DiagnosticsLogHandler is { } handler)
        {
            handler(formattedMessage);
            return;
        }

        Debug.WriteLine(formattedMessage);
    }
}
