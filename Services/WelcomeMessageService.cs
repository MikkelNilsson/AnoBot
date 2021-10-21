using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace SimpBot
{
    public class WelcomeMessageService
    {
        private DataService _dataService;
        private DiscordSocketClient _client;
        private BotSettingsService _settings;

        public WelcomeMessageService(DataService dataService, DiscordSocketClient client, BotSettingsService settings)
        {
            _dataService = dataService;
            _client = client;
            _settings = settings;
        }
        
        public void Initialize()
        {
            _client.UserJoined += PlayWelcomeMessage;
        }

        private async Task PlayWelcomeMessage(SocketGuildUser usr)
        {
            try
            {
                if (IsWelcomeMessageActive(usr.Guild))
                {
                    var welcomeMessage = GetWelcomeMessage(usr.Guild);
                    var channel = (SocketTextChannel) usr.Guild.GetChannel(welcomeMessage.channel);
                    await channel.SendMessageAsync(welcomeMessage.message.Replace("¤name¤", "<@" + usr.Id + ">"));
                }
            }
            catch (Exception e)
            {
                Util.Log(e.Message +  e.StackTrace);
            }
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