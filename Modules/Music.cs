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
        private MusicService _musicService;
        
        public Music(MusicService musicService)
        {
            _musicService = musicService;
        }

        [Command("Join")]
        public async Task Join()
        {
            if (!_musicService.HasMusicPrivilege(Context)) return;
            if (_musicService.NodeHasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm already connected to a voice channel!");
                return;
            }

            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
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
        public async Task Leave()                       //It can only disconnect if you're in the channel the bot is in.
        {
            if (!_musicService.NodeHasPlayer(Context.Guild))
                return;

            await _musicService.LeaveAsync(Context.Guild.VoiceChannels.First());
            await ReplyAsync($"Leaving voice channel!");

        }

        [Command("Play")]
        [Alias("P")]
        public async Task Play([Remainder]string query)
        {
            if (!_musicService.NodeHasPlayer(Context.Guild))
                await Join();
            Util.Log($"MUSIC: Trying to play {query}");
            var result = _musicService.PlayAsync(query, Context.Guild);
            await ReplyAsync(result.Result);
        }

        [Command("Stop")]
        public async Task Stop()
        {
            await _musicService.StopAsync(Context.Guild);
            await ReplyAsync("Music stopped!");
        }

        [Command("Skip")]
        [Alias("N")]
        public async Task Skip()
        {
            String response = (await _musicService.SkipAsync(Context.Guild));
            await ReplyAsync(response);
        }

        [Command("Volume")]
        public async Task Volume(ushort vol)
        {
            await ReplyAsync(await _musicService.SetVolumeAsync(Context.Guild, vol));
        }

        [Command("Pause")]
        public async Task Pause()
        {
            await ReplyAsync(await _musicService.PauseOrResumeAsync(Context.Guild, "pause"));
        }

        [Command("Resume")]
        public async Task Resume()
        {
            await ReplyAsync(await _musicService.PauseOrResumeAsync(Context.Guild, "resume"));
        }
    }
}