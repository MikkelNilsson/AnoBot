using System;
using System.Collections.Generic;
using System.IO;
using Discord;
using SimpBot.Custom_Classes;

namespace SimpBot
{
    public class DataService
    {
        
        private Dictionary<ulong, ServerData> botData;

        private string path;

        private string divider = "";

        public string Divider
        {
            get => divider;
        }

        public DataService()
        {
            Util.Log("Initializing data service");
            if (Environment.CurrentDirectory.Contains('/')) divider = "/";
            else divider = "\\";
            path = Environment.CurrentDirectory + divider + "Data";
            LoadData();
        }
        
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

        public ServerData GetServerData(ulong guildId)
        {
            if (!botData.ContainsKey(guildId))
            {
                botData.Add(guildId, new ServerData());
                SaveServerData(guildId);
                return botData[guildId];
            }
            return botData[guildId];
        }
        
    }
}