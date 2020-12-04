using System;
using System.Threading.Tasks;
using Discord;

namespace SimpBot
{
    public class Util
    {
        public static Task Log(LogMessage logMessage)
        {
            Console.WriteLine(DateTime.UtcNow.ToString("[MM/dd/yyyy HH:mm:ss] ") + logMessage.Message);
            return Task.CompletedTask;
        }
    }
}