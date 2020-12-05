using SimpBot.Custom_Classes;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace SimpBot
{
    public class BotSettingsService
    {
        private Dictionary<ulong, ServerData> botData;

        private string path;

        private string divider = "";
        
        public BotSettingsService()
        {
            if (Environment.CurrentDirectory.Contains('/')) divider = "/";
            else divider = "\\";
            path = Environment.CurrentDirectory + divider + "Data";
            Util.Log("PATH: " + path);
            LoadData();
        }
        //                                                                         --- Handling Server Data ------------
        public void LoadData()
        {
            
            if (!Directory.Exists(path))
            {
                Util.Log("Directory not found! Creating new directory..");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                botData = new Dictionary<ulong, ServerData>();
            }
            else
            {
                if (botData is null) botData = new Dictionary<ulong, ServerData>();

                String[] files = Directory.GetFiles(path);
                foreach (string filePath in files)
                {
                    string[] pathArray = filePath.Split(divider);
                    botData.Add(ulong.Parse(pathArray[^1].Substring(0, pathArray[^1].Length - 4)), 
                        ServerData.Deserialize(File.ReadAllText(filePath)));
                }
            }
        }

        public void SaveServerData(ulong guildId)
        {
            File.WriteAllText(path + divider + guildId + ".txt", botData[guildId].Serialize(guildId));
        }

        private ServerData GetServerData(IGuild guild)
        {
            if (!botData.ContainsKey(guild.Id))
            {
                botData.Add(guild.Id, new ServerData());
                SaveServerData(guild.Id);
                return botData[guild.Id];
            }
            return botData[guild.Id];
        }

        //                                                                         --- Prefix --------------------------
        public string SetPrefix(IGuild guild, string prefix)
        {
            GetServerData(guild).SetPrefix(prefix);
            SaveServerData(guild.Id);
            return $"Prefix set to {prefix}";
        }
        
        public string GetPrefix(IGuild guild)
        {
            return GetServerData(guild).GetPrefix();
        }
        
        //                                                                         --- Help ----------------------------
        public async Task HelpAsync(SocketCommandContext context)
        {
            await context.Message.DeleteAsync();
            var dmChannel = context.User.GetOrCreateDMChannelAsync().Result;
            await dmChannel.SendMessageAsync($"" +
                $"> __**Help:**__" +
                $"\n> " +
                $"\n> To use commands on **{context.Guild.Name}**, use \'**{GetPrefix(context.Guild)}**\' in front of one of the following commands:" +
                $"\n> " +
                /*$"\n> ***__Music:__***" +
                $"\n> **Play <song name or link>**: Use to play music in a voice channel. To use this, you must be in a voice channel." +
                $"\n> **Skip**: Use to skip the current song." +
                $"\n> **Pause**: Use to pause/unpause the current song." +
                $"\n> **Resume**: Use to unpause the current song." +
                $"\n> **Stop**: Use to stop the music." +
                $"\n> **leave**: Use to make the bot leave the voice channel." +
                $"\n> " +*/
                $"\n> ***__Bot Settings:__***" +
                $"\n> **SetPrefix <prefix>**: Use to set prefix for commands." +
                $"\n> **SetDefaultRole <@role>**: Use to set default role. Role will be added to every user joining the server from that point on. *(Does not affect current members)*" +
                $"\n> **RemoveDefaultRole**: Use to remove the default role function.");
        }

        //                                                                         --- Default Role --------------------
        public IRole GetDefaultRole(IGuild guild)
        {
            if (GetServerData(guild).HasDefaultRole())
                return guild.GetRole(GetServerData(guild).GetDefaultRole());
            else return guild.EveryoneRole;
        }
        
        public string SetDefaultRole(IGuild guild, string command)
        {
            Console.WriteLine(command);
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

            GetServerData(guild).SetDefaultRole(dRole.Id);
            SaveServerData(guild.Id);
            return $"Default role set to {dRole.Name}";
        }

        public string RemoveDefaultRole(SocketGuild guild)
        {
            if (!GetServerData(guild).HasDefaultRole())
                return "No default role has been set.";

            GetServerData(guild).RemoveDefaultRole();
            SaveServerData(guild.Id);
            return "Removed default role.";
        }
    }
}
