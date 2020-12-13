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
        private DataService _dataService;
        private BotSettingsService _settingsService;
        private WelcomeMessageService _wmService;

        public SimpBot(DiscordSocketClient client = null, CommandService cmdService = null,
            BotSettingsService settingsService = null, WelcomeMessageService wmService = null,
            DataService dataService = null)
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
            _dataService = dataService ?? new DataService();
            _settingsService = settingsService ?? new BotSettingsService(_dataService);
            _wmService = wmService ?? new WelcomeMessageService(_dataService);
        }

        public async Task InitalizeAsync()
        {
            string token;
            try
            {
                token = System.IO.File.ReadAllText(Environment.CurrentDirectory + _dataService.Divider+ "secret" + _dataService.Divider + "botToken.txt");
            } catch
            {
                Util.Log("Could not load token from file, trying env variables.");
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
            .AddSingleton(_dataService)
            .AddSingleton(_settingsService)
            .AddSingleton(_wmService)
            .BuildServiceProvider();


        private async Task UserJoined(SocketGuildUser usr)
        {
            if (_wmService.IsWelcomeMessageActive(usr.Guild))
            {
                var welcomeMessage = _wmService.GetWelcomeMessage(usr.Guild);
                var channel = (SocketTextChannel) usr.Guild.GetChannel(welcomeMessage.channel);
                channel.SendMessageAsync(welcomeMessage.message.Replace("¤name¤", "<@" + usr.Id + ">"));
            }
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
