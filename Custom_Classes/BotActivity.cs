using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpBot.Custom_Classes
{
    class BotActivity : IActivity
    {
        public string Name => "!help";

        public ActivityType Type => ActivityType.Listening;

        public ActivityProperties Flags => throw new NotImplementedException();

        public string Details => "Hello Sailor! <3";
    }
}
