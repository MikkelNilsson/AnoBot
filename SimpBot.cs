using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using System;
using SimpBot.Custom_Classes;
using Microsoft.Extensions.DependencyInjection;

namespace SimpBot
{
    class SimpBot
    {
        private DiscordSocketClient _client;
        private CommandService _cmdService;
        private IServiceProvider _services;
        private BotSettingsService _settingsService;

        public SimpBot(DiscordSocketClient client = null, CommandService cmdService = null, BotSettingsService settingsService = null)
        {
            _client = client ?? new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 50,
                LogLevel = LogSeverity.Verbose
            }) ;
            _cmdService = cmdService ?? new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async
            });
            _settingsService = settingsService ?? new BotSettingsService();
        }

        public async Task InitalizeAsync()
        {
            string token;
            try
            {
                token = System.IO.File.ReadAllText(Environment.CurrentDirectory + "/secret/botToken.txt");
            } catch
            {
                Console.WriteLine("Could not load token from file, trying env variables.");
                token = System.Environment.GetEnvironmentVariable("botToken");
            }
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.Log += Util.Log;
            _services = SetupServices();

            _client.UserJoined += UserJoined;

            var cmdHandler = new CommandHandler(_client, _cmdService, _services, _settingsService);
            await cmdHandler.InitializeAsync();

            await _client.SetStatusAsync(UserStatus.DoNotDisturb);
            await _client.SetActivityAsync(new BotActivity());


            await Task.Delay(-1);
        }

        private IServiceProvider SetupServices()
            => new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_cmdService)
            .AddSingleton(_settingsService)
            .BuildServiceProvider();


        private async Task UserJoined(SocketGuildUser usr)
        {
            try
            {
                await usr.AddRoleAsync(_settingsService.GetDefaultRole(usr.Guild));
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
