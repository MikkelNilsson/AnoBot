using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using Victoria.Enums;
using System.Linq;
using System.Net;
using Discord.Commands;
using Victoria.Responses.Rest;

namespace SimpBot.Services
{
    
    //Done TODO Create fast forward: !ff 15 -> skip 15 secs of the song
    //TODO move functionality: !move 15 1 -> moves song in position 15 to position 1
    //TODO queue functionality: !queue -> pretty print queue somehow.
    //TODO BASSBOOST funtionality: !bassboost 2 -> bassboost 2 out of 10
    //Done TODO queue a playlist: !playlist "link" -> queue every song in playlist (up to 25 songs)
    //TODO Soundcloud play with specific soundcloud link or !soundcloud
    //TODO Automatic disconnect after 5 mins with nobody in the channel
    public class MusicService
    {
        private readonly LavaConfig _lavaConfig;
        private readonly LavaNode _lavaNode;
        private readonly DiscordSocketClient _client;
        private LavaPlayer _player;
        public MusicService(LavaConfig lavaConfig, LavaNode lavaNode, DiscordSocketClient client)
        {
            _lavaConfig = lavaConfig;
            _lavaNode = lavaNode;
            _client = client;
        }

        public Task InitializeAsync()
        {
            _client.Ready += ClientReady;
            _lavaNode.OnLog += Log;
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
            SearchResponse results;
            if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
            {
                Util.Log("MUSIC: Link Detected");
                results = await _lavaNode.SearchAsync(query);
            }
            else
            {
                Util.Log($"MUSIC: Youtube search: {query}");
                results = await _lavaNode.SearchYouTubeAsync(query);
            }

            switch (results.LoadStatus)
            {
                case LoadStatus.NoMatches:
                    return "No matches found. - " + query;
                case LoadStatus.LoadFailed:
                    Util.Log($"MUSIC: LOAD FAILED: {results.Exception.Message}");
                    return "Load Failed!";
                case LoadStatus.SearchResult:
                case LoadStatus.TrackLoaded:
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
                case LoadStatus.PlaylistLoaded:
                    //TODO fix playlists where it won't load some playlists... no idea why...
                    foreach (var t in results.Tracks)
                    {
                        if (_player.PlayerState != PlayerState.Playing)
                        {
                            await _player.PlayAsync(t);
                            await _player.ResumeAsync();
                            continue;
                        }
                        _player.Queue.Enqueue(t);
                    }

                    return $"*{results.Playlist.Name}* loaded with {results.Tracks.Count} songs!";
                default:
                    return "Something happened, but I'm not gonna tell you what! HAHA!";
            }
        }

        public async Task<string> FastForward(SocketCommandContext context, int secs)
        {
            SetPlayer(context.Guild);
            var pos = _player.Track.Position + TimeSpan.FromSeconds(secs);
            if (pos > _player.Track.Duration)
            {
                return "Fast forward extended track length.";
            }
            await _player.SeekAsync(_player.Track.Position + TimeSpan.FromSeconds(secs));
            return $"Fast forwarded to {(_player.Track.Position.TotalHours >= 1 ? _player.Track.Position.Hours + ":" : "") + _player.Track.Position.Minutes + ":" + _player.Track.Position.Seconds}.";
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
            
            if(!endEvent.Player.Queue.TryDequeue(out var item))
            {
                await endEvent.Player.TextChannel.SendMessageAsync("Queue empty.");
                return;
            }

            await endEvent.Player.PlayAsync(item);
            await endEvent.Player.TextChannel.SendMessageAsync(
                $"**Now playing:** *{item.Title}*\n{item.Url}");
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


        private Task Log(LogMessage logMessage)
        {
            Util.Log($"LAVA: {logMessage.Message}");

            return Task.CompletedTask;
        }

        private async Task ClientReady()
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
            return (Util.isAno(context) || context.Guild.Id == 681785163595644929);
        }
    }
}
