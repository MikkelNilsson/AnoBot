using SimpBot.Custom_Classes;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
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
            await dmChannel.SendMessageAsync($"" +
                                             $"> __**Help:**__" +
                                             $"\n> " +
                                             $"\n> To use commands on **{context.Guild.Name}**, use \'**{GetPrefix(context.Guild)}**\' in front of one of the following commands:" +
                                             $"\n> " +
                                             (gUser.GuildPermissions.Has(GuildPermission.Administrator) ? 
                                                 $"\n> ***__Bot Settings:__***" +
                                                 $"\n> **SetPrefix <prefix>** or use **SP <prefix>**: Use to set prefix for commands."  +
                                                 $"\n> **SetDefaultRole <@role>** or use **SDR <@role>**: Use to set default role. Role will be added to every user joining the server from that point on. *(Does not affect current members)*" +
                                                 $"\n> **RemoveDefaultRole** or use **RDR**: Use to remove the default role function."  +
                                                 $"\n> " +
                                                 $"\n> ***__Welcome Message:__***" +
                                                 $"\n> **SetWelcomeMessage <channel> <message>** or use **SWM <channel> <message>**: Sets the message sent when people join." +
                                                 $"\n> __Channel__: Tag the channel you want the message sent in." +
                                                 $"\n> __Message__: Use **¤name¤** where you want to tag the person joining."
                                                 : ""));
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
