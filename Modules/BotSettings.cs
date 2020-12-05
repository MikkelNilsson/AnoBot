using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace SimpBot.Modules
{
    public class BotSettings : ModuleBase<SocketCommandContext>
    {
        private BotSettingsService _settingsService;

        public BotSettings(BotSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [Command("SetPrefix")]
        [Alias("SP")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Setprefix(string remainder)
        {
            _settingsService.SetPrefix(Context.Guild, remainder.Trim());
            await ReplyAsync($"Prefix set to \'{remainder.Trim()}\'!");
        }

        [Command("Help")]
        [Alias("H")]
        [RequireBotPermission(Discord.GuildPermission.ManageMessages)]
        public async Task Help()
        {
            await _settingsService.HelpAsync(Context);
        }

        [Command("SetDefaultRole")]
        [Alias("SDR")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task SetDefaultRole(string remainder)
        {
            await ReplyAsync(_settingsService.SetDefaultRole(Context.Guild, remainder));
            await Context.Message.DeleteAsync();
        }

        [Command("RemoveDefaultRole")]
        [Alias("RDR")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemoveDefaultRole()
        {
            await ReplyAsync(_settingsService.RemoveDefaultRole(Context.Guild));
        }
    }
}
