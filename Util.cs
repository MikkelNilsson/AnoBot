﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

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

        public static bool isAno(SocketCommandContext context)
        {
            return (context.User.Id == 215044487871725573 || context.User.Id == 763301348078387260);
        }

        public static bool isMe(IUser user)
        {
            return user.Id == 575320025410437131 || user.Id == 743104199583334451;
        }
        
        //TODO Create a parse function, to parse channels. Used for ActivateLeaveMessage and SetWelcomeMessage.
    }
}