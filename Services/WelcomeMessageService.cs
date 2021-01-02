using Discord.WebSocket;
using System;

namespace SimpBot
{
    public class WelcomeMessageService
    {
        private DataService _dataService;

        public WelcomeMessageService(DataService dataService)
        {
            _dataService = dataService;
        }
        
        public string SetWelcomeMessage(SocketGuild guild, string command)
        {
            string[] arguments = command.Split(" ", 3);

            if (arguments[1].StartsWith("<#") && arguments[1].EndsWith(">"))
            {
                ulong channelId = ulong.Parse(arguments[1].Substring(2, arguments[1].Length - 3));

                if (guild.GetChannel(channelId) is SocketTextChannel)
                {

                    _dataService.GetServerData(guild.Id).SetWelcomeMessage(channelId, arguments[2]);
                    _dataService.SaveServerData(guild.Id);

                    return "Welcome message set to channel: <#" + channelId + ">!";
                }

                return "<#"+ channelId + "> is not a valid text channel.";
            }

            return "Welcome message setup was unsuccessful, use !help for info on command.";
        }

        public string RemoveWelcomeMessage(SocketGuild guild)
        {
            if (!_dataService.GetServerData(guild.Id).HasWelcomeMessage())
                return "Welcome message is not active.";
            
            _dataService.GetServerData(guild.Id).RemoveWelcomeMessage();
            _dataService.SaveServerData(guild.Id);
            return "Welcome message was removed.";
        }

        public bool IsWelcomeMessageActive(SocketGuild guild)
        {
            return _dataService.GetServerData(guild.Id).HasWelcomeMessage();
        }

        public (ulong channel, string message) GetWelcomeMessage(SocketGuild usrGuild)
        {
            return _dataService.GetServerData(usrGuild.Id).GetWelcomeMessage();
        }
    }
}