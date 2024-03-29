﻿using System;
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
            _dataService.GetServerData(guild.Id).SetPrefix(prefix);
            _dataService.SaveServerData(guild.Id);
            return $"Prefix set to {prefix}";
        }

        public string GetPrefix(IGuild guild)
        {
            return _dataService.GetServerData(guild.Id).GetPrefix();
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

            if (gUser.GuildPermissions.Has(GuildPermission.ManageGuild) || Util.isAno(context))
            {
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
                res.AddField("**__Leave Message:__**",
                    "`LeaveMessage <channel>` or use `LME <channel>`: A message is sent when a user leaves the server in the specified channel.\n" +
                    "`RemoveLeaveMessage` or use `RLM`: Disables the leave message function.");
            }

            var embed = res.WithAuthor(context.Client.CurrentUser)
                .WithFooter(
                    (context.User.Id == 614083078100484106 ? "❤💕Thanks for using WUBot, Thomas!💕❤" : "Thanks for using WUBot!"))
                .WithCurrentTimestamp()
                .Build();
            await dmChannel.SendMessageAsync(embed: embed);
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
    }
}
