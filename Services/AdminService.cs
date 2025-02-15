using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SimpBot.Modules;

namespace SimpBot.Services
{
    public class AdminService
    {
        private MusicService _music;
        private DiscordSocketClient _client;

        public AdminService(DiscordSocketClient client, MusicService music)
        {
            _music = music;
            _client = client;
        }
        public async Task<string> getActiveMusicServers(SocketCommandContext context)
        {
            if (!Util.isAdminChannel(context.Channel, context.Guild)) return "";
            var ps = _music.GetPlayers();
            if (ps.Count() == 0)
            {
                return "No players active";
            }
            string res = "Active Players:";
            foreach (var player in ps)
            {
                res += "\n" + player.VoiceChannel.Guild.Name;
            }

            return res;
        }

        public async Task<string> getAllServers(SocketCommandContext context)
        {
            if (!Util.isAdminChannel(context.Channel, context.Guild)) return "";
            var guilds = _client.Guilds;
            if (guilds.Count() == 0)
            {
                return "No servers active";
            }
            string res = "Active Servers:";
            foreach (var guild in guilds)
            {
                res += "\n" + guild.Name;
            }

            return res;

        }
    }
}