using UnityEngine;

namespace Uduino
{
    public static class Log 
    {
        private static LogLevel _debugLevel;

        public static void Error(object message)
        {
            if((int)_debugLevel <= (int)LogLevel.ERROR)
                UnityEngine.Debug.LogError(message);
        }

        public static void Warning(object message)
        {
            if ((int)_debugLevel <= (int)LogLevel.WARNING)
                UnityEngine.Debug.LogWarning(message);
        }
        public static void Info(object message)
        {
            if ((int)_debugLevel <= (int)LogLevel.INFO)
                UnityEngine.Debug.Log(((string)message).RemoveLineEndings());
        }

        public static void Debug(object message)
        {
            if ((int)_debugLevel <= (int)LogLevel.DEBUG)
                UnityEngine.Debug.Log(message);
        }

        public static void SetLogLevel(LogLevel level)
        {
            _debugLevel = level;
        }

        public static string RemoveLineEndings(this string value)
        {
            if (System.String.IsNullOrEmpty(value))
            {
                return value;
            }
            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            return value.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(lineSeparator, string.Empty).Replace(paragraphSeparator, string.Empty);
        }
    }

}