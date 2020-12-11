using Discord.WebSocket;
using System;

namespace SimpBot
{
    public class WelcomeMessageService
    {
        public string SetWelcomeMessage(SocketGuild guild, string command)
        {
            String[] arguments = command.Split(" ");
            Console.WriteLine("> " + command);
            
            return arguments[1];
        }

        public string RemoveWelcomeMessage(SocketGuild guild)
        {
            if (!BotSeGetServerData(guild).HasWelcomeMessage())
                return "Welcome message is not active";
            
            GetServerData(guild).RemoveWelcomeMessage();
            SaveServerData(guild.Id);
            return "Welcome message deactivated";
        }

        public bool IsWelcomeMessageActive(SocketGuild guild)
        {
            return GetServerData(guild).HasWelcomeMessage();
        }

        public (ulong channel, string message) GetWelcomeMessage(SocketGuild usrGuild)
        {
            
        }
    }
}