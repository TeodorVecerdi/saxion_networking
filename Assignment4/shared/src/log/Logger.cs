using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using shared.serialization;

public static class Logger {
    private const string logFormat = " [{0}.{1} (at {3}:{2})]";
    private const bool kIncludeStackTrace = false;
    private const bool kIncludeFileInfo = false;
    private const bool kIncludeTimestamp = false;

    private static string GetString(object message) {
        if (message == null) return "Null";

        return message is IFormattable formattable ? formattable.ToString(null, CultureInfo.InvariantCulture) : message.ToString();
    }

    [Conditional("DEBUG")]
    public static void Except(Exception exception, object ctx, bool includeStackTrace = kIncludeStackTrace, bool includeFileInfo = kIncludeFileInfo, bool includeTimestamp = kIncludeTimestamp) {
        var message = exception.Message;
        if (includeStackTrace) message += $"\n{exception.StackTrace}";
        Error(message, ctx, "EXCEPTION", includeFileInfo, includeTimestamp);
    }
    
    [Conditional("DEBUG")]
    public static void Colored(object message, ConsoleColor color, object ctx = null, string messageTitle = "LOG", bool includeFileInfo = kIncludeFileInfo, bool includeTimestamp = kIncludeTimestamp) {
        WriteMessage(message, ctx, messageTitle, color, includeFileInfo, includeTimestamp);
    }

    [Conditional("DEBUG")]
    public static void Info(object message, object ctx = null, string messageTitle = "INFO", bool includeFileInfo = kIncludeFileInfo, bool includeTimestamp = kIncludeTimestamp) {
        WriteMessage(message, ctx, messageTitle, ConsoleColor.Blue, includeFileInfo, includeTimestamp);
    }

    [Conditional("DEBUG")]
    public static void Warn(object message, object ctx = null, string messageTitle = "WARN", bool includeFileInfo = kIncludeFileInfo, bool includeTimestamp = kIncludeTimestamp) {
        WriteMessage(message, ctx, messageTitle, ConsoleColor.DarkYellow, includeFileInfo, includeTimestamp);
    }

    [Conditional("DEBUG")]
    public static void Error(object message, object ctx = null, string messageTitle = "ERROR", bool includeFileInfo = kIncludeFileInfo, bool includeTimestamp = kIncludeTimestamp) {
        WriteMessage(message, ctx, messageTitle, ConsoleColor.DarkRed, includeFileInfo, includeTimestamp);
    }
    
    [Conditional("DEBUG")]
    private static void WriteMessage(object message, object ctx, string messageTitle, ConsoleColor color, bool includeFileInfo, bool includeTimestamp) {
        var fileInfo = includeFileInfo ? GetFileInfo() : "";
        messageTitle = includeTimestamp ? GetTitleWithDate(messageTitle) : messageTitle;

        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = color;
        Console.Write("[" + messageTitle + $"]{(ctx != null ? $" @ {Utils.FriendlyName(ctx.GetType())}" : "")}");
        Console.ResetColor();
        Console.ForegroundColor = color;
        Console.Write($" {GetString(message)}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(fileInfo);
        Console.ResetColor();
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