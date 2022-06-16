using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Globalization;

namespace Sosu.Services
{
    public class ConfigureWebhook : IHostedService
    {
        private readonly ILogger<ConfigureWebhook> _logger;
        private readonly IServiceProvider _services;
        private readonly BotConfiguration _botConfig;

        public ConfigureWebhook(ILogger<ConfigureWebhook> logger,
                                IServiceProvider serviceProvider,
                                IConfiguration configuration)
        {
            _logger = logger;
            _services = serviceProvider;
            _botConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
            //_botConfig.HostAddress = Variables.ngrokServer;
            //_botConfig.BotToken = "1328219094:AAEqOYjjONDQJzwpAjOXLn1zaLMvNXBszTo";

            Variables.commands = new Dictionary<string[], Func<ITelegramBotClient, Message, Task>>()
            {
                { new string[] {"/help"},   new Func<ITelegramBotClient, Message, Task>(HandleCommands.Help)},
                { new string[] {"/start"},   new Func<ITelegramBotClient, Message, Task>(HandleCommands.Start)},
                { new string[] {"/last", "/l"}, new Func<ITelegramBotClient, Message, Task>(HandleCommands.OsuLast)},
                { new string[] {"/lastscoresuka", "/lss"}, new Func<ITelegramBotClient, Message, Task>(HandleCommands.OsuLastScoreSuka)},
                { new string[] {"/set"},        new Func<ITelegramBotClient, Message, Task>(HandleCommands.OsuSet)},
                { new string[] {"/score", "/s"},new Func<ITelegramBotClient, Message, Task>(HandleCommands.OsuScore)},
                { new string[] {"/user"},       new Func<ITelegramBotClient, Message, Task>(HandleCommands.OsuUser)},
                { new string[] {"/userbest"},   new Func<ITelegramBotClient, Message, Task>(HandleCommands.OsuUserBest)},
                { new string[] {"/compare"},   new Func<ITelegramBotClient, Message, Task>(HandleCommands.OsuCompare)},
                { new string[] {"/chat_stats"},   new Func<ITelegramBotClient, Message, Task>(HandleCommands.OsuChatStats)},
                { new string[] {"/sendm"},   new Func<ITelegramBotClient, Message, Task>(HandleCommands.AdminSendm)},
                { new string[] {"/del"},   new Func<ITelegramBotClient, Message, Task>(HandleCommands.AdminDelete)},
                { new string[] {"/get"},   new Func<ITelegramBotClient, Message, Task>(HandleCommands.AdminGet)},
                { new string[] {"/settings"},   new Func<ITelegramBotClient, Message, Task>(HandleCommands.Settings)},
                { new string[] {"/de"},   new Func<ITelegramBotClient, Message, Task>(HandleCommands.DanbooruExplicit)},
                { new string[] {"/dn"},   new Func<ITelegramBotClient, Message, Task>(HandleCommands.DanbooruNonExplicit)},
                { new string[] {"/test"},   new Func<ITelegramBotClient, Message, Task>(HandleCommands.Test)},
            };
            Variables.callbacks = new Dictionary<string, Func<ITelegramBotClient, CallbackQuery, Task>>()
            {
                { "user", new Func<ITelegramBotClient, CallbackQuery, Task>(HandleCallbacks.OsuUser)},
                { "userbest", new Func<ITelegramBotClient, CallbackQuery, Task>(HandleCallbacks.OsuUserBest)},
                { "songprewiew", new Func<ITelegramBotClient, CallbackQuery, Task>(HandleCallbacks.OsuSongPrewiew)},
                { "language", new Func<ITelegramBotClient, CallbackQuery, Task>(HandleCallbacks.SettingsLanguage)}
            };
            Variables.osuApi = new Sosu.osu.V1.osuApi("");
            //Variables.osuApiV2 = new Osu.V2.osuApi();
            Variables.osuUsers = new List<Sosu.Types.osuUser>();
            Variables.chats = new List<Sosu.Types.Chat>();
            Variables.db = new Database("Host=;" +
                            "Port=5432;" +
                            "User ID=;" +
                            "Password=;" +
                            "Database=;" +
                            "Pooling=true;" +
                            "SSL Mode=Require;" +
                            "TrustServerCertificate=true;");
            //Variables.db = new Database("Host=localhost;Port=1337;Username=postgres;Password=5202340;Database=shiukkzbot");
            Variables.danbooruApi = new danbooruApi.danbooru.danbooru();

            Other.LoadData();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-US");

            using var scope = _services.CreateScope();
            var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
            Variables.bot = await botClient.GetMeAsync();
            // Configure custom endpoint per Telegram API recommendations:
            // https://core.telegram.org/bots/api#setwebhook
            // If you'd like to make sure that the Webhook request comes from Telegram, we recommend
            // using a secret path in the URL, e.g. https://www.example.com/<token>.
            // Since nobody else knows your bot's token, you can be pretty sure it's us.
            var webhookAddress = @$"{_botConfig.HostAddress}/bot/{_botConfig.BotToken}";
            Console.WriteLine("Setting webhook: " + webhookAddress);
            await botClient.SetWebhookAsync(
                url: webhookAddress,
                allowedUpdates: Array.Empty<UpdateType>(),
                cancellationToken: cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            ////Remove webhook upon app shutdown

            //using var scope = _services.CreateScope();
            //var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

            //Console.WriteLine("Removing webhook");
            //await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
        }
    }
}
