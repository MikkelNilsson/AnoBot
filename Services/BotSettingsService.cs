using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace SimpBot
{
    public class BotSettingsService
    {
        private DataService _dataService;
        private DiscordSocketClient _client;

        public BotSettingsService(DataService dataService, DiscordSocketClient client)
        {
            _dataService = dataService;
            _client = client;
        }

        public void Initialize()
        {
            _client.UserJoined += AddDefaultRole;
        }

        private async Task AddDefaultRole(SocketGuildUser usr)
        {
            try
            {
                await usr.AddRoleAsync(GetDefaultRole(usr.Guild));
            }
            catch (Exception e)
            {
                Util.Log(e.Message + ": " + e.StackTrace);
            }
        }

        //--- Prefix ---
        public string SetPrefix(IGuild guild, string prefix)
        {
            _dataService.GetServerData(guild.Id).Prefix = prefix;
            _dataService.SaveServerData(guild.Id);
            return $"Prefix set to {prefix}";
        }

        public string GetPrefix(IGuild guild)
        {
            return _dataService.GetServerData(guild.Id).Prefix;
        }

        //--- Help ---
        public async Task HelpAsync(SocketCommandContext context, string argument)
        {
            Util.Log(argument.Trim());
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

            if (gUser.GuildPermissions.Has(GuildPermission.ManageGuild) || Util.isAno((SocketGuildUser)context.User))
            {
                res.AddField("**__Bot Settings:__**",
                    "`SetPrefix <prefix>` or use `SP <prefix>`: Use to set prefix for commands.");
                res.AddField("**__Default Role:__**",
                    "`SetDefaultRole <@role>` or use `SDR <@role>`: Use to set the role, which will be added to every user joining the server. *(Does not affect current members)*\n" +
                    "`RemoveDefaultRole` or use `RDR`: Use to remove the default role function.");

                res.AddField("**__Welcome Message:__**",
                    "`SetWelcomeMessage <channel> <message>` or use `SWM <channel> <message>`: Sets the message sent when people join.\n" +
                    "__Channel__: Tag the channel you want the message sent in.\n" +
                    "__Message__: Use *¤name¤* where you want to tag the person joining.\n" +
                    "`RemoveWelcomeMessage` or use `RWM`: Removes the current active welcome message.");
                res.AddField("**__Leave Message:__**",
                    "`LeaveMessage <channel>` or use `LME <channel>`: A message is sent when a user leaves the server in the specified channel.\n" +
                    "`RemoveLeaveMessage` or use `RLM`: Disables the leave message function.");
                res.AddField("**__Music Permissions:__**",
                    "`SetMusicRole <Role>` or use `SMR <Role>`: Sets a role to grant music privileges, only users with this role will be able to play music.\n" +
                    "`RemoveMusicRole` or use `RMR`: Removes music role, everyone can play music with WUbot.");
            }

            res.AddField("**__Music:__**",
                "`Play <query>`: Query being a youtube link or a search phrase.\n" +
                "`Queue`: Shows the queue.\n" +
                "`Skip`: Skips the current song.\n" +
                "`Clear`: Clears the queue.\n" +
                "`Leave`: The bot leaves the voice channel and clears the queue.\n" +
                "`Volume <volume level>`: Sets the volume (Level between 0 and 150).\n" +
                "`Shuffle`: Shuffles the current queue.\n" +
                "`FastForward <amount in seconds>`: Fast forwards the track a given amount.\n" +
                "`Loop`: Loops playlist.\n" +
                "`LoopSingle`: Loops the first song playing.\n");

            var embed = res.WithAuthor(context.Client.CurrentUser)
                .WithFooter(
                    (context.User.Id == 614083078100484106 ? "❤💕Thank you for using WUBot, Thomas!💕❤" : "Thank you for using WUBot!"))
                .WithUrl("https://anomark22.github.io/")
                .Build();
            await dmChannel.SendMessageAsync(embed: embed);
        }

        //--- Default Role ---
        public IRole GetDefaultRole(IGuild guild)
        {
            if (_dataService.GetServerData(guild.Id).HasDefaultRole())
                return guild.GetRole(_dataService.GetServerData(guild.Id).DefaultRole);
            else return guild.EveryoneRole;
        }

        public string SetDefaultRole(IGuild guild, string command)
        {
            command = command.Trim();
            if (!command.StartsWith("<@&"))
                return $"{command} is not a valid role!";
            command = command.Substring(3, command.Length - 4);
            IRole dRole = guild.GetRole(ulong.Parse(command));

            if (dRole is null)
                return $"Role not found: {command}";

            _dataService.GetServerData(guild.Id).DefaultRole = dRole.Id;
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

        public string SetMusicRole(SocketGuild guild, string sRole)
        {
            sRole = sRole.Trim();
            if (!sRole.StartsWith("<@&"))
                return $"{sRole} is not a valid role!";

            sRole = sRole.Substring(3, sRole.Length - 4);

            IRole mRole = guild.GetRole(ulong.Parse(sRole));

            if (mRole is null)
                return $"Role not found: {sRole}";

            _dataService.GetServerData(guild.Id).MusicRole = mRole.Id;
            _dataService.SaveServerData(guild.Id);
            return $"Music role set to {mRole.Name}";
        }

        public string RemoveMusicRole(SocketGuild guild)
        {
            if (!_dataService.GetServerData(guild.Id).HasDefaultRole())
                return "No music role has been set.";

            _dataService.GetServerData(guild.Id).RemoveMusicRole();
            _dataService.SaveServerData(guild.Id);
            return "Removed music role.";
        }
    }
}
