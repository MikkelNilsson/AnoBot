using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using Victoria.Enums;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using Discord.Commands;
using Victoria.Responses.Rest;
using SimpBot.Custom_Classes;
using System.Threading;
using EmbedIO.Utilities;
using SpotifyAPI.Web;
using SearchResponse = Victoria.Responses.Rest.SearchResponse;

namespace SimpBot.Services
{

    //Done TODO Create fast forward: !ff 15 -> skip 15 secs of the song
    //TODO move functionality: !move 15 1 -> moves song in position 15 to position 1
    //Done TODO queue functionality: !queue -> pretty print queue somehow.
    //TODO BASSBOOST funtionality: !bassboost 2 -> bassboost 2 out of 10
    //Done TODO queue a playlist: !playlist "link" -> queue every song in playlist (up to 25 songs)
    //TODO Soundcloud play with specific soundcloud link or !soundcloud
    //DONE TODO Automatic disconnect after 5 mins with nobody in the channel
    //DONE TODO spotify playlist retrieve songtitles and stuff to play from yt
    //TODO setting: clear queue on leave
    //Done TODO clear functionality -> clear queue.
    public class MusicService
    {
        private readonly LavaConfig _lavaConfig;
        private readonly LavaNode _lavaNode;
        private readonly DiscordSocketClient _client;
        private LavaPlayer _player;
        private ServerData _data;
        private readonly DataService _dataService;
        private readonly SpotifyClient _spotify;
        public MusicService(LavaConfig lavaConfig, LavaNode lavaNode, SpotifyClient spotify, DiscordSocketClient client, DataService dataService)
        {
            _lavaConfig = lavaConfig;
            _lavaNode = lavaNode;
            _client = client;
            _dataService = dataService;
            _spotify = spotify;
        }

        public Task InitializeAsync()
        {
            _client.Ready += ClientReady;
            _lavaNode.OnLog += Log;
            _lavaNode.OnTrackEnded += TrackEnded;
            _client.ReactionAdded += OnReactionAdded;
            //_client.UserVoiceStateUpdated += OnUserVoiceUpdate;
            return Task.CompletedTask;
        }



        public async Task ConnectAsync(SocketVoiceChannel voiceChannel, ITextChannel txtChannel)
            => await _lavaNode.JoinAsync(voiceChannel, txtChannel);

        // private Task OnUserVoiceUpdate(SocketUser user, SocketVoiceState state1, SocketVoiceState state2)
        // {
        //     if (!SetPlayer(state2.VoiceChannel.Guild))
        //         return Task.CompletedTask;

        //     return Task.CompletedTask;

        //     if (state2.VoiceChannel.Id == _player.VoiceChannel.Id)
        //     {
        //         //Create something here
        //     }
        // }

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

        private void startMusicTimer(IGuild guild)
        {
            if (!SetPlayer(guild)) return;
            SetData(guild);
            _data.startMusicTimer(_player, _lavaNode);
        }

        private void stopMusicTimer(IGuild guild)
        {
            SetData(guild);
            _data.stopMusicTimer();
        }

        public async Task<(string nowPlaying, bool isNowPlaying)> PlayAsync(string query, SocketCommandContext context)
        {
            SetPlayer(context.Guild);
            startMusicTimer(context.Guild);
            SearchResponse results;
            if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
            {
                Util.Log("MUSIC: Link Detected");

                if (query.ToLower().Contains("spotify.com"))
                {
                    return await PlaySpotify(query, context);
                }

                results = await _lavaNode.SearchAsync(query);
            }
            else
            {
                Util.Log($"MUSIC: Youtube search: {query}");
                results = await _lavaNode.SearchYouTubeAsync(query);

            }

            await _lavaNode.MoveChannelAsync(context.Channel);
            switch (results.LoadStatus)
            {
                case LoadStatus.NoMatches:
                    return ("No matches found. - " + query, false);
                case LoadStatus.LoadFailed:
                    Util.Log($"MUSIC: LOAD FAILED: {results.Exception.Message}");
                    return ("Load Failed!", false);
                case LoadStatus.SearchResult:
                case LoadStatus.TrackLoaded:
                    var track = results.Tracks.FirstOrDefault();
                    if (track is null) return ("Load Failed!", false);

                    stopMusicTimer(context.Guild);
                    
                    if (_player.PlayerState == PlayerState.Playing)
                    {
                        _player.Queue.Enqueue(track);
                        return ($"*{track.Title}* has been added to the queue.", false);
                    }
                    else
                    {
                        await _player.PlayAsync(track);
                        return ($"**Now playing:** *{track.Title}*\n{track.Url}", true);
                    }
                case LoadStatus.PlaylistLoaded:
                    bool isNowPlaying = false;
                    foreach (var t in results.Tracks)
                    {
                        if (_player.PlayerState != PlayerState.Playing)
                        {
                            await _player.PlayAsync(t);
                            await _player.ResumeAsync();
                            isNowPlaying = true;
                            continue;
                        }
                        _player.Queue.Enqueue(t);
                    }

                    await context.Channel.SendMessageAsync(
                        $"*{results.Playlist.Name}* loaded with {results.Tracks.Count} songs!");
                    stopMusicTimer(context.Guild);
                    return (isNowPlaying ? ($"**Now playing:** *{_player.Track.Title}*\n{_player.Track.Url}", true) : ("", false));
                default:
                    return ("Something happened, but I'm not gonna tell you what! HAHA!", false);
            }
        }

        public async Task<(string nowPlaying, bool isNowPlaying)> PlaySpotify(string spotifyLink, SocketCommandContext context)
        {
            SearchResponse results;
            if (spotifyLink.ToLower().Contains("track"))
            {
                Util.Log("MUSIC: Spotify song detected!");
                string id = spotifyLink.Split("track/")[1].Split("?")[0];
                FullTrack t = await _spotify.Tracks.Get(id);
                Util.Log("MUSIC: " + t.Name + " " + t.Artists[0].Name);
                results = await _lavaNode.SearchYouTubeAsync(t.Name + " " + t.Artists[0].Name);
                if (results.LoadStatus == LoadStatus.TrackLoaded || results.LoadStatus == LoadStatus.SearchResult)
                {
                    var track = results.Tracks.FirstOrDefault();
                    if (track is null) return ("Load Failed!", false);

                    stopMusicTimer(context.Guild);
                    
                    if (_player.PlayerState == PlayerState.Playing)
                    {
                        _player.Queue.Enqueue(track);
                        return ($"*{track.Title}* has been added to the queue.", false);
                    }
                    else
                    {
                        await _player.PlayAsync(track);
                        return ($"**Now playing:** *{track.Title}*\n{track.Url}", true);
                    }
                }
            }
            else if (spotifyLink.ToLower().Contains("playlist"))
            {
                context.Channel.SendMessageAsync("Loading spotify playlist, this might take a while to complete.");
                Util.Log("MUSIC: Spotify playlist detected!");
                string id = spotifyLink.Split("playlist/")[1].Split("?")[0];
                FullPlaylist fp = await _spotify.Playlists.Get(id);
                
                if (fp.Tracks == null) return ("Load playlist from spotify failed", false);
                
                var tracks = await _spotify.PaginateAll(fp.Tracks);
                string returnMessage = "";
                int trackcount = 0;
                bool isNowPlaying = false;
                for (int j = 0; j < tracks.Count; j++)
                {
                    FullTrack t = (FullTrack) (tracks[j].Track);
                    results = await _lavaNode.SearchYouTubeAsync(t.Name + " " + t.Artists[0].Name);
                    if (results.LoadStatus == LoadStatus.TrackLoaded || results.LoadStatus == LoadStatus.SearchResult)
                    {
                        stopMusicTimer(context.Guild);
                        trackcount++;
                        if (_player.PlayerState != PlayerState.Playing)
                        {
                            await _player.PlayAsync(results.Tracks.FirstOrDefault());
                            await _player.ResumeAsync();
                            isNowPlaying = true;
                        }
                        else
                        {
                            _player.Queue.Enqueue(results.Tracks.FirstOrDefault());
                        }
                    }
                    else
                    {
                        returnMessage += "\nFailed loading: " + t.Name + " " + t.Artists[0].Name + "";
                    }

                }

                await context.Channel.SendMessageAsync("Added " + trackcount + " to the queue" + returnMessage);

                return (isNowPlaying ? ($"**Now playing:** *{_player.Track.Title}*\n{_player.Track.Url}", true) : ("", false));
            }
            else if (spotifyLink.ToLower().Contains("album"))
            {
                Util.Log("MUSIC: Spotify playlist detected!");
                string id = spotifyLink.Split("playlist/")[1].Split("?")[0];
                FullAlbum fa = await _spotify.Albums.Get(id);

                var tracks = fa.Tracks.Items;
                string returnMessage = "";
                int trackcount = 0;
                bool isNowPlaying = false;
                for (int i = 0; i < tracks.Count; i++)
                {
                    var t = tracks[i];
                    
                    results = await _lavaNode.SearchYouTubeAsync(t.Name + " " + t.Artists[0].Name);
                    if (results.LoadStatus == LoadStatus.TrackLoaded || results.LoadStatus == LoadStatus.SearchResult)
                    {
                        trackcount++;
                        if (_player.PlayerState != PlayerState.Playing)
                        {
                            stopMusicTimer(context.Guild);
                            await _player.PlayAsync(results.Tracks.FirstOrDefault());
                            await _player.ResumeAsync();
                            isNowPlaying = true;
                        }
                        else
                        {
                            _player.Queue.Enqueue(results.Tracks.FirstOrDefault());
                        }
                    }
                    else
                    {
                        returnMessage += "\nFailed loading: " + t.Name + " " + t.Artists[0].Name + "";
                    }
                }
                
                await context.Channel.SendMessageAsync("Added " + trackcount + " to the queue" + returnMessage);

                return (isNowPlaying ? ($"**Now playing:** *{_player.Track.Title}*\n{_player.Track.Url}", true) : ("", false));
            }

            return ("Load from spotify failed!", false);
        }

        public async Task<string> FastForward(IGuild guild, int secs)
        {
            if (!SetPlayer(guild)) return "not playing anything man";
            var pos = _player.Track.Position + TimeSpan.FromSeconds(secs);
            if (pos > _player.Track.Duration)
            {
                return "Fast forward extended track length.";
            }
            await _player.SeekAsync(pos);

            return $"Fast forwarded to {(pos.TotalHours >= 1 ? pos.Hours + ":" : "") + pos.Minutes.ToString("D2") + ":" + pos.Seconds.ToString("D2")}.";
        }

        public string Shuffle(IGuild guild)
        {
            if (!SetPlayer(guild) || _player.Queue.Count <= 0)
            {
                return "";
            }
            _player.Queue.Shuffle();
            return "Queue Shuffled.";
        }

        public string Clear(IGuild guild)
        {
            if (!SetPlayer(guild))
                return "";

            _player.Queue.Clear();
            return "Queue cleared.";
        }

        public async Task<string> LeaveAsync(SocketVoiceChannel voiceChannel)
        {
            _lavaNode.GetPlayer(voiceChannel.Guild).Queue.Clear(); //clears queue on leave
            SetData(voiceChannel.Guild);
            _data.QueueLoop = false;
            _data.SingleLoop = false;
            _data.MusicQueueMessage = null;
            _data.NowPlayingMessage = null;
            stopMusicTimer(voiceChannel.Guild);

            await _lavaNode.LeaveAsync(voiceChannel);
            return "";
        }

        private async Task TrackEnded(TrackEndedEventArgs endEvent)
        {

            // if (_client.GetChannel(endEvent.Player.VoiceChannel.Id).Users.Count == 1)
            // {
            //     await LeaveAsync((SocketVoiceChannel)_client.GetChannel(endEvent.Player.VoiceChannel.Id));
            //     return;
            // }

            SetData(endEvent.Player.VoiceChannel.Guild);
            SetPlayer(endEvent.Player.VoiceChannel.Guild);
            if (_data.QueueLoop)
            {
                _player.Queue.Enqueue(endEvent.Track);
            }
            else if (_data.SingleLoop)
            {
                await _player.PlayAsync(endEvent.Track);
                if (_data.NowPlayingMessage != null)
                    await _data.NowPlayingMessage.DeleteAsync();
                _data.NowPlayingMessage = await endEvent.Player.TextChannel.SendMessageAsync(
                    $"**Now playing *(looping)*:** *{endEvent.Track.Title}*\n{endEvent.Track.Url}");
                return;
            }
            else if (!endEvent.Reason.ShouldPlayNext())
                return;

            if (endEvent.Player.PlayerState == PlayerState.Playing)
                return;
            
            if (!endEvent.Player.Queue.TryDequeue(out var item))
            {
                await endEvent.Player.TextChannel.SendMessageAsync("Queue empty.");
                _data.startMusicTimer(_player, _lavaNode);
                return;
            }

            await endEvent.Player.PlayAsync(item);
            if (_data.NowPlayingMessage != null)
                await _data.NowPlayingMessage.DeleteAsync();
            _data.NowPlayingMessage = await endEvent.Player.TextChannel.SendMessageAsync(
                $"**Now playing:** *{item.Title}*\n{item.Url}");
        }

        public async Task StopAsync(IGuild guild)
        {
            if (!SetPlayer(guild))
                return;
            
            startMusicTimer(guild);
            
            await _player.StopAsync();
        }

        public async Task<string> SkipAsync(IGuild guild)
        {
            if (!SetPlayer(guild) ||
                (_player.PlayerState != PlayerState.Playing &&
                _player.PlayerState != PlayerState.Paused))
                return "Nothing is playing!";

            LavaTrack oldTrack = _player.Track;
            Console.WriteLine(_player.Queue.Count.ToString());
            if (_player.Queue.Count < 1)
            {
                await _player.StopAsync();
                
                startMusicTimer(guild);
                
                return $"**Skipped:** *{oldTrack.Title}*\n";
            }
            await _player.SeekAsync(_player.Track.Duration - TimeSpan.FromMilliseconds(1));
            return $"**Skipped:** *{oldTrack.Title}*\n";
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
                stopMusicTimer(guild);
                await _player.ResumeAsync();
                return "Music resumed!";
            }
            if (command.Equals("resume"))
                return "Music is already playing!";

            await _player.PauseAsync();
            startMusicTimer(guild);
            return "Paused music!";
        }

        public (Embed embed, string errmsg, bool err, bool isLastPage) Queue(IGuild guild, int pageNumber, bool edit)
        {
            int count = _player.Queue.Count();
            if (!SetPlayer(guild) || count < 1)
                return (null, "Queue empty", true, false);

            --pageNumber;


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
                if (!_data.MusicQueueMessage.HasValue) return (null, "No queue message recorded", true, isLastPage);
                _data.MusicQueueMessage.Value.msg.ModifyAsync(msg => msg.Embed = resEmbed);
                _data.MusicQueueMessage = (_data.MusicQueueMessage.Value.msg, pageNumber + 1);
                return (null, "", false, isLastPage);
            }

            return (resEmbed, "", false, isLastPage);
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

            if (arg1.HasValue && HasMusicPrivilege((SocketGuildUser)arg1.Value.Author) && !Util.isWuBot(arg3.User.Value) && _data.MusicQueueMessage.HasValue && arg1.Value.Id == _data.MusicQueueMessage.Value.msg.Id)
            {
                var pageNumber = -1;
                if (arg3.Emote.Equals(new Emoji("\U000023EE")))
                {
                    pageNumber = 1;
                }
                else if (arg3.Emote.Equals(new Emoji("\U000025C0")))
                {
                    pageNumber = Math.Max(_data.MusicQueueMessage.Value.page - 1, 1);
                }
                else if (arg3.Emote.Equals(new Emoji("\U000025B6")))
                {
                    pageNumber = Math.Min(_data.MusicQueueMessage.Value.page + 1, Convert.ToInt32(Math.Ceiling(_player.Queue.Count / 10.0)));
                }
                else if (arg3.Emote.Equals(new Emoji("\U000023ED")))
                {
                    SetPlayer(chan.Guild);
                    pageNumber = Convert.ToInt32(Math.Ceiling(_player.Queue.Count / 10.0));
                }

                if (pageNumber == -1) return;

                var (_, _, err, _) = Queue(chan.Guild, pageNumber, true);

                if (err) return;

                await _data.MusicQueueMessage.Value.msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
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

        public string LoopSingle(SocketGuild contextGuild)
        {
            if (!SetPlayer(contextGuild))
            {
                return "Player is not playing";
            }
            SetData(contextGuild);

            _data.SingleLoop = !_data.SingleLoop;
            _data.QueueLoop = false;
            return (_data.SingleLoop ? "Looping the first song" : "Looping stopped");
        }

        public string LoopQueue(SocketGuild contextGuild)
        {
            if (!SetPlayer(contextGuild))
            {
                return "Player is not playing";
            }
            SetData(contextGuild);

            _data.QueueLoop = !_data.QueueLoop;
            _data.SingleLoop = false;
            return (_data.QueueLoop ? "Looping the playlist" : "Looping stopped");
        }

        public bool HasMusicPrivilege(IUser user)
        {
            SocketGuildUser guser = (SocketGuildUser)user;
            SetData(guser.Guild);
            return (/*Util.isAno(guser) ||*/ !_data.HasMusicRole() || guser.Roles.Contains(guser.Guild.GetRole(_data.MusicRole)));
        }

        public IEnumerable<LavaPlayer> GetPlayers()
        {
            return _lavaNode.Players;
        }
    }
}
