using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using Victoria.Enums;
using System.Linq;
using System.Web;
using Discord.Commands;

namespace SimpBot.Services
{
    public class MusicService
    {
        private LavaConfig _lavaConfig;
        private LavaNode _lavaNode;
        private DiscordSocketClient _client;
        private LavaPlayer _player;
        public MusicService(LavaConfig lavaConfig, LavaNode lavanode, DiscordSocketClient client)
        {
            _lavaConfig = lavaConfig;
            _lavaNode = lavanode;
            _client = client;
        }

        public Task InitalizeAsync()
        {
            _client.Ready += clientReady;
            _lavaNode.OnLog += LogAsync;
            _lavaNode.OnTrackEnded += TrackEnded;
            return Task.CompletedTask;
        }


        public async Task ConnectAsync(SocketVoiceChannel voiceChannel, ITextChannel txtChannel)
            => await _lavaNode.JoinAsync(voiceChannel, txtChannel);

        private void SetPlayer(IGuild guild)
        {
            _player = _lavaNode.GetPlayer(guild);
        }

        public async Task<string> PlayAsync(string query, IGuild guild)
        {
            SetPlayer(guild);
            string searchQuery = "";
            try
            {
                if (query.Contains("youtu"))
                {
                    Util.Log("MUSIC: YouTube Link Detected");
                    Uri uri = new Uri(query);
                    var attributes = HttpUtility.ParseQueryString(uri.Query);

                    if (attributes.AllKeys.Contains("v"))
                    {
                        searchQuery = attributes.Get("v");
                    }
                    else
                    {

                        searchQuery = uri.Segments.Last();
                    }

                }
                else
                {
                    searchQuery = query;
                }
            } catch (Exception e)
            {
                Util.Log("MUSIC: Query exception: " + e.Message);
                searchQuery = query;
            }
            
            var results = await _lavaNode.SearchYouTubeAsync(searchQuery);
            if (results.LoadStatus == LoadStatus.NoMatches)
            {
                return "No matches found. - " + searchQuery;
            } else if (results.LoadStatus == LoadStatus.LoadFailed)
            {
                return "Load Failed!";
            }

            //Console.WriteLine("SEARCHING FOR: " + searchQuery + " SEARCH RESULT: " + results.LoadStatus.ToString());

            var track = results.Tracks.FirstOrDefault();

            if(_player.PlayerState == PlayerState.Playing)
            {
                _player.Queue.Enqueue(track);
                return $"*{track.Title}* has been added to the queue.";
            }
            else
            {
                await _player.PlayAsync(track);
                return $"**Now playing:** *{track.Title}*\n{track.Url}";
            }
        }

        public async Task LeaveAsync(SocketVoiceChannel voiceChannel)
        {
            _lavaNode.GetPlayer(voiceChannel.Guild).Queue.Clear(); //clears queue on leave
            await _lavaNode.LeaveAsync(voiceChannel);
        }

        private async Task TrackEnded(TrackEndedEventArgs endEvent)
        {
            if (!endEvent.Reason.ShouldPlayNext())
                return;
            
            if(!endEvent.Player.Queue.TryDequeue(out var item) || !(item is LavaTrack nextTrack))
            {
                await endEvent.Player.TextChannel.SendMessageAsync("Queue empty.");
                return;
            }

            await endEvent.Player.PlayAsync(nextTrack);
        }

        public async Task StopAsync(IGuild guild)
        {
            SetPlayer(guild);

            if (_player is null)
                return;

            await _player.StopAsync();
        }

        public async Task<String> SkipAsync(IGuild guild)
        {
            SetPlayer(guild);

            if (_player is null || _player.Queue.Count is 0)
                return "Nothing in queue!";

            LavaTrack oldTrack = _player.Track;
            await _player.SkipAsync();
            return $"**Skipped:** *{oldTrack.Title}*\n**Now playing:** *{_player.Track.Title}*";
        }

        public async Task<String> SetVolumeAsync(IGuild guild, ushort vol)
        {
            SetPlayer(guild);

            if (vol > 150 || vol < 2)
                return "Invalid volume level!";


            await _player.UpdateVolumeAsync(vol);
            return $"Volume set to {vol}!";
        }

        public async Task<string> PauseOrResumeAsync(IGuild guild, string command)
        {
            SetPlayer(guild);

            if (_player is null)
                return "Player isn't playing!";

            if(_player.PlayerState == PlayerState.Paused )
            {
                await _player.ResumeAsync();
                return "Music resumed!";
            }
            if (command.Equals("resume"))
                return "Music is already playing!";

            await _player.PauseAsync();
            return "Paused music!";
        }


        private Task LogAsync(LogMessage logMessage)
        {
            Util.Log($"LAVA: {logMessage.Message}");

            return Task.CompletedTask;
        }

        private async Task clientReady()
        {
            if (!_lavaNode.IsConnected)
            {
                _lavaConfig.LogSeverity = LogSeverity.Info;
                await _lavaNode.ConnectAsync();
            }
        }

        public bool NodeHasPlayer(IGuild guild)
        {
            return _lavaNode.HasPlayer(guild);
        }

        public bool HasMusicPrivilege(SocketCommandContext context)
        {
            return context.User.Id == 215044487871725573;
        }
    }
}
