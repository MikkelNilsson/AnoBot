using System.Linq;

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

        public static (ulong guildId, ServerData data) Deserialize(string serializeString)
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

        public void SetPrefix(string newPrefix)
        {
            prefix = newPrefix;
        }

        public string GetPrefix()
        {
            return prefix;
        }

        public bool hasDefaultRole()
        {
            return !(defaultRole == 0);
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
            return "newGuild-" + guildId + "\n" + 
                "prefix: " + prefix + "\n" + 
                "defaultRole: " + defaultRole + "\n" + 
                "|-|-|end\n";
        }
    }
}
