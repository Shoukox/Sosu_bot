using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Sosu.osu.V1.Types;
using Sosu.osu.V1.Enums;
using Sosu.Localization;
using OppaiSharp;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Mods = Sosu.osu.V1.Enums.Mods;
using Beatmap = Sosu.osu.V1.Types.Beatmap;
using Telegram.Bot.Types.InputFiles;
using System.Diagnostics;

namespace Sosu.Services
{
    public class HandleCommands
    {
        public static async Task ProcessNotCommand(ITelegramBotClient bot, Message msg)
        {
            Message message = msg;
            if (message.Text.Contains("osu.ppy.sh"))
            {
                var user = Variables.osuUsers.FirstOrDefault(m => m.telegramId == msg.From.Id);
                var chat = Variables.chats.FirstOrDefault(m => m.chat.Id == msg.Chat.Id);
                ILocalization language = Langs.GetLang(chat.language);

                long beatmap_id = -1;
                string[] splittedLink = message.Text.Split("/");
                string last = splittedLink.Last();
                int index = last.IndexOf('+');
                string mods = "";
                OppaiSharp.Mods Mods = new OppaiSharp.Mods();
                if (index != -1)
                {
                    mods = last.Substring(index + 1, last.Length - index - 1);
                    for (int i = 0; i < mods.Length - 1; i += 2)
                    {
                        string mod = mods.Substring(i, 2).ToUpper();
                        Mods = (OppaiSharp.Mods)Variables.osuApi.getMod((Sosu.osu.V1.Enums.Mods)Mods, ref mod);
                    }
                    splittedLink = message.Text.Replace("+" + mods, "").Split('/');
                }
                else
                {
                    Mods = OppaiSharp.Mods.NoMod;
                }

                var array = splittedLink.Reverse().ToArray();
                bool isBeatmapSets = false;
                for (int i = 0; i <= array.Count() - 1; i++)
                {
                    try
                    {
                        beatmap_id = long.Parse(array[i]);
                        if (array[i + 1] == "beatmapsets")
                            isBeatmapSets = true;
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }


                if (beatmap_id < 0) return;

                Beatmap beatmap = null;
                if (!isBeatmapSets)
                    beatmap = await Variables.osuApi.GetBeatmapByBeatmapIdAsync(beatmap_id, (int)(Mods));
                else
                {
                    beatmap = await Variables.osuApi.GetBeatmapByBeatmapsetsIdAsync(beatmap_id, 0, (int)(Mods));
                    beatmap_id = int.Parse(beatmap.beatmap_id);
                }
                double[] acc100 = Other.ppCalc(beatmap_id, 100, Mods, 0, int.Parse(beatmap.max_combo));
                double[] acc98 = Other.ppCalc(beatmap_id, 98, Mods, 0, int.Parse(beatmap.max_combo));
                double[] acc96 = Other.ppCalc(beatmap_id, 96, Mods, 0, int.Parse(beatmap.max_combo));


                string duration = $"{beatmap.hit_length() / 60}:{(beatmap.hit_length() % 60):00}";
                string textToSend = Langs.ReplaceEmpty(language.send_mapInfo(), new[] { $"{beatmap.version}", $"{double.Parse(beatmap.difficultyrating):N2}", $"{duration}", $"{beatmap.creator}", $"{beatmap.GetApproved()}", $"{beatmap.diff_size()}", $"{beatmap.diff_approach()}", $"{beatmap.diff_overall()}", $"{beatmap.diff_drain()}", $"{beatmap.bpm()}", $"{acc100[0]}", $"{acc98[0]}", $"{acc96[0]}", $"{Mods}" });
                InputOnlineFile photo = new InputOnlineFile(new Uri($"https://assets.ppy.sh/beatmaps/{beatmap.beatmapset_id}/covers/card@2x.jpg"));
                await Variables.db.InsertOrUpdateOsuChatsTable(beatmap_id, message.Chat.Id, 0, chat.members);
                chat.lastBeatmap_id = beatmap_id;

                var ik = new InlineKeyboardMarkup(new InlineKeyboardButton("Song prewiew") { CallbackData = $"{chat.chat.Id} songprewiew {beatmap.beatmapset_id}" });
                try
                {
                    await bot.SendPhotoAsync(message.Chat.Id, photo, caption: textToSend, ParseMode.Html, replyMarkup: ik);
                }
                catch (Exception)
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, textToSend, ParseMode.Html, replyMarkup: ik, disableWebPagePreview: true);
                }
            }
        }
        public static async Task Start(ITelegramBotClient bot, Message msg)
        {
            var chat = Variables.chats.FirstOrDefault(m => m.chat.Id == msg.Chat.Id);
            Localization.ILocalization language = Langs.GetLang(chat.language);
            await bot.SendTextMessageAsync(msg.Chat.Id, language.command_start(), parseMode: ParseMode.Html);
        }
        public static async Task Help(ITelegramBotClient bot, Message msg)
        {
            var chat = Variables.chats.FirstOrDefault(m => m.chat.Id == msg.Chat.Id);
            Localization.ILocalization language = Langs.GetLang(chat.language);

            await bot.SendTextMessageAsync(msg.Chat.Id, language.command_help(), ParseMode.Html);
        }
        public static async Task OsuLast(ITelegramBotClient bot, Message msg)
        {
            Score[] scores = default;
            var user = Variables.osuUsers.FirstOrDefault(m => m.telegramId == msg.From.Id);
            var chat = Variables.chats.FirstOrDefault(m => m.chat.Id == msg.Chat.Id);
            ILocalization language = Langs.GetLang(chat.language);
            string osunickname = "";

            Message message = await bot.SendTextMessageAsync(msg.Chat.Id, language.waiting(), replyToMessageId: msg.MessageId, parseMode: ParseMode.Html);
            string[] splittedMessage = msg.Text.Split(" ");



            //await Variables.osuApiV2.GetRecentScoresByNameAsync(user.osuName, 1);

            if (splittedMessage.Length == 3)
            {
                scores = await Variables.osuApi.GetRecentScoresByNameAsync(splittedMessage[1], (splittedMessage.Length == 2) ? 1 : int.Parse(splittedMessage[2]));
                osunickname = splittedMessage[1];
            }
            if (splittedMessage.Length == 2)
            {
                if (splittedMessage[1].Length == 1)
                {
                    if (user == default)
                    {
                        await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, language.error_noUser(), ParseMode.Html);
                        return;
                    }
                    else
                    {
                        scores = await Variables.osuApi.GetRecentScoresByNameAsync(user.osuName, int.Parse(splittedMessage[1]));
                        osunickname = user.osuName;
                    }
                }
                else
                {
                    scores = await Variables.osuApi.GetRecentScoresByNameAsync(splittedMessage[1], 1);
                    osunickname = splittedMessage[1];
                }
            }
            if (splittedMessage.Length == 1)
            {
                if (user == default)
                {
                    await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, language.error_noUser(), ParseMode.Html);
                    return;
                }
                else
                {
                    scores = await Variables.osuApi.GetRecentScoresByNameAsync(user.osuName, 1);
                    osunickname = user.osuName;
                }
            }
            if (scores == default)
            {
                await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, language.error_noRecords(), ParseMode.Html);
                return;
            }
            string textToSend = $"<b>{osunickname}</b>\n\n";
            int i = 0;
            foreach (var item in scores)
            {
                Mods mods = (Mods)Variables.osuApi.CalculateModsMods(int.Parse(item.enabled_mods));

                Beatmap beatmap = await Variables.osuApi.GetBeatmapByBeatmapIdAsync(int.Parse(item.beatmap_id));
                if (i == 0) chat.lastBeatmap_id = long.Parse(beatmap.beatmap_id);
                beatmap.ParseHTML();

                //double[] curpp = new[]
                //{Other.ppCalc1(long.Parse(beatmap.beatmap_id), item.accuracy(), (OppaiSharp.Mods)mods, int.Parse(item.count100), int.Parse(item.count50), int.Parse(item.countmiss), int.Parse(item.maxcombo)),
                //Other.ppCalc1(long.Parse(beatmap.beatmap_id), item.accuracy(), (OppaiSharp.Mods)mods, int.Parse(item.count100), int.Parse(item.count50), 0, int.Parse(beatmap.max_combo))};
                double[] curpp = Other.ppCalc(long.Parse(beatmap.beatmap_id), item.accuracy(), (OppaiSharp.Mods)mods, int.Parse(item.countmiss), int.Parse(item.maxcombo));
                textToSend += Langs.ReplaceEmpty(language.command_last(), new[] { $"{i + 1}", $"{item.rank}", $"{item.beatmap_id}", $"{beatmap.title}", $"{beatmap.version}", $"{beatmap.GetApproved()}", $"{item.count300}", $"{item.count100}", $"{item.count50}", $"{item.countmiss}", $"{item.accuracy():N2}", $"{mods}", $"{item.maxcombo}", $"{beatmap.max_combo}", $"{curpp[0]}", $"{curpp[1]}", $"{DateTimeOffset.Parse(item.date).AddHours(5):dd.MM.yyyy HH:mm} UTC+05:00", $"{item.completion(beatmap.countobjects()):N1}" });
                i++;
            }
            await Variables.db.InsertOrUpdateOsuChatsTable(chat.lastBeatmap_id, chat.chat.Id, 0, chat.members);
            await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, textToSend, ParseMode.Html, disableWebPagePreview: true);
        }
        public static async Task OsuSet(ITelegramBotClient bot, Message msg)
        {
            var chat = Variables.chats.FirstOrDefault(m => m.chat.Id == msg.Chat.Id);
            ILocalization language = Langs.GetLang(chat.language);

            string name = string.Join(" ", msg.Text.Split(" ").Skip(1));

            var item = Variables.osuUsers.FirstOrDefault(m => m.telegramId == msg.From.Id);
            if (item == default)
            {
                Variables.osuUsers.Add(new Sosu.Types.osuUser(telegramId: msg.From.Id, osuName: name, pp: 0));
                item = Variables.osuUsers.Last();
                await Variables.db.InsertOrUpdateOsuUsersTable(item.telegramId, item.osuName, 1, item.pp);
                Console.WriteLine("added");
            }
            else
            {
                Console.WriteLine($"changed, last = {item.osuName}, new = {name}");
                item.telegramId = msg.From.Id;
                item.osuName = name;
                item.pp = 0;
                await Variables.db.InsertOrUpdateOsuUsersTable(item.telegramId, item.osuName, 0, item.pp);

            }
            string sendText = Langs.ReplaceEmpty(language.command_set(), new[] { $"{name}" });
            await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId, parseMode: ParseMode.Html);
        }
        public static async Task OsuScore(ITelegramBotClient bot, Message msg)
        {
            var user = Variables.osuUsers.FirstOrDefault(m => m.telegramId == msg.From.Id);
            Score[] scores = default;
            long beatmap_id = default;
            var chat = Variables.chats.First(m => m.chat.Id == msg.Chat.Id);
            string osunickname = "";
            ILocalization language = Langs.GetLang(chat.language);

            Message message = await bot.SendTextMessageAsync(msg.Chat.Id, language.waiting(), replyToMessageId: msg.MessageId, parseMode: ParseMode.Html);
            string[] splittedMessage = msg.Text.Split(" ");

            if (user == default)
            {
                await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, language.error_noUser(), ParseMode.Html);
                return;
            }
            if (splittedMessage.Length == 1)
            {
                if (msg.ReplyToMessage != null && msg.ReplyToMessage.Entities != null)
                {
                    MessageEntity? beatmapEntity = null;
                    string? beatmapText = null;
                    string[] bIdSplitter = new[] { "/beatmaps/", "/b/", "/beatmapsets/" };
                    foreach (var item in bIdSplitter)
                    {
                        beatmapEntity = msg.ReplyToMessage.Entities.FirstOrDefault(m => m.Type == MessageEntityType.TextLink && (m.Url.Contains(item)));
                        if (beatmapEntity != null) break;
                    }
                    foreach (var item in bIdSplitter)
                    {
                        beatmapText = (msg.ReplyToMessage.Text.Contains(item) ? msg.ReplyToMessage.Text : null);
                        if (beatmapText != null) break;
                    }
                    if (beatmapText != null)
                    {
                        beatmap_id = Other.GetBeatmapIdFromLink(Other.GetUrlFromText(msg.ReplyToMessage.Text));
                        scores = await Variables.osuApi.GetScoresOnMapByName(user.osuName, beatmap_id);
                    }
                    else if (beatmapEntity != null)
                    {
                        beatmap_id = long.Parse(beatmapEntity.Url.Split("/").Last());
                        scores = await Variables.osuApi.GetScoresOnMapByName(user.osuName, beatmap_id);
                    }
                }
                if (scores == default || beatmap_id == default)
                {
                    beatmap_id = Variables.chats.First(m => m.chat.Id == msg.Chat.Id).lastBeatmap_id;
                    scores = await Variables.osuApi.GetScoresOnMapByName(user.osuName, beatmap_id);
                }
                osunickname = user.osuName;
            }
            else if (splittedMessage.Length == 2)
            {
                if (splittedMessage[1].Contains("osu.ppy.sh"))
                {
                    string[] splittedLink = splittedMessage[1].Split("/");
                    foreach (var item in splittedLink.Reverse())
                    {
                        try
                        {
                            beatmap_id = long.Parse(item);
                            break;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                    if (beatmap_id != default)
                    {
                        scores = await Variables.osuApi.GetScoresOnMapByName(user.osuName, beatmap_id);
                    }
                    osunickname = user.osuName;
                }
                else if (msg.ReplyToMessage != null && msg.ReplyToMessage.Entities != null)
                {
                    MessageEntity? beatmapEntity = null;
                    string? beatmapText = null;
                    string[] bIdSplitter = new[] { "/beatmaps/", "/b/", "/beatmapsets/" };
                    foreach (var item in bIdSplitter)
                    {
                        beatmapEntity = msg.ReplyToMessage.Entities.FirstOrDefault(m => m.Type == MessageEntityType.TextLink && (m.Url.Contains(item)));
                        if (beatmapEntity != null) break;
                    }
                    foreach (var item in bIdSplitter)
                    {
                        beatmapText = (msg.ReplyToMessage.Text.Contains(item) ? msg.ReplyToMessage.Text : null);
                        if (beatmapText != null) break;
                    }
                    if (beatmapText != null)
                    {
                        beatmap_id = Other.GetBeatmapIdFromLink(Other.GetUrlFromText(msg.ReplyToMessage.Text));
                        scores = await Variables.osuApi.GetScoresOnMapByName(splittedMessage[1], beatmap_id);
                    }
                    else if (beatmapEntity != null)
                    {
                        beatmap_id = long.Parse(beatmapEntity.Url.Split("/").Last());
                        scores = await Variables.osuApi.GetScoresOnMapByName(splittedMessage[1], beatmap_id);
                    }
                    osunickname = splittedMessage[1];
                }
            }
            if (scores == default)
            {
                await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, language.error_noRecords(), ParseMode.Html);
                return;
            }
            string textToSend = $"<b>{osunickname}</b>\n\n";
            int i = 0;
            foreach (var item in scores)
            {
                Mods mods = (Mods)Variables.osuApi.CalculateModsMods(int.Parse(item.enabled_mods));

                double accuracy = (50 * double.Parse(item.count50) + 100 * double.Parse(item.count100) + 300 * double.Parse(item.count300)) / (300 * (double.Parse(item.countmiss) + double.Parse(item.count50) + double.Parse(item.count100) + double.Parse(item.count300))) * 100;
                Beatmap beatmap = await Variables.osuApi.GetBeatmapByBeatmapIdAsync(beatmap_id);
                if (i == 0)
                {
                    chat.lastBeatmap_id = beatmap_id;
                }

                double curpp = -1;
                if (item.pp != null)
                    curpp = double.Parse(item.pp);
                else
                    curpp = Other.ppCalc(beatmap_id, accuracy, (OppaiSharp.Mods)mods, int.Parse(item.countmiss), int.Parse(item.maxcombo))[0];
                beatmap.ParseHTML();
                textToSend += Langs.ReplaceEmpty(language.command_score(), new[] { $"{item.rank}", $"{beatmap_id}", $"{beatmap.title}", $"{beatmap.version}", $"{beatmap.GetApproved()}", $"{item.count300}", $"{item.count100}", $"{item.count50}", $"{item.countmiss}", $"{item.accuracy():N2}", $"{mods}", $"{item.maxcombo}", $"{beatmap.max_combo}", $"{curpp:N2}", $"{DateTimeOffset.Parse(item.date).AddHours(5):dd.MM.yyyy HH:mm zzz}" });
                i++;
            }
            await Variables.db.InsertOrUpdateOsuChatsTable(chat.lastBeatmap_id, chat.chat.Id, 0, chat.members);
            await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, textToSend, ParseMode.Html, disableWebPagePreview: true);
        }
        public static async Task OsuUser(ITelegramBotClient bot, Message msg)
        {
            var user = Variables.osuUsers.FirstOrDefault(m => m.telegramId == msg.From.Id);
            var chat = Variables.chats.FirstOrDefault(m => m.chat.Id == msg.Chat.Id);
            Sosu.osu.V1.Types.User osuUser = null;
            ILocalization language = Langs.GetLang(chat.language);

            Message message = await bot.SendTextMessageAsync(msg.Chat.Id, language.waiting(), replyToMessageId: msg.MessageId);
            string[] splittedMessage = msg.Text.Split(' ');
            if (splittedMessage.Length == 1)
            {
                if (user != default)
                {
                    osuUser = await Variables.osuApi.GetUserInfoByNameAsync(user.osuName);
                    user.osuName = osuUser.username();
                }
                else
                {
                    await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, language.error_noUser(), ParseMode.Html);
                    return;
                }
            }
            else if (splittedMessage.Length >= 2)
            {
                osuUser = await Variables.osuApi.GetUserInfoByNameAsync(string.Join(" ", splittedMessage.Skip(1)));
            }

            if (osuUser == null)
            {
                await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, language.error_userNotFound(), ParseMode.Html);
                return;
            }
            var users1 = Variables.osuUsers.Where(m => m.osuName.ToLower() == osuUser.username().ToLower());

            string different = "";
            if (users1 != null && users1.Count() != 0)
            {
                different = $"(+{Math.Round(double.Parse(osuUser.pp_raw()) - users1.OrderByDescending(m => m.pp).ElementAt(0).pp, 2)})";
            }
            string textToSend = Langs.ReplaceEmpty(language.command_user(), new[] { "Standard", $"{osuUser.profile_url()}", $"{osuUser.username()}", $"{osuUser.pp_rank()}", $"{osuUser.pp_country_rank()}", $"{osuUser.country()}", $"{osuUser.pp_raw():N2}", $"{different:N2}", $"{double.Parse(osuUser.accuracy()):N2}", $"{osuUser.playcount()}", $"{osuUser.playtime_hours()}", $"{osuUser.count_rank_ssh()}", $"{osuUser.count_rank_sh()}", $"{osuUser.count_rank_ss()}", $"{osuUser.count_rank_s()}", $"{osuUser.count_rank_a()}" });
            var ik = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
                {
                    new InlineKeyboardButton[] {new InlineKeyboardButton("Standard") {CallbackData = $"{msg.Chat.Id} user 0 {osuUser.username()}"}, new InlineKeyboardButton("Taiko") {CallbackData = $"{msg.Chat.Id} user 1 {osuUser.username()}" }},
                    new InlineKeyboardButton[] {new InlineKeyboardButton("Catch") {CallbackData = $"{msg.Chat.Id} user 2 {osuUser.username()}" }, new InlineKeyboardButton("Mania") { CallbackData = $"{msg.Chat.Id} user 3 {osuUser.username()}" }}

                });

            await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, textToSend, ParseMode.Html, replyMarkup: ik, disableWebPagePreview: true);
            if (users1 != null && users1.Count() != 0)
            {
                foreach (var item in users1)
                {
                    item.pp = double.Parse(osuUser.pp_raw());
                    await Variables.db.InsertOrUpdateOsuUsersTable(item.telegramId, item.osuName, 0, item.pp);
                }
            }
        }
        public static async Task OsuUserBest(ITelegramBotClient bot, Message msg)
        {
            var user = Variables.osuUsers.FirstOrDefault(m => m.telegramId == msg.From.Id);
            var chat = Variables.chats.FirstOrDefault(m => m.chat.Id == msg.Chat.Id);
            ILocalization language = Langs.GetLang(chat.language);

            Message message = await bot.SendTextMessageAsync(msg.Chat.Id, language.waiting(), replyToMessageId: msg.MessageId);
            string[] splittedMessage = msg.Text.Split(' ');
            Score[] scores = null;
            int gameMode = 0;
            Sosu.osu.V1.Types.User osuUser = null;
            if (splittedMessage.Length == 1)
            {
                if (user != default)
                {
                    scores = await Variables.osuApi.GetUserBestByNameAsync(user.osuName, 5);
                    osuUser = await Variables.osuApi.GetUserInfoByNameAsync(user.osuName, gameMode);
                }
                else
                {
                    await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, language.error_noUser(), ParseMode.Html);
                    return;
                }
            }
            else if (splittedMessage.Length == 2)
            {
                scores = await Variables.osuApi.GetUserBestByNameAsync(splittedMessage[1], 5);
                osuUser = await Variables.osuApi.GetUserInfoByNameAsync(splittedMessage[1], gameMode);
            }
            else if (splittedMessage.Length == 3)
            {
                gameMode = int.Parse(splittedMessage[2]);
                scores = await Variables.osuApi.GetUserBestByNameAsync(splittedMessage[1], 5, gameMode);
                osuUser = await Variables.osuApi.GetUserInfoByNameAsync(splittedMessage[1], gameMode);
            }

            if (scores == null)
            {
                await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, language.error_noRecords(), ParseMode.Html);
                return;
            }
            string mode = Variables.osuApi.GetGameMode(gameMode);
            string textToSend = $"{osuUser.username()}({mode})\n\n";

            int i = 0;
            foreach (var item in scores)
            {
                Mods mods = (Mods)Variables.osuApi.CalculateModsMods(int.Parse(item.enabled_mods));
                Beatmap beatmap = await Variables.osuApi.GetBeatmapByBeatmapIdAsync(long.Parse(item.beatmap_id));
                beatmap.ParseHTML();
                textToSend += Langs.ReplaceEmpty(language.command_userbest(), new[] { $"{i + 1}", $"{item.rank}", $"{beatmap.beatmap_id}", $"{beatmap.title}", $"{beatmap.version}", $"{beatmap.GetApproved()}", $"{item.count300}", $"{item.count100}", $"{item.count50}", $"{item.countmiss}", $"{item.accuracy():N2}", $"{mods}", $"{item.maxcombo}", $"{beatmap.max_combo}", $"{double.Parse(item.pp)}" });
                i += 1;
            }
            var ik = new InlineKeyboardMarkup(
                new InlineKeyboardButton[] { new InlineKeyboardButton("Previous") { CallbackData = $"{chat.chat.Id} userbest previous 0 {gameMode} {osuUser.username()}" }, new InlineKeyboardButton("Next") { CallbackData = $"{chat.chat.Id} userbest next 0 {gameMode} {osuUser.username()}" } }
                );
            await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, textToSend, ParseMode.Html, replyMarkup: ik, disableWebPagePreview: true);
        }
        public static async Task OsuCompare(ITelegramBotClient bot, Message msg)
        {
            var user = Variables.osuUsers.FirstOrDefault(m => m.telegramId == msg.From.Id);
            var chat = Variables.chats.FirstOrDefault(m => m.chat.Id == msg.Chat.Id);
            ILocalization language = Langs.GetLang(chat.language);

            Message message = await bot.SendTextMessageAsync(msg.Chat.Id, language.waiting(), replyToMessageId: msg.MessageId);
            string[] splittedMessage = msg.Text.Split(' ');

            if (splittedMessage.Length < 3)
            {
                await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, language.error_argsLength(), ParseMode.Html);
                return;
            }

            Sosu.osu.V1.Types.User user1 = null;
            Sosu.osu.V1.Types.User user2 = null;
            int gamemode = splittedMessage.Length == 3 ? 0 : int.Parse(splittedMessage[3]);
            user1 = await Variables.osuApi.GetUserInfoByNameAsync(splittedMessage[1], gamemode);
            user2 = await Variables.osuApi.GetUserInfoByNameAsync(splittedMessage[2], gamemode);

            if (user1 == null || user2 == null)
            {
                await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, language.error_userNotFound(), ParseMode.Html);
                return;
            }

            string acc1 = $"{double.Parse(user1.accuracy()):N2}";
            string acc2 = $"{double.Parse(user2.accuracy()):N2}";
            int max = Other.GetMax(user1.pp_country_rank().Length + "# UZ".Length, user1.pp_rank().Length + "#".Length, user1.pp_raw().ToString().Length + "pp".Length, acc1.Length + "%".Length, $"{user1.playtime_hours()}h".Length, user1.username().Length);

            string textToSend = Langs.ReplaceEmpty(language.command_compare(), new[] { $"{Variables.osuApi.GetGameMode(gamemode)}", $"{user1.username().PadRight(max)}", $"{user2.username()}", $"{("#" + user1.pp_rank()).PadRight(max)}", $"{user2.pp_rank()}", $"{("#" + user1.pp_country_rank() + " " + user1.country()).PadRight(max)}", $"{(user2.pp_country_rank() + " " + user2.country())}", $"{(user1.pp_raw() + "pp").PadRight(max)}", $"{user2.pp_raw()}", $"{(acc1 + "%").PadRight(max)}", $"{acc2}%", $"{(user1.playtime_hours().ToString() + "h").PadRight(max)}", $"{user2.playtime_hours()}" });
            await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, textToSend, ParseMode.Html, disableWebPagePreview: true);
        }
        public static async Task OsuChatStats(ITelegramBotClient bot, Message msg)
        {
            var user = Variables.osuUsers.FirstOrDefault(m => m.telegramId == msg.From.Id);
            var chat = Variables.chats.FirstOrDefault(m => m.chat.Id == msg.Chat.Id);
            ILocalization language = Langs.GetLang(chat.language);

            List<Sosu.Types.osuUser> chatMembers = new();

            Message message = await bot.SendTextMessageAsync(msg.Chat.Id, language.waiting(), replyToMessageId: msg.MessageId);
            string sendText = language.command_chatstats_title();
            foreach (var item in chat.members)
            {
                var curUser = Variables.osuUsers.FirstOrDefault(m => m.telegramId == item);
                if (curUser != null)
                {
                    chatMembers.Add(curUser);
                }
            }
            var sortedChatMembers = chatMembers.OrderByDescending(m => m.pp).ToList();

            int i = 1;
            foreach (var item in sortedChatMembers)
            {
                if (i == 11) break;
                sendText += Langs.ReplaceEmpty(language.command_chatstats_row(), new[] { $"{i}", $"{item.osuName}", $"{item.pp}" });
                i += 1;
            }

            sendText += language.command_chatstats_end();
            await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, sendText, ParseMode.Html, disableWebPagePreview: true);
        }
        public static async Task OsuLastScoreSuka(ITelegramBotClient bot, Message msg)
        {
            Score[] scores = default;
            var user = Variables.osuUsers.FirstOrDefault(m => m.telegramId == msg.From.Id);
            var chat = Variables.chats.FirstOrDefault(m => m.chat.Id == msg.Chat.Id);
            ILocalization language = Langs.GetLang(chat.language);
            string osunickname = "";

            Message message = await bot.SendTextMessageAsync(msg.Chat.Id, language.waiting(), replyToMessageId: msg.MessageId);
            string[] splittedMessage = msg.Text.Split(" ");

            if (splittedMessage.Length == 3)
            {
                scores = await Variables.osuApi.GetRecentScoresByNameAsync(splittedMessage[1], (splittedMessage.Length == 2) ? 1 : int.Parse(splittedMessage[2]));
                osunickname = splittedMessage[1];
            }
            if (splittedMessage.Length == 2)
            {
                if (splittedMessage[1].Length == 1)
                {
                    if (user == default)
                    {
                        await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, language.error_noUser(), ParseMode.Html);
                        return;
                    }
                    else
                    {
                        scores = await Variables.osuApi.GetRecentScoresByNameAsync(user.osuName, int.Parse(splittedMessage[1]));
                        osunickname = user.osuName;
                    }
                }
                else
                {
                    scores = await Variables.osuApi.GetRecentScoresByNameAsync(splittedMessage[1], 1);
                    osunickname = splittedMessage[1];
                }
            }
            if (splittedMessage.Length == 1)
            {
                if (user == default)
                {
                    await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, language.error_noUser(), ParseMode.Html);
                    return;
                }
                else
                {
                    scores = await Variables.osuApi.GetRecentScoresByNameAsync(user.osuName, 1);
                    osunickname = user.osuName;
                }
            }
            if (scores == default)
            {
                await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, language.error_noRecords(), ParseMode.Html);
                return;
            }
            double usersPP = double.Parse((await Variables.osuApi.GetUserInfoByNameAsync(osunickname)).pp_raw());
            string textToSend = $"<b>{osunickname}</b>\n\n";
            int i = 0;
            foreach (var item in scores)
            {
                Mods mods = (Mods)Variables.osuApi.CalculateModsMods(int.Parse(item.enabled_mods));
                Beatmap beatmap = await Variables.osuApi.GetBeatmapByBeatmapIdAsync(int.Parse(item.beatmap_id));
                if (i == 0) chat.lastBeatmap_id = long.Parse(beatmap.beatmap_id);
                beatmap.ParseHTML();
                double[] curpp = Other.ppCalc(long.Parse(beatmap.beatmap_id), item.accuracy(), (OppaiSharp.Mods)mods, int.Parse(item.countmiss), int.Parse(item.maxcombo));

                //conditions
                string rankStr = "";
                string commentsStr = "";
                rankStr = item.rank switch
                {
                    "F" => language.command_lastScoreSuka_mapFailed(),
                    "D" => language.command_lastScoreSuka_rankD(),
                    "C" => language.command_lastScoreSuka_rankC(),
                    "B" => language.command_lastScoreSuka_rankB(),
                    "A" => language.command_lastScoreSuka_rankA(),
                    "S" => language.command_lastScoreSuka_rankS(),
                    _ => "Ëó÷øèé"
                };

                if (beatmap.total_length() >= 300) commentsStr += $"{language.command_lastScoreSuka_longDuration()}\n";
                if (beatmap.total_length() < 60) commentsStr += $"{language.command_lastScoreSuka_lowDuration()}\n";
                if (double.Parse(item.maxcombo) / double.Parse(beatmap.max_combo) >= 0.9 && int.Parse(item.countmiss) >= 1) commentsStr += $"{language.command_lastScoreSuka_shitMisses()}\n";
                if (int.Parse(beatmap.count_slider) >= int.Parse(beatmap.count_normal)) commentsStr += $"{language.command_lastScoreSuka_manySliders()}\n";
                if (beatmap.title.Split(" ").Length >= 5) commentsStr += $"{language.command_lastScoreSuka_mapTitleTooLong()}\n";


                if (usersPP < 3000)
                {
                    if (double.Parse(beatmap.difficultyrating) <= 2) commentsStr += $"{language.command_lastScoreSuka_tooEasyMapForPlayer()}";
                    else if (double.Parse(beatmap.difficultyrating) >= 6) commentsStr += $"{language.command_lastScoreSuka_tooHardMapForPlayer()}";
                }
                else if (usersPP >= 3000 && usersPP <= 5000)
                {
                    if (double.Parse(beatmap.difficultyrating) <= 3) commentsStr += $"{language.command_lastScoreSuka_tooEasyMapForPlayer()}";
                    else if (double.Parse(beatmap.difficultyrating) >= 7) commentsStr += $"{language.command_lastScoreSuka_tooHardMapForPlayer()}";
                }
                else if (usersPP >= 5000 && usersPP <= 9000)
                {
                    if (double.Parse(beatmap.difficultyrating) <= 4) commentsStr += $"{language.command_lastScoreSuka_tooEasyMapForPlayer()}";
                    else if (double.Parse(beatmap.difficultyrating) >= 8) commentsStr += $"{language.command_lastScoreSuka_tooHardMapForPlayer()}";
                }
                else if (usersPP >= 9000)
                {
                    if (double.Parse(beatmap.difficultyrating) <= 5) commentsStr += $"{language.command_lastScoreSuka_tooEasyMapForPlayer()}";
                    else if (double.Parse(beatmap.difficultyrating) >= 8) commentsStr += $"{language.command_lastScoreSuka_tooHardMapForPlayer()}";
                }
                textToSend += Langs.ReplaceEmpty(language.command_lastScoreSuka(), new[] { $"<a href=\"https://osu.ppy.sh/beatmaps/{beatmap.beatmap_id}\">{beatmap.title} [{beatmap.version}]</a>", $"{DateTimeOffset.Parse(item.date):dd.MM.yyyy HH:mm zzz}", $"{item.rank}", $"{rankStr}", $"{commentsStr}" });
                i++;
            }
            await Variables.db.InsertOrUpdateOsuChatsTable(chat.lastBeatmap_id, chat.chat.Id, 0, chat.members);
            await bot.EditMessageTextAsync(msg.Chat.Id, message.MessageId, textToSend, ParseMode.Html, disableWebPagePreview: true);
        }

        public static async Task AdminSendm(ITelegramBotClient bot, Message msg)
        {
            if (Variables.WHITELIST.Contains(msg.From.Id))
            {
                await bot.SendTextMessageAsync(msg.Chat.Id, string.Join(" ", msg.Text.Split(" ").Skip(1)), ParseMode.Html, replyToMessageId: msg.ReplyToMessage.MessageId);
                await bot.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
            }
        }
        public static async Task AdminDelete(ITelegramBotClient bot, Message msg)
        {
            if (Variables.WHITELIST.Contains(msg.From.Id))
            {
                if (msg.ReplyToMessage != null)
                {
                    await bot.DeleteMessageAsync(msg.ReplyToMessage.Chat.Id, msg.ReplyToMessage.MessageId);
                }
            }
        }
        public static async Task AdminGet(ITelegramBotClient bot, Message msg)
        {
            if (Variables.WHITELIST.Contains(msg.From.Id))
            {
                if (msg.ReplyToMessage != null)
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(msg.ReplyToMessage, Newtonsoft.Json.Formatting.Indented);
                    await bot.SendTextMessageAsync(msg.Chat.Id, json, replyToMessageId: msg.MessageId);
                }
            }
        }

        public static async Task Settings(ITelegramBotClient bot, Message msg)
        {
            var user = Variables.osuUsers.FirstOrDefault(m => m.telegramId == msg.From.Id);
            var chat = Variables.chats.FirstOrDefault(m => m.chat.Id == msg.Chat.Id);

            ILocalization language = Langs.GetLang(chat.language);

            InlineKeyboardMarkup ik = new InlineKeyboardMarkup(
                new InlineKeyboardButton[][]
                {
                    new InlineKeyboardButton[] { new InlineKeyboardButton(language.settings_language_ru()) { CallbackData = $"{chat.chat.Id} language ru"} },
                    new InlineKeyboardButton[] { new InlineKeyboardButton(language.settings_language_en()) { CallbackData = $"{chat.chat.Id} language en"} }
                });

            await bot.SendTextMessageAsync(msg.Chat.Id, language.settings(), ParseMode.Html, replyMarkup: ik);
        }

        public static async Task DanbooruExplicit(ITelegramBotClient bot, Message msg)
        {
            string[] splittedMessage = msg.Text.Split(" ");
            string tags = string.Join(" ", splittedMessage.Skip(1));

            danbooruApi.danbooru.Classes.Post danbooruPost = Variables.danbooruApi.RandomPostByTags(tags, new string[] { "e" });
            if (danbooruPost == null)
            {
                string sendText = "There is no such tag\\photo for this request.\nTry again.";
                await bot.SendTextMessageAsync(msg.Chat.Id,sendText, replyToMessageId: msg.MessageId);
                return;
            }

            InputOnlineFile inputOnlineFile = new InputOnlineFile(Variables.danbooruApi.ImageUrlToStream(danbooruPost.file_url));

            string text = $"Id: {danbooruPost.id}\nRating: {danbooruPost.rating}\nTags: <b>{danbooruPost.bestTag}</b> ...";
            await bot.SendPhotoAsync(msg.Chat.Id, inputOnlineFile, caption: text, replyToMessageId: msg.MessageId, parseMode:ParseMode.Html);
        }
        public static async Task DanbooruNonExplicit(ITelegramBotClient bot, Message msg)
        {
            string[] splittedMessage = msg.Text.Split(" ");
            string tags = string.Join(" ", splittedMessage.Skip(1));

            
            danbooruApi.danbooru.Classes.Post danbooruPost = Variables.danbooruApi.RandomPostByTags(tags, new string[] { "q", "g", "s" });
            if (danbooruPost == null)
            {
                string sendText = "There is no such tag\\photo for this request.\nTry again.";
                await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId);
                return;
            }

            InputOnlineFile inputOnlineFile = new InputOnlineFile(Variables.danbooruApi.ImageUrlToStream(danbooruPost.file_url));

            string text = $"Id: {danbooruPost.id}\nRating: {danbooruPost.rating}\nTags: <b>{danbooruPost.bestTag}</b> ...";
            await bot.SendPhotoAsync(msg.Chat.Id, inputOnlineFile, caption: text, replyToMessageId: msg.MessageId, parseMode: ParseMode.Html);
        }


        public static async Task Test(ITelegramBotClient bot, Message msg)
        {
            var user = Variables.osuUsers.FirstOrDefault(m => m.telegramId == msg.From.Id);
            var chat = Variables.chats.FirstOrDefault(m => m.chat.Id == msg.Chat.Id);
            ILocalization language = Langs.GetLang(chat.language);

            
        }
    }
}
