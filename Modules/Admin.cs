using Discord;
using Discord.Commands;
using SimpBot.Services;
using System.Threading.Tasks;

namespace SimpBot.Modules
{
    public class Admin : ModuleBase<SocketCommandContext>
    {
        private AdminService _admin;

        public Admin(AdminService admin)
        {
            _admin = admin;
        }
        
        [Command("ActiveMusic")]
        private async Task getActiveMusicServers()
        {
            await ReplyAsync(await _admin.getActiveMusicServers(Context));
        }
    }
}