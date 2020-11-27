using Discord.Commands;
using System.Threading.Tasks;

namespace SimpBot.Modules
{
    class Ping : ModuleBase<SocketCommandContext>
    {
        [Command("Ping")]
        public async Task Pong()
        {
            await ReplyAsync("PONG!");
        }
    }
}
