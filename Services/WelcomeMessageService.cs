using Discord.WebSocket;
using System;

namespace SimpBot
{
    public class WelcomeMessageService
    {
        private DataService _dataService;
        public string SetWelcomeMessage(SocketGuild guild, string command)
        {
            String[] arguments = command.Split(" ");
            Console.WriteLine("> " + command);
            
            return arguments[1];
        }

        public string RemoveWelcomeMessage(SocketGuild guild)
        {
            if (!_dataService.GetServerData(guild.Id).HasWelcomeMessage())
                return "Welcome message is not active";
            
            _dataService.GetServerData(guild.Id).RemoveWelcomeMessage();
            _dataService.SaveServerData(guild.Id);
            return "Welcome message deactivated";
        }

        public bool IsWelcomeMessageActive(SocketGuild guild)
        {
            return _dataService.GetServerData(guild.Id).HasWelcomeMessage();
        }

        public (ulong channel, string message) GetWelcomeMessage(SocketGuild usrGuild)
        {
            return (0, "");
        }
    }
}