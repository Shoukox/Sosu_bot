using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Sosu.osu.V1.Types;

namespace Sosu.Services
{
    public class HandleUpdateService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<HandleUpdateService> _logger;

        public HandleUpdateService(ITelegramBotClient botClient, ILogger<HandleUpdateService> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        public async Task EchoAsync(Update update)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage),
                UpdateType.Unknown => Nothing(),
                UpdateType.InlineQuery => Nothing(),
                UpdateType.ChosenInlineResult => Nothing(),
                UpdateType.CallbackQuery => BotOnCallbackQuery(update.CallbackQuery),
                UpdateType.ChannelPost => Nothing(),
                UpdateType.EditedChannelPost => Nothing(),
                UpdateType.ShippingQuery => Nothing(),
                UpdateType.PreCheckoutQuery => Nothing(),
                UpdateType.Poll => Nothing(),
                UpdateType.PollAnswer => Nothing(),
                UpdateType.MyChatMember => Nothing(),
                UpdateType.ChatMember => Nothing(),
                _ => Nothing(),
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(exception);
            }
        }

        private async Task BotOnMessageReceived(Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            await CheckMessage(message);
            Console.WriteLine($"{message.Chat.Title} -=- {message.From.Username}: {message.Text}");

            message.Text = message.Text.Replace("@" + Variables.bot.Username, "");
            string[] splittedText = message.Text.Split(' ');
            if (message.Text[0] == '/')
            {
                var action = Variables.commands.FirstOrDefault(m => m.Key.Contains(splittedText[0]));
                if (action.Value != null)
                {
                    await action.Value(_botClient, message);
                    //Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");
                }
            }
            else
            {
                await HandleCommands.ProcessNotCommand(_botClient, message);
            }

        }
        private async Task BotOnCallbackQuery(CallbackQuery callback)
        {
            Console.WriteLine($"Receive callback data: {callback.Data}");

            string[] splittedCallback = callback.Data.Split(' ');
            if(callback.Data.Length >= 2)
            {
                var item = Variables.callbacks.FirstOrDefault(m => m.Key == splittedCallback[1]);
                if (item.Value != default)
                {
                    await item.Value(_botClient, callback);
                }
            }
        }
        private async Task CheckMessage(Message message)
        {
            bool isGroup = message.Chat.Id != message.From.Id;
            var chat = Variables.chats.FirstOrDefault(m => m.chat.Id == message.Chat.Id);
            if (chat == default)
            {
                Variables.chats.Add(new Sosu.Types.Chat(message.Chat, 0));
                await Variables.db.InsertOrUpdateOsuChatsTable(0, message.Chat.Id, 1, chat.members);
                chat = Variables.chats.Last();
            }
            else
            {
                chat.chat = message.Chat;
            }

            if (isGroup)
            {
                if (!chat.members.Contains(message.From.Id)) chat.members.Add(message.From.Id);
                await Variables.db.InsertOrUpdateOsuChatsTable(chat.lastBeatmap_id, chat.chat.Id, 0, chat.members);
            }
        }
        private Task Nothing()
        {
            return Task.CompletedTask;
        }

        public Task HandleErrorAsync(Exception exception)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogError(ErrorMessage);
            return Task.CompletedTask;
        }

    }
}
