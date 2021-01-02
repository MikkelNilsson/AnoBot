using System;
using Discord.WebSocket;

namespace SimpBot
{
    public class LeaveMessageService
    {
        private DataService _dataService;
        public LeaveMessageService(DataService dataService)
        {
            _dataService = dataService;
        }

        public string SetLeaveMessage(SocketGuild guild, string command)
        {
            if (command.StartsWith("<#") && command.EndsWith(">"))
            {
                ulong channelId = ulong.Parse(command.Substring(2, command.Length - 3));

                if (guild.GetChannel(channelId) is SocketTextChannel)
                {
                    _dataService.GetServerData(guild.Id).ActivateLeaveMessage(channelId);
                    _dataService.SaveServerData(guild.Id);
                    
                    return "Leave message activated on channel <#" + channelId + ">!";
                }
                
                return "<#"+ channelId + "> is not a valid text channel.";
            }
            return "Leave message setup was unsuccessful, use !help for info on command.";
        }

        public string RemoveLeaveMessage(SocketGuild guild)
        {
            if (!_dataService.GetServerData(guild.Id).HasLeaveMessage())
                return "Leave message is not active.";
            
            _dataService.GetServerData(guild.Id).DisableLeaveMessage();
            _dataService.SaveServerData(guild.Id);
            return "Leave message was removed.";
        }
    }
}