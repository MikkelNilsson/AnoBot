using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace SimpBot.Modules
{
    public class LeaveMessage : ModuleBase<SocketCommandContext>
    {
        private LeaveMessageService _leaveService;
        public LeaveMessage(LeaveMessageService leaveService)
        {
            _leaveService = leaveService;
        }

        [Command("LeaveMessage")]
        [Alias("LME")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetLeaveMessage(string remainder)
        {
            await ReplyAsync(_leaveService.SetLeaveMessage(Context.Guild, remainder));
        }
        
        [Command("RemoveLeaveMessage")]
        [Alias("RLM")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveLeaveMessage(string remainder)
        {
            await ReplyAsync(_leaveService.RemoveLeaveMessage(Context.Guild));
        }
    }
}