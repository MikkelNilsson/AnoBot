﻿using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using Victoria.Enums;
using System.Linq;
using Discord.Commands;
using Victoria.Responses.Rest;
using SimpBot.Custom_Classes;

namespace SimpBot.Services
{

    //Done TODO Create fast forward: !ff 15 -> skip 15 secs of the song
    //TODO move functionality: !move 15 1 -> moves song in position 15 to position 1
    //TODO queue functionality: !queue -> pretty print queue somehow.
    //TODO BASSBOOST funtionality: !bassboost 2 -> bassboost 2 out of 10
    //Done TODO queue a playlist: !playlist "link" -> queue every song in playlist (up to 25 songs)
    //TODO Soundcloud play with specific soundcloud link or !soundcloud
    //TODO Automatic disconnect after 5 mins with nobody in the channel
    //TODO spotify playlist retrieve songtitles and stuff to play from yt
    //TODO setting: clear queue on leave
    //TODO clear functionality -> clear queue.
    public class MusicService
    {
        private readonly LavaConfig _lavaConfig;
        private readonly LavaNode _lavaNode;
        private readonly DiscordSocketClient _client;
        private LavaPlayer _player;
        private ServerData _data;
        private readonly DataService _dataService;
        public MusicService(LavaConfig lavaConfig, LavaNode lavaNode, DiscordSocketClient client, DataService dataService)
        {
            _lavaConfig = lavaConfig;
            _lavaNode = lavaNode;
            _client = client;
            _dataService = dataService;
        }

        public Task InitializeAsync()
        {
            _client.Ready += ClientReady;
            _lavaNode.OnLog += Log;
            _lavaNode.OnTrackEnded += TrackEnded;
            _client.ReactionAdded += OnReactionAdded;
            return Task.CompletedTask;
        }


        public async Task ConnectAsync(SocketVoiceChannel voiceChannel, ITextChannel txtChannel)
            => await _lavaNode.JoinAsync(voiceChannel, txtChannel);

        private bool SetPlayer(IGuild guild)
        {
            if (!NodeHasPlayer(guild)) return false;
            _player = _lavaNode.GetPlayer(guild);
            return true;
        }

        private void SetData(IGuild guild)
        {
            _data = _dataService.GetServerData(guild.Id);
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
                    if (track is null) return "Load Failed!";
                    
                    if (_player.PlayerState == PlayerState.Playing)
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

        public string Shuffle(SocketGuild guild)
        {

            if (SetPlayer(guild) || _player.Queue.Count <= 0)
            {
                return "Queue empty, nothing to shuffle.";
            }
            _player.Queue.Shuffle();
            return "Queue Shuffled.";
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

            if (!endEvent.Player.Queue.TryDequeue(out var item))
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

            if (SetPlayer(guild) || _player.Queue.Count is 0)
                return "Nothing in queue!";

            LavaTrack oldTrack = _player.Track;
            await _player.SkipAsync();
            return $"**Skipped:** *{oldTrack.Title}*\n**Now playing:** *{_player.Track.Title}*";
        }

        public async Task<String> SetVolumeAsync(IGuild guild, ushort vol)
        {
            if (!SetPlayer(guild))
                return "No music is playing, why u wanna change volume?!?";

            if (vol > 150 || vol < 2)
                return "Invalid volume level!";


            await _player.UpdateVolumeAsync(vol);
            return $"Volume set to {vol}!";
        }

        public async Task<string> PauseOrResumeAsync(IGuild guild, string command)
        {

            if (!SetPlayer(guild))
                return "I'm not even playing!";

            if (_player.PlayerState == PlayerState.Paused)
            {
                await _player.ResumeAsync();
                return "Music resumed!";
            }
            if (command.Equals("resume"))
                return "Music is already playing!";

            await _player.PauseAsync();
            return "Paused music!";
        }

        public (Embed embed, string errmsg, bool err, bool isLastPage) Queue(IGuild guild, int pageNumber, bool edit)
        {
            if (!SetPlayer(guild))
                return (null, "Queue empty", true, false);
            
            --pageNumber;
            
            int count = _player.Queue.Count();
            int offset = pageNumber * 10;

            if (offset >= count)
                return (null, "Invalid page number", true, false);

            var embed = new EmbedBuilder
            {
                Title = "Queue:",
                Description = ""
            };

            embed.Description += "**Now playing:** [*" + _player.Track.Title + "*](" + _player.Track.Url + ")\n\n";

            for (int i = offset; (i < (offset + 10) && i < count); i++)
            {
                var track = _player.Queue.ElementAt(i);
                embed.Description += "**" + (i + 1) + "**: [" + track.Title + "](" + track.Url + ")   " + ((track.Duration.TotalHours >= 1 ? track.Duration.Hours + ":" : "") + track.Duration.Minutes.ToString("D2") + ":" + track.Duration.Seconds.ToString("D2")) + "\n";
            }

            bool isLastPage = (offset + 10) >= count;
            
            var resEmbed = embed.WithColor(new Color(0x000000))
                .WithFooter("page: " + (pageNumber + 1) + "/" + Math.Ceiling(count / 10.0))
                .Build();
            
            if (edit)
            {
                SetData(guild);
                _data.MusicQueueMessage.msg.ModifyAsync(msg => msg.Embed = resEmbed);
                _data.MusicQueueMessage = (_data.MusicQueueMessage.msg, pageNumber + 1);
                return (null, "", false, isLastPage);
            }
            else
            {
                return (resEmbed, "", false, isLastPage);
            }
        }
        
        public async Task AddQueueReactions(IMessage msg)
        {
            await msg.AddReactionAsync(new Emoji("\U000023EE"));
            await msg.AddReactionAsync(new Emoji("\U000025C0"));
            await msg.AddReactionAsync(new Emoji("\U000025B6"));
            await msg.AddReactionAsync(new Emoji("\U000023ED"));
        }
        
        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var chan = arg2 as IGuildChannel;
            if (chan is null) return;
            SetData(chan.Guild);
            
            if (arg1.HasValue && !Util.isMe(arg3.User.Value) && arg1.Value.Id == _data.MusicQueueMessage.msg.Id)
            {
                var pageNumber = -1;
                if (arg3.Emote.Equals(new Emoji("\U000023EE")))
                {
                    pageNumber = 1;
                }
                else if (arg3.Emote.Equals(new Emoji("\U000025C0")))
                {
                    pageNumber = Math.Max(_data.MusicQueueMessage.page - 1, 1);
                }
                else if (arg3.Emote.Equals(new Emoji("\U000025B6")))
                {
                    pageNumber = Math.Min(_data.MusicQueueMessage.page + 1, Convert.ToInt32(Math.Ceiling(_player.Queue.Count / 10.0)));
                }
                else if (arg3.Emote.Equals(new Emoji("\U000023ED")))
                {
                    SetPlayer(chan.Guild);
                    pageNumber = Convert.ToInt32(Math.Ceiling(_player.Queue.Count / 10.0));
                }

                if (pageNumber == -1) return;
                
                var (_, _,err, _) = Queue(chan.Guild, pageNumber, true);

                if (err) return;

                await _data.MusicQueueMessage.msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
            }
        }


        private static Task Log(LogMessage logMessage)
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

        // public static bool HasMusicPrivilege(SocketCommandContext context)
        // {
        //     return (Util.isAno(context) || true);
        // }
    }
}
