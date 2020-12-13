using System.Linq;

namespace SimpBot.Custom_Classes
{

    public class ServerData
    {
        private string prefix;
        private ulong defaultRole;
        private ulong welcomeChannel;
        private string welcomeMessage;

        public ServerData()
        {
            prefix = "!";
            defaultRole = 0;
            welcomeChannel = 0;
            welcomeMessage = "";
        }
        
        //--Parseing/Serializing Data--
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
                    case("welcomeMessage"):
                        res.ParseWelcomeMessage(sarr[1]);
                        break;
                }
            }

            return res;
        }

        private void ParseWelcomeMessage(string raw)
        {
            string[] dataarray = raw.Split("message: ", 2);
            welcomeChannel = ulong.Parse(dataarray[0]);
            welcomeMessage = dataarray[1];
        }
        
        public string Serialize(ulong guildId)
        {
            return (
                "prefix: " + prefix + "\n" +
                "defaultRole: " + defaultRole + "\n" +
                "welcomeMessage: " + welcomeChannel + "message: " + welcomeMessage
            );
        }
        
        //--Prefix--
        public void SetPrefix(string newPrefix)
        {
            prefix = newPrefix;
        }

        public string GetPrefix()
        {
            return prefix;
        }

        //--Default Role--
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

        //--Welcome Message--
        public void SetWelcomeMessage(ulong channel, string message)
        {
            welcomeChannel = channel;
            welcomeMessage = message;
        }

        public (ulong channel, string message) GetWelcomeMessage()
        {
            return (welcomeChannel, welcomeMessage);
        }

        public void RemoveWelcomeMessage()
        {
            welcomeMessage = "";
            welcomeChannel = 0;
        }

        public bool HasWelcomeMessage()
        {
            return welcomeChannel != 0;
        }
    }
}
