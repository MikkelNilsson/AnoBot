using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using System;
using SimpBot.Custom_Classes;
using Microsoft.Extensions.DependencyInjection;
using Victoria;
using SimpBot.Services;

namespace SimpBot
{
    class SimpBot
    {
        private DiscordSocketClient _client;
        private CommandService _cmdService;
        private IServiceProvider _services;
        private DataService _dataService;

        public SimpBot(DiscordSocketClient client = null, CommandService cmdService = null, DataService dataService = null)
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
        }

        public async Task InitalizeAsync()
        {
            string token = "";
            try
            {
                token = await System.IO.File.ReadAllTextAsync(Environment.CurrentDirectory + _dataService.Divider +
                                                              "secret" + _dataService.Divider + "botToken.txt");
            }
            catch
            {
                Util.Log("Could not load token from file, trying env variables.");
                token = System.Environment.GetEnvironmentVariable("botToken");
            }


            await _client.LoginAsync(TokenType.Bot, token);
            token = "";
            await _client.StartAsync();

            _client.Log += Util.Log;
            _services = SetupServices();
            
            _client.UserLeft += UserLeft;
            
            await _services.GetRequiredService<BotSettingsService>().InitializeAsync();
            await _services.GetRequiredService<MusicService>().InitializeAsync();
            await _services.GetRequiredService<WelcomeMessageService>().InitializeAsync();

            var cmdHandler = new CommandHandler(_client , _cmdService, _services, _services.GetRequiredService<BotSettingsService>());
            await cmdHandler.InitializeAsync();

            await _client.SetStatusAsync(UserStatus.Online);
            await _client.SetActivityAsync(new BotActivity());


            await Task.Delay(-1);
        }


        private IServiceProvider SetupServices()
            => new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_cmdService)
            .AddSingleton(_dataService)
            .AddSingleton<LeaveMessageService>()
            .AddSingleton<LavaNode>()
            .AddSingleton(new LavaConfig{SelfDeaf = false})
            .AddSingleton<MusicService>()
            .AddSingleton<BotSettingsService>()
            .AddSingleton<WelcomeMessageService>()
            .BuildServiceProvider();

        private Task UserLeft(SocketGuildUser arg)
        {
            if (_dataService.GetServerData(arg.Guild.Id).HasLeaveMessage())
            {
                var channel =
                    arg.Guild.GetChannel(_dataService.GetServerData(arg.Guild.Id).getLeaveChannel()) as
                        SocketTextChannel;
                channel?.SendMessageAsync(arg.Username + (!(arg.Nickname is null) ? " (" + arg.Nickname + ")" : "") + " left the server.");
            }
            return Task.CompletedTask;
        }
    }
}
