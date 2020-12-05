using System;
using System.Threading.Tasks;
using Discord;

namespace SimpBot
{
    public static class Util
    {
        public static void Log(string message, LogSeverity logSeverity = LogSeverity.Info, string source = "Unknown" )
        {
            Log(new LogMessage(logSeverity, source , message));
        }
        public static Task Log(LogMessage logMessage)
        {
            Console.WriteLine(DateTime.UtcNow.ToString("[MM/dd/yyyy HH:mm:ss] ") + logMessage.Message);
            return Task.CompletedTask;
        }
    }
}