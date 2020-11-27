﻿using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpBot
{
    class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmdService;
        private readonly IServiceProvider _services;
        private readonly BotSettingsService _settingsService;

        public CommandHandler(DiscordSocketClient client, CommandService cmdService, IServiceProvider services, BotSettingsService settingsService)
        {
            _client = client;
            _cmdService = cmdService;
            _services = services;
            _settingsService = settingsService;
        }

        public async Task InitializeAsync()
        {
            await _cmdService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _cmdService.Log += LogAsync;
            _client.MessageReceived += HandleMessage;
        }

        private async Task HandleMessage(SocketMessage msg)
        { 
            var argPos = 0;
            if (msg.Author.IsBot) return;

            Console.WriteLine(msg.Author.ToString().Substring(0, msg.Author.ToString().Length - 5) + ": " + msg.Content);

            SocketUserMessage userMsg = msg as SocketUserMessage;
            if (userMsg is null) return;

            var context = new SocketCommandContext(_client, userMsg);

            if (context.Guild is null)
                return;

            if (!userMsg.HasStringPrefix(_settingsService.GetPrefix(context.Guild), ref argPos))
            {
                if (userMsg.HasStringPrefix("!help", ref argPos))
                    await _settingsService.HelpAsync(context);
                return;
            }
            IUserMessage demsg = context.Message;
            var result = await _cmdService.ExecuteAsync(context, argPos, _services);
        }

        private Task LogAsync(LogMessage logMessage)
        {
            Console.WriteLine(logMessage.Message);
            return Task.CompletedTask;
        }
    }
}
