using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using System;
using SimpBot.Custom_Classes;
using Microsoft.Extensions.DependencyInjection;
using Victoria;
using SimpBot.Services;
using SpotifyAPI.Web;

namespace SimpBot
{
    class SimpBot
    {
        private DiscordSocketClient _client;
        private CommandService _cmdService;
        private IServiceProvider _services;
        private DataService _dataService;
        private SpotifyClient _spotify;

        public SimpBot(DiscordSocketClient client = null, CommandService cmdService = null, DataService dataService = null)
        {
            _client = client ?? new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 50,
                LogLevel = LogSeverity.Verbose
            });
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

            string spotifycredentials = "";
            try
            {
                spotifycredentials = await System.IO.File.ReadAllTextAsync(
                    Environment.CurrentDirectory + _dataService.Divider +
                    "secret" + _dataService.Divider + "spotifyCredentials.txt");
            }
            catch
            {
                Util.Log("Could not load Spotify credentials from file, trying env variables.");
                spotifycredentials = System.Environment.GetEnvironmentVariable("spotifyCredentials");
            }

            spotifycredentials = spotifycredentials.Split("\n")[0];
            
            
            await _client.LoginAsync(TokenType.Bot, token);
            token = "";
            await _client.StartAsync();
            _spotify = new SpotifyClient(SpotifyClientConfig.CreateDefault().WithAuthenticator(new ClientCredentialsAuthenticator(spotifycredentials.Split(" ")[0], spotifycredentials.Split(" ")[1])));
            spotifycredentials = "";
            Util.Log((_spotify != null ? "Spotify connected. Test: " + _spotify.Tracks.Get("3z06k8YF9CqX0CGFrlekOK").Result.Name : "Spotify Error!"));
            _client.Log += Util.Log;
            _services = SetupServices();

            _services.GetRequiredService<BotSettingsService>().Initialize();
            await _services.GetRequiredService<MusicService>().InitializeAsync();
            _services.GetRequiredService<WelcomeMessageService>().Initialize();

            var cmdHandler = new CommandHandler(_client, _cmdService, _services, _services.GetRequiredService<BotSettingsService>());
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
            .AddSingleton<LavaConfig>()
            .AddSingleton<MusicService>()
            .AddSingleton<BotSettingsService>()
            .AddSingleton<WelcomeMessageService>()
            .AddSingleton(_spotify)
            .BuildServiceProvider();

    }
}
