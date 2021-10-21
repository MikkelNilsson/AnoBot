using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace SimpBot
{
    public class LeaveMessageService
    {
        private DataService _dataService;
        private DiscordSocketClient _client;
        public LeaveMessageService(DataService dataService, DiscordSocketClient client)
        {
            _dataService = dataService;
            _client = client;
        }

        private async Task SendLeaveMessage(SocketGuildUser arg)
        {
            if (_dataService.GetServerData(arg.Guild.Id).HasLeaveMessage())
            {
                var channel =
                    arg.Guild.GetChannel(_dataService.GetServerData(arg.Guild.Id).getLeaveChannel()) as
                        SocketTextChannel;
                await channel.SendMessageAsync(arg.Username + (!(arg.Nickname is null) ? " (" + arg.Nickname + ")" : "") + " left the server.");
            }
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

                return "<#" + channelId + "> is not a valid text channel.";
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