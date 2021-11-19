﻿using SimpBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpBot.Modules
{
    public class Music : ModuleBase<SocketCommandContext>
    {
        private readonly MusicService _musicService;
        private readonly DataService _dataService;

        public Music(MusicService musicService, DataService dataService)
        {
            _musicService = musicService;
            _dataService = dataService;
        }

        [Command("Join")]
        private async Task Join()
        {
            if (!await Permission()) return;
            if (_musicService.NodeHasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm already connected to a voice channel!");
                return;
            }

            var user = Context.User as SocketGuildUser;
            if (user?.VoiceChannel is null)
            {
                await ReplyAsync("You need to connect to a voice channel!");
                return;
            }
            else
            {
                try
                {
                    await _musicService.ConnectAsync(user.VoiceChannel, Context.Channel as ITextChannel);
                    await ReplyAsync($"Now Connected to {user.VoiceChannel.Name}!");
                    Util.Log($"MUSIC: Joined {user.VoiceChannel.Name} on server {user.Guild.Name}");
                }
                catch (Exception e)
                {
                    await ReplyAsync(e.Message);
                }
            }
        }

        [Command("Leave")]
        private async Task Leave()                       //It can only disconnect if you're in the channel the bot is in.
        {
            if (!await Permission()) return;
            if (!_musicService.NodeHasPlayer(Context.Guild))
                return;

            await _musicService.LeaveAsync(Context.Guild.VoiceChannels.First());
            await ReplyAsync("Leaving voice channel!");

        }

        [Command("FastForward")]
        [Alias("FF")]
        private async Task FastForward([Remainder] string secs)
        {
            if (!await Permission()) return;
            if (!_musicService.NodeHasPlayer(Context.Guild))
            {
                await ReplyAsync("Bot is not connected to a voice channel.");
                return;
            }

            if (Int32.TryParse(secs.Trim(), out var sec))
            {
                if (sec > 0)
                {
                    await ReplyAsync(await _musicService.FastForward(Context.Guild, sec));
                }
                else
                {
                    await ReplyAsync("Negative amount: " + secs + " is not valid.");
                }

            }
            else
            {
                await ReplyAsync("\'*" + secs + "*\' is not a valid number.");
            }
        }

        [Command("Play")]
        [Alias("P")]
        private async Task Play([Remainder] string query)
        {
            if (!await Permission()) return;
            if (!_musicService.NodeHasPlayer(Context.Guild))
                await Join();
            Util.Log($"MUSIC: Trying to play {query}");
            var (nowPlaying, isNowPlaying) = await _musicService.PlayAsync(query, Context);

            if (isNowPlaying)
            {
                var data = _dataService.GetServerData(Context.Guild.Id);
                if (data.NowPlayingMessage != null)
                    await data.NowPlayingMessage.DeleteAsync();
                data.NowPlayingMessage = await ReplyAsync(nowPlaying);
            }
            else
                await ReplyAsync(nowPlaying);
        }

        [Command("Stop")]
        private async Task Stop()
        {
            if (!await Permission()) return;
            await _musicService.StopAsync(Context.Guild);
            await ReplyAsync("Music stopped!");
        }

        [Command("Skip", true)]
        private async Task Skip()
        {
            if (!await Permission()) return;
            var response = await _musicService.SkipAsync(Context.Guild);
            await ReplyAsync(response);
        }

        [Command("Loop")]
        private async Task LoopQueue()
        {
            if (!await Permission()) return;
            var res = _musicService.LoopQueue(Context.Guild);
            await ReplyAsync(res);
        }

        [Command("LoopSingle")]
        private async Task LoopSingle()
        {
            if (!await Permission()) return;
            var res = _musicService.LoopSingle(Context.Guild);
            await ReplyAsync(res);
        }

        [Command("Volume")]
        private async Task Volume(ushort vol)
        {
            if (!await Permission()) return;
            await ReplyAsync(await _musicService.SetVolumeAsync(Context.Guild, vol));
        }

        [Command("Pause")]
        private async Task Pause()
        {
            if (!await Permission()) return;
            await ReplyAsync(await _musicService.PauseOrResumeAsync(Context.Guild, "pause"));
        }

        [Command("Resume")]
        private async Task Resume()
        {
            if (!await Permission()) return;
            await ReplyAsync(await _musicService.PauseOrResumeAsync(Context.Guild, "resume"));
        }

        [Command("Shuffle")]
        private async Task Shuffle()
        {
            if (!await Permission()) return;
            await ReplyAsync(_musicService.Shuffle(Context.Guild));
        }

        [Command("Queue", true)]
        [Alias("Q")]
        private async Task Queue(string remainder)
        {
            var r = remainder.Trim();
            if (Int32.TryParse(r, out int page))
            {
                await QueueLogic(page);
            }
            else
            {
                await ReplyAsync("Page number is not a valid number");
            }
        }
        [Command("Queue")]
        [Alias("Q")]
        private async Task QueueNoArg()
        {
            await QueueLogic(1);
        }

        private async Task QueueLogic(int page)
        {
            if (!await Permission()) return;
            var data = _dataService.GetServerData(Context.Guild.Id);

            var res = _musicService.Queue(Context.Guild, page, false);
            if (res.err)
            {
                await ReplyAsync(res.errmsg);
                return;
            }

            var msg = await ReplyAsync(embed: res.embed);
            await _musicService.AddQueueReactions(msg);
            data.MusicQueueMessage = (msg, page);
        }

        [Command("Clear")]
        [Alias("C")]
        private async Task ClearQueue()
        {
            if (!await Permission()) return;
            await ReplyAsync(_musicService.Clear(Context.Guild));
        }

        private async Task<bool> Permission()
        {
            if (_musicService.HasMusicPrivilege(Context.User))
            {
                await ReplyAsync("Permission denied");
                return false;
            }
            return true;
        }
    }
}
