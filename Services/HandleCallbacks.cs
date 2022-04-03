using OppaiSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Sosu.osu.V1.Types;
using System.IO;
using File = System.IO.File;
using Mods = Sosu.osu.V1.Enums.Mods;
using Telegram.Bot.Types.Enums;
using Sosu.Localization;

namespace Sosu.Services
{
    public class HandleCallbacks
    {
        public static async Task OsuUser(ITelegramBotClient bot, CallbackQuery callback)
        {
            var chat = Variables.chats.FirstOrDefault(m => m.chat.Id == callback.Message.Chat.Id);
            ILocalization language = Langs.GetLang(chat.language);

            string name = "";
            string[] splittedCallback = callback.Data.Split(' ');
            int mode = int.Parse(splittedCallback[2]);

            for (int i = 3; i <= splittedCallback.Length - 1; i++)
            {
                name += splittedCallback[i];
                if (i != splittedCallback.Length - 1) name += " ";
            }
            Sosu.osu.V1.Types.User osuUser = await Variables.osuApi.GetUserInfoByNameAsync(name, mode);

            string modeString = Other.Capitalize(Variables.osuApi.GetGameMode(int.Parse(splittedCallback[2])));
            var users1 = Variables.osuUsers.Where(m => m.osuName.ToLower() == osuUser.username().ToLower());
            string different = "";
            if (users1 != null && users1.Count() != 0 && modeString == "Standard")
            {
                different = $"(+{Math.Round(double.Parse(osuUser.pp_raw()) - users1.OrderByDescending(m => m.pp).ElementAt(0).pp, 2)})";
            }
            string textToSend = Langs.ReplaceEmpty(language.command_user(), new[] { $"{osuUser.profile_url()}", $"{osuUser.username()}", $"{osuUser.pp_rank()}", $"{osuUser.pp_country_rank()}", $"{osuUser.country()}", $"{osuUser.pp_raw():N2}", $"{different:N2}", $"{osuUser.accuracy():N2}", $"{osuUser.playcount()}", $"{osuUser.playtime_hours()}", $"{osuUser.count_rank_ssh()}", $"{osuUser.count_rank_sh()}", $"{osuUser.count_rank_ss()}", $"{osuUser.count_rank_s()}", $"{osuUser.count_rank_a()}" });
            var ik = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
                {
                    new InlineKeyboardButton[] {new InlineKeyboardButton {Text = "Standard", CallbackData = $"{callback.Message.Chat.Id} user 0 {name}"}, new InlineKeyboardButton { Text = "Taiko", CallbackData = $"{callback.Message.Chat.Id} user 1 {name}" }},
                    new InlineKeyboardButton[] {new InlineKeyboardButton {Text = "Catch", CallbackData = $"{callback.Message.Chat.Id} user 2 {name}" }, new InlineKeyboardButton { Text = "Mania", CallbackData = $"{callback.Message.Chat.Id} user 3 {name}" }}

                });
            await bot.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, textToSend, ParseMode.Html, replyMarkup: ik, disableWebPagePreview: true);
            await bot.AnswerCallbackQueryAsync(callback.Id);
        }
        public static async Task OsuUserBest(ITelegramBotClient bot, CallbackQuery callback)
        {
            var chat = Variables.chats.FirstOrDefault(m => m.chat.Id == callback.Message.Chat.Id);
            ILocalization language = Langs.GetLang(chat.language);

            string[] splittedCallback = callback.Data.Split(' ');

            if (splittedCallback[2] == "previous" && splittedCallback[3] == "0")
            {
                await bot.AnswerCallbackQueryAsync(callback.Id, language.error_noPreviousScores(), true);
                return;
            }

            Score[] scores = null;
            int step = int.Parse(splittedCallback[3]);
            string action = splittedCallback[2];
            int gameMode = int.Parse(splittedCallback[4]);
            string mode = Variables.osuApi.GetGameMode(gameMode);
            string name = "";
            for (int i = 5; i <= splittedCallback.Length - 1; i++)
                name += splittedCallback[i];

            if (action == "next")
            {
                scores = await Variables.osuApi.GetUserBestByNameAsync(name, 5 * (step + 2), gameMode);
                int takecount = scores.Length >= 5 ? 5 : scores.Length;
                scores = scores.TakeLast(takecount).ToArray();
                step += 1;
            }
            else if (action == "previous")
            {
                scores = await Variables.osuApi.GetUserBestByNameAsync(name, 5 * step, gameMode);
                int takecount = scores.Length >= 5 ? 5 : scores.Length;
                scores = scores.TakeLast(takecount).ToArray();
                step -= 1;
            }

            string textToSend = $"{name}({mode})\n\n";
            int index = step * 5;
            foreach (var item in scores)
            {
                Mods mods = (Mods)Variables.osuApi.CalculateModsMods(int.Parse(item.enabled_mods));
                double accuracy = (50 * double.Parse(item.count50) + 100 * double.Parse(item.count100) + 300 * double.Parse(item.count300)) / (300 * (double.Parse(item.countmiss) + double.Parse(item.count50) + double.Parse(item.count100) + double.Parse(item.count300))) * 100;
                Sosu.osu.V1.Types.Beatmap beatmap = await Variables.osuApi.GetBeatmapByBeatmapIdAsync(long.Parse(item.beatmap_id));
                beatmap.ParseHTML();
                textToSend += Langs.ReplaceEmpty(language.command_userbest(), new[] { $"{index + 1}", $"{item.rank}", $"{beatmap.beatmap_id}", $"{beatmap.title}", $"{beatmap.version}", $"{beatmap.GetApproved()}", $"{item.count300}", $"{item.count100}", $"{item.count50}", $"{item.countmiss}", $"{item.accuracy():N2}", $"{mods}", $"{item.maxcombo}", $"{beatmap.max_combo}", $"{double.Parse(item.pp)}" });
                index += 1;
            }
            var ik = new InlineKeyboardMarkup(
               new InlineKeyboardButton[] {new InlineKeyboardButton { Text = "Previous", CallbackData = $"{callback.Message.Chat.Id} userbest previous {step} {splittedCallback[4]} {name}" },  new InlineKeyboardButton { Text = "Next", CallbackData = $"{callback.Message.Chat.Id} userbest next {step} {splittedCallback[4]} {name}" } }
               );
            await bot.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, textToSend, Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: ik, disableWebPagePreview: true);
            await bot.AnswerCallbackQueryAsync(callback.Id);
        }
        public static async Task OsuSongPrewiew(ITelegramBotClient bot, CallbackQuery callback)
        {
            string[] splittedCallback = callback.Data.Split(' ');
            int beatmapset_id = int.Parse(splittedCallback[2]);

            byte[] data = null;
            using (WebClient wc = new WebClient())
            {
                data = wc.DownloadData($"https://b.ppy.sh/preview/{beatmapset_id}.mp3");
            }
            await File.WriteAllBytesAsync($"{beatmapset_id}.mp3", data);
            using (FileStream fs = File.Open($"{beatmapset_id}.mp3", FileMode.Open, FileAccess.Read))
            {
                await bot.SendAudioAsync(callback.Message.Chat.Id, new InputOnlineFile(fs));
            }
            await bot.AnswerCallbackQueryAsync(callback.Id);
        }
        public static async Task SettingsLanguage(ITelegramBotClient bot, CallbackQuery callback)
        {
            var chat = Variables.chats.FirstOrDefault(m => m.chat.Id == callback.Message.Chat.Id);

            string[] splittedCallback = callback.Data.Split(' ');
            string lang = splittedCallback[2];

            chat.language = lang;
            ILocalization language = Langs.GetLang(chat.language);
            string sendText = $"{language.settings_language_changedSuccessfully()}";
            await bot.AnswerCallbackQueryAsync(callback.Id, sendText, showAlert: true);
        }
    }
}
