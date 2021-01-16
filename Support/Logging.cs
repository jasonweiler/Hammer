using System;

namespace Hammer.Support
{
    public enum LogLevel
    {
        Critical,
        Error,
        Warning,
        Info,
    }

    public class Log
    {
        public static LogLevel LogLevel { get; set; } = LogLevel.Error; 

        public static void Critical(string text)
        {
            LogHelper(LogLevel.Critical, text);
        }

        public static void Error(string text)
        {
            LogHelper(LogLevel.Error, text);
        }

        public static void Warning(string text)
        {
            LogHelper(LogLevel.Warning, text);
        }

        public static void Info(string text)
        {
            LogHelper(LogLevel.Info, text);
        }

        static void LogHelper(LogLevel level, string text)
        {
            if (level <= LogLevel)
            {
                Out($"{level} - {text}");
            }
        }

        public static void Exception(Exception ex, string text="")
        {
            Out($"Exception - {text}\n{ex}");
        }
        
        public static void Out(string text="")
        {
            Console.Out.WriteLine(text);
        }
    }
}
