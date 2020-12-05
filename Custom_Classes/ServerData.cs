﻿using System.Linq;

namespace SimpBot.Custom_Classes
{

    class ServerData
    {
        private string prefix;
        private ulong defaultRole;

        public ServerData()
        {
            prefix = "!";
            defaultRole = 0;
        }

        public static (ulong guildId, ServerData data) OldDeserialize(string serializeString)
        {
            //TODO FIX THIS TRASH BS!
            serializeString.TrimStart('\\').TrimStart('n');
            string[] sarr = serializeString.Split("\n").Where(x => x != "").ToArray();
            string[] gid = sarr[0].Split("-");
            ulong GuildId = ulong.Parse(gid[1]);
            ServerData tmp = new ServerData();
            tmp.SetPrefix(sarr[1].Substring(8));
            tmp.SetDefaultRole(ulong.Parse(sarr[2].Substring(13)));
            return (GuildId, tmp);
        }
        public static ServerData Deserialize(string serializeString)
        {
            ServerData res = new ServerData();
            foreach (string s in serializeString.Split("\n"))
            {
                string[] sarr = s.Split(": ", 2);
                switch (sarr[0])
                {
                    case("prefix"):
                        res.prefix = sarr[1];
                        break;
                    case("defaultRole"):
                        res.defaultRole = ulong.Parse(sarr[1]);
                        break;
                }
            }

            return res;
        }

        public void SetPrefix(string newPrefix)
        {
            prefix = newPrefix;
        }

        public string GetPrefix()
        {
            return prefix;
        }

        public bool HasDefaultRole()
        {
            return defaultRole != 0;
        }

        public ulong GetDefaultRole()
        {
            return defaultRole;
        }

        public void SetDefaultRole(ulong dRoleId)
        {
            defaultRole = dRoleId;
        }

        internal void RemoveDefaultRole()
        {
            defaultRole = 0;
        }

        public string Serialize(ulong guildId)
        {
            return "prefix: " + prefix + "\n" +
                   "defaultRole: " + defaultRole;
        }
    }
}
