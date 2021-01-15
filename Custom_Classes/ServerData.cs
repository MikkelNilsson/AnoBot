using System.Linq;

namespace SimpBot.Custom_Classes
{

    public class ServerData
    {
        private string prefix;
        private ulong defaultRole;
        private ulong welcomeChannel;
        private string welcomeMessage;
        private ulong leaveChannel;

        public ServerData()
        {
            prefix = "!";
            defaultRole = 0;
            welcomeChannel = 0;
            welcomeMessage = "";
            leaveChannel = 0;
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
                    case("leaveMessage"):
                        res.leaveChannel = ulong.Parse(sarr[1]);
                        break;
                }
            }

            return res;
        }

        private void ParseWelcomeMessage(string raw)
        {
            string[] dataarray = raw.Split("message: ", 2);
            welcomeChannel = ulong.Parse(dataarray[0]);
            welcomeMessage = dataarray[1].Replace("|NewlinE|", "\n");
        }
        
        public string Serialize(ulong guildId)
        {
            return (
                (prefix != "!" ? "prefix: " + prefix + "\n" : "") +
                (HasDefaultRole() ? "defaultRole: " + defaultRole + "\n" : "") +
                (HasWelcomeMessage() ? "welcomeMessage: " + welcomeChannel + "message: " + welcomeMessage.Replace("\n", "|NewlinE|") + "\n" : "") +
                (HasLeaveMessage() ? "leaveMessage: " + leaveChannel + "\n" : "")
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
        
        //--Leave Message--
        public void ActivateLeaveMessage(ulong channel)
        {
            leaveChannel = channel;
        }

        public void DisableLeaveMessage()
        {
            leaveChannel = 0;
        }

        public bool HasLeaveMessage()
        {
            return leaveChannel != 0;
        }

        public ulong getLeaveChannel()
        {
            return leaveChannel;
        }
    }
}
