using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace SimpBot
{
    public static class Util
    {
        public static void Log(string message, LogSeverity logSeverity = LogSeverity.Info, string source = "Unknown" )
        {
            //add append to file to store logs log term kind of (maybe 14 days worth of logs)
            Log(new LogMessage(logSeverity, source , message));
        }
        public static Task Log(LogMessage logMessage)
        {
            Console.WriteLine(DateTime.UtcNow.ToString("[MM/dd/yyyy HH:mm:ss] ") + logMessage.Message);
            return Task.CompletedTask;
        }

        public static bool isAno(SocketGuildUser user)
        {
            return (user.Id == 215044487871725573 || user.Id == 763301348078387260);
        }

        public static bool isWuBot(IUser user)
        {
            return user.Id == 575320025410437131 || user.Id == 743104199583334451;
        }

        public static bool isAdminChannel(ISocketMessageChannel chan, SocketGuild guild)
        {
            return chan.Id == 896876210322305034 && guild.Id == 795413460074102795;
        }
        
        //TODO Create a parse function, to parse channels. Used for ActivateLeaveMessage and SetWelcomeMessage.
    }
}