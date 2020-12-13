using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace SimpBot
{
    public class BotSettingsService
    {
        private DataService _dataService;

        public BotSettingsService(DataService dataService)
        {
            _dataService = dataService;
        }

        //--- Prefix ---
        public string SetPrefix(IGuild guild, string prefix)
        {
            _dataService.GetServerData(guild.Id).SetPrefix(prefix);
            return $"Prefix set to {prefix}";
        }
        
        public string GetPrefix(IGuild guild)
        {
            return _dataService.GetServerData(guild.Id).GetPrefix();
        }
        
        //--- Help ---
        public async Task HelpAsync(SocketCommandContext context)
        {
            await context.Message.DeleteAsync();
            var dmChannel = context.User.GetOrCreateDMChannelAsync().Result;
            IGuildUser gUser = context.User as IGuildUser;

            var res = new EmbedBuilder
            {
                Title = "Help:",
                Description =
                    $"To use commands on **{context.Guild.Name}**, use \'**{GetPrefix(context.Guild)}**\' in front of one of the following commands:",
                Color = Color.Blue
            };
            
            if (gUser.GuildPermissions.Has(GuildPermission.Administrator)) {
                res.AddField("**__Bot Settings:__**",
                    "`SetPrefix <prefix>` or use `SP <prefix>`: Use to set prefix for commands.");
                res.AddField("**__Default Role:__**",
                    "`SetDefaultRole <@role>` or use `SDR <@role>`: Use to set default role. Role will be added to every user joining the server. *(Does not affect current members)*\n" +
                    "`RemoveDefaultRole` or use `RDR`: Use to remove the default role function.");

                res.AddField("**__Welcome Message:__**",
                    "`SetWelcomeMessage <channel> <message>` or use `SWM <channel> <message>`: Sets the message sent when people join.\n" +
                    "__Channel__: Tag the channel you want the message sent in.\n" +
                    "__Message__: Use *¤name¤* where you want to tag the person joining.\n" +
                    "`RemoveWelcomeMessage` or use `RWM`: Removes the current active welcome message.");
            }

            var embed = res.WithAuthor(context.Client.CurrentUser)
                .WithFooter(
                    "Thanks for using WUBot!")
                .WithCurrentTimestamp()
                .Build();
            await dmChannel.SendMessageAsync(embed:embed);
        }

        //--- Default Role ---
        public IRole GetDefaultRole(IGuild guild)
        {
            if (_dataService.GetServerData(guild.Id).HasDefaultRole())
                return guild.GetRole(_dataService.GetServerData(guild.Id).GetDefaultRole());
            else return guild.EveryoneRole;
        }
        
        public string SetDefaultRole(IGuild guild, string command)
        {
            if (!command.StartsWith("<@&"))
                return $"{command} is not a valid role!";
            command = command.Substring(3, command.Length - 4);
            IRole dRole = null;
            foreach (IRole role in guild.Roles)
            {
                if (role.Id == ulong.Parse(command))
                {
                    dRole = role;
                    break;
                }
            }
            if (dRole is null)
                return $"Role not found {command}";

            _dataService.GetServerData(guild.Id).SetDefaultRole(dRole.Id);
            _dataService.SaveServerData(guild.Id);
            return $"Default role set to {dRole.Name}";
        }

        public string RemoveDefaultRole(SocketGuild guild)
        {
            if (!_dataService.GetServerData(guild.Id).HasDefaultRole())
                return "No default role has been set.";

            _dataService.GetServerData(guild.Id).RemoveDefaultRole();
            _dataService.SaveServerData(guild.Id);
            return "Removed default role.";
        }

        //--- Welcome Message ---
        
    }
}
