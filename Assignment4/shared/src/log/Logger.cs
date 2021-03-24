using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

/// <summary>
/// Debug class with utility methods: Log, LogWarning, LogError and Assert
/// Inspired by the Unity implementation is the Debug class. Gets stripped from Release builds.
/// </summary>
public static class Logger {
    private const string logFormat = " [{0}.{1} (at {3}:{2})]";

    private static string GetString(object message) {
        if (message == null) return "Null";

        return message is IFormattable formattable ? formattable.ToString(null, CultureInfo.InvariantCulture) : message.ToString();
    }

    public static void Except(Exception exception, bool includeStackTrace = false, bool includeFileInfo = false, bool includeTimestamp = false) {
        var message = exception.Message;
        if (includeStackTrace) message += $"\n{exception.StackTrace}";
        Error(message, "EXCEPTION", includeFileInfo, includeTimestamp);
    }

    public static void Info(object message, string messageTitle = "INFO", bool includeFileInfo = false, bool includeTimestamp = false) {
#if DEBUG
        var fileInfo = includeFileInfo ? GetFileInfo() : "";
        messageTitle = includeTimestamp ? GetTitleWithDate(messageTitle) : messageTitle;

        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.Write("[" + messageTitle + "]");
        Console.ResetColor();
        Console.Write(" " + GetString(message));
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(fileInfo);
        Console.ResetColor();
#endif
    }

    public static void Warn(object message, string messageTitle = "WARN", bool includeFileInfo = false, bool includeTimestamp = false) {
#if DEBUG
        var fileInfo = includeFileInfo ? GetFileInfo() : "";
        messageTitle = includeTimestamp ? GetTitleWithDate(messageTitle) : messageTitle;

        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.DarkYellow;
        Console.Write("[" + messageTitle + "]");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write(" " + GetString(message));
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(fileInfo);
        Console.ResetColor();
#endif
    }

    public static void Error(object message, string messageTitle = "ERROR", bool includeFileInfo = false, bool includeTimestamp = false) {
#if DEBUG
        var fileInfo = includeFileInfo ? GetFileInfo() : "";
        messageTitle = includeTimestamp ? GetTitleWithDate(messageTitle) : messageTitle;

        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.DarkRed;
        Console.Write("[" + messageTitle + "]");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write(" " + GetString(message));
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(fileInfo);
        Console.ResetColor();
#endif
    }

    private static string GetTitleWithDate(string messageTitle) {
        return $"{DateTime.Now:HH:mm:ss dd/mm/yyyy} - {messageTitle}";
    }

    private static string GetFileInfo() {
        var stack = new StackFrame(2, true);
        var mth = stack.GetMethod();
        var fname = stack.GetFileName();
        var lineNumber = stack.GetFileLineNumber();
        var fileName = fname?.Substring(fname.LastIndexOf("\\", StringComparison.InvariantCulture) + 1);
        var className = mth.ReflectedType?.Name;
        var method = new StringBuilder();
        method.Append(mth.Name);
        method.Append("(");
        var methodParameters = mth.GetParameters();
        for (var i = 0; i < methodParameters.Length; i++) {
            method.Append(methodParameters[i].ParameterType);
            if (i != methodParameters.Length - 1) method.Append(", ");
        }

        method.Append(")");
        var fileInfo = string.Format(logFormat, className, method, lineNumber, fileName);
        return fileInfo;
    }
}