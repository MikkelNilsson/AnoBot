using System;
using System.Linq;
using Discord;
using System.Threading;
using Victoria;

namespace SimpBot.Custom_Classes
{

    public class ServerData
    {
        public string Prefix { get; set; }
        public ulong DefaultRole { get; set; }
        public ulong MusicRole { get; set; }
        private ulong welcomeChannel;
        private string welcomeMessage;
        private ulong leaveChannel;

        public bool QueueLoop { get; set; }
        public bool SingleLoop { get; set; }
        public (IUserMessage msg, int page)? MusicQueueMessage { get; set; }
        public IUserMessage NowPlayingMessage { get; set; }
        public Timer Timer { get; set; }

        public ServerData()
        {
            QueueLoop = false;
            SingleLoop = false;
            Prefix = "!";
            DefaultRole = 0;
            MusicRole = 0;
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
                    case ("prefix"):
                        res.Prefix = sarr[1];
                        break;
                    case ("defaultRole"):
                        res.DefaultRole = ulong.Parse(sarr[1]);
                        break;
                    case ("musicRole"):
                        res.MusicRole = ulong.Parse(sarr[1]);
                        break;
                    case ("welcomeMessage"):
                        res.ParseWelcomeMessage(sarr[1]);
                        break;
                    case ("leaveMessage"):
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
                (Prefix != "!" ? "prefix: " + Prefix + "\n" : "") +
                (HasDefaultRole() ? "defaultRole: " + DefaultRole + "\n" : "") +
                (HasWelcomeMessage() ? "welcomeMessage: " + welcomeChannel + "message: " + welcomeMessage.Replace("\n", "|NewlinE|") + "\n" : "") +
                (HasLeaveMessage() ? "leaveMessage: " + leaveChannel + "\n" : "")

            );
        }

        //--Default Role--
        public bool HasDefaultRole()
        {
            return DefaultRole != 0;
        }

        internal void RemoveDefaultRole()
        {
            DefaultRole = 0;
        }

        //--Music Role--

        public bool HasMusicRole()
        {
            return MusicRole != 0;
        }

        public void RemoveMusicRole()
        {
            MusicRole = 0;
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
        
        //--Connection timer--
        private bool hasMusicTimer()
        {
            return Timer != null;
        }

        private bool isTimerActive;

        public void stopMusicTimer()
        {
            Console.WriteLine("Stop timer call!");
            if (hasMusicTimer())
            {
                Timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                isTimerActive = false;
            }
        }

        public void startMusicTimer(LavaPlayer player, LavaNode node)
        {
            Console.WriteLine("start timer call!");
            if (!hasMusicTimer())
                Timer = new Timer(TimerCallback, new TimerCallbackObject
                {
                    n = node,
                    p = player
                }, TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);
            else
                Timer.Change(TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);
            isTimerActive = true;
        }

        private class TimerCallbackObject
        {
            public LavaNode n;
            public LavaPlayer p;
        }
        
        private async void TimerCallback(object o)
        {
            Console.WriteLine("timer callback function! with is active = " + (isTimerActive ? "true" : "false"));
            TimerCallbackObject t = (TimerCallbackObject) o;
            if (isTimerActive)
            {
                Console.WriteLine("Leave please");
                await t.n.LeaveAsync(t.p.VoiceChannel);
            }
            else
            {
                Console.WriteLine("Don't leave me! :(");
            }
        }
    }
}
