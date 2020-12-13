using System;
using System.Security.Claims;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace SimpBot.Modules
{
    public class WelcomeMessage : ModuleBase<SocketCommandContext>
    {
        private WelcomeMessageService _wms;
        
        public WelcomeMessage(WelcomeMessageService wms)
        {
            _wms = wms;
        }

        //TODO: Fix so that the whole of the command gets parsed through
        [Command("SetWelcomeMessage", true)] //might be a fix on ^
        [Alias("SWM")]
        [RequireUserPermission(GuildPermission.Administrator)]
        //TODO: Make sure bot can write in the channel suggested (Channel permissions)
        public async Task SetWelcomeMessage(string remainder)
        {
            Console.WriteLine("hmm?" + Context.Message.Content);
            await ReplyAsync(_wms.SetWelcomeMessage(Context.Guild, Context.Message.Content));
        }

        [Command("RemoveWelcomeMessage", true)]
        [Alias("RWM")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemoveWelcomeMessage()
        {
            await ReplyAsync(_wms.RemoveWelcomeMessage(Context.Guild));
        }

    }
}