using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sosu.Localization
{
    public class Langs
    {
        public static ILocalization GetLang(string lang)
        {
            ILocalization language = new ru();
            switch (lang)
            {
                case "ru":
                    language = new ru();
                    break;
                case "en":
                    language = new en();
                    break;
            }
            return language;
        }
        public static string ReplaceEmpty(string text, string[] replace)
        {
            foreach (var item in replace)
            {
                int ind = text.IndexOf("{}");
                text = text.Remove(ind, 2).Insert(ind, item);
            }
            return text;
        }
        public class ru : ILocalization
        {
            public string settings() => "Настройки:";
            public string settings_language_changedSuccessfully() => "Язык успешно изменен на русский.";
            public string settings_language_ru() => "Русский";
            public string settings_language_en() => "English";
            public string command_start() =>
                $"Бот-помощник для игрока osu!\n" +
                $"/help - для получения списка всех комманд.\n\n" +
                $"Если вы найдете какие-либо проблемы или предложения по расширению функционала бота, пишите моему создателю - @Shoukkoo";
            public string command_help() =>
                $"Бот-помощник для осера\n\n" +
                $"Комманды:\n" +
                $"<b>Важно! Если в вашем нике есть пробелы, заменяйте их на \"_\". Например, \"Blue Archive\" -> \"Blue_Archive\"</b>\n\n" +
                $"/set [nickname] - добавить\\изменить свой ник в боте.\n" +
                $"/user [nickname] - краткая информация об игроке.\n" +
                $"/last [nickname] [count] - последние сыгранные игры.\n" +
                $"/lss [nickname] [count] - последние сыгранные игры. (нестандартная альтернатива)\n" +
                $"/score [beatmap_link] - ваши рекорды на этой карте.\n" +
                $"/userbest [nickname] [mode] - лучшие игры игрока.\n" +
                $"/compare [nickname1] [nickname2] [mode] - сравнить игроков. [mode]: 0-std, 1-taiko, 2-catch, 3-mania\n" +
                $"/chat_stats - топ10 игроков в чате.\n\n" +
                $"[mode]: 0-std, 1-taiko, 2-catch, 3-mania\n" +
                $"Если отправить ссылку карты, бот отправит краткую информацию о ней";
            public string command_last() =>
                "{}. <b>({})</b> <a href=\"https://osu.ppy.sh/beatmaps/{}\">{} [{}]</a> <b>({})</b>\n" +
                    "{} / {} / {} / {}❌ - <b><i>{}</i></b>%\n" +
                    "<b>{}</b> <i>{}/{}</i> <b><u>~{}pp</u></b> (<b><u>~{}pp</u></b> if FC)\n({}) {}% пройдено\n\n";
            public string command_lastScoreSuka() =>
                "<b>{}</b>" +
                "\n{}" +
                "\n{} = {}" +
                "\n\n{}";

            public string command_set() => "Теперь ты <b>{}</b>";
            public string command_score() =>
                "<b>({})</b> <a href=\"https://osu.ppy.sh/beatmaps/{}\">{} [{}]</a> <b>({})</b>\n" +
                    "{} / {} / {} / {}❌ - <b><i>{}</i></b>%\n" +
                    "<b>{}</b> <i>{}/{}</i> <b><u>{}pp</u></b>\n({})\n\n";
            public string command_user() =>
                   "<b>{}</b>\n" +
                "<a href=\"{}\"><i>{}</i></a>\n\n" +
                "<b>rank</b>: <i>#{} (#{} {})</i>\n" +
                "<b>pp</b>: <i>{} {}</i>\n" +
                "<b>accuracy</b>: <i>{}%</i>\n" +
                "<b>plays</b>: <i>{}</i>\n" +
                "<b>playtime</b>: <i>{}h</i>\n\n" +
                "<i>{}</i> <b>SSH</b> - <i>{}</i> <b>SH</b>\n" +
                "<i>{}</i> <b>SS</b> - <i>{}</i> <b>S</b> - <i>{}</i> <b>A</b>";
            public string command_compare() =>
                  "<pre>" +
                "{}\n\n" +
                "{}  {}\n" +
                "{}  #{}\n" +
                "{}  #{}\n" +
                "{}  {}pp\n" +
                "{}  {}\n" +
                "{}  {}h\n" +
                "</pre>";
            public string command_userbest() =>
                 "{}. (<b>{}</b>) <a href=\"http://osu.ppy.sh/b/{}\">{} [{}]</a> <b>{}</b>\n" +
                    "{} / {} / {} / {}❌ - <b><i>{}</i></b>%\n" +
                    "<b>{}</b> <i>{}/{}</i> <b><u>{}pp</u></b>\n\n";
            public string command_chatstats_title() => "Топ-10 осеров в группе:\n\n";
            public string command_chatstats_row() => "<b>{}. {}</b>: <i>{}pp</i>\n";
            public string command_chatstats_end() => "\nИспользуйте <b>/user</b>, чтобы обновить ваш <b>pp</b> в данном списке.";
            public string send_mapInfo() =>
                     "[{}] - {}* - {} - {} - <b>{}</b>\n" +
                     "<b>CS</b>: {} | <b>AR</b>: {} | <b>OD</b>: {} | <b>HP</b>: {} | <b>BPM</b>: {}\n" +
                     "100% - {}pp | 98% - {}pp | 96% - {}pp\n<b>{}</b>";
            public string waiting() => "Подожди немного...";

            public string error_noUser() => "Ты кто? Юзай\n/set [nickname]";
            public string error_userNotFound() => "Произошел троллинг...\nИгрок не найден";
            public string error_noRecords() => "Произошел троллинг...\nРекорды не найдены";
            public string error_argsLength() => "Произошел троллинг...\nНеверное количество аргументов";
            public string error_noPreviousScores() => "Прошлых рекордов не существует";

            public string command_lastScoreSuka_rankD() => "Нубас";
            public string command_lastScoreSuka_rankC() => "Лох";
            public string command_lastScoreSuka_rankB() => "Лошара";
            public string command_lastScoreSuka_rankA() => "Неплох";
            public string command_lastScoreSuka_rankS() => "Красава";
            public string command_lastScoreSuka_longDuration() => "Нифига себе длинная мапа"; //>=5min
            public string command_lastScoreSuka_lowDuration() => "Опять 30 секундные фармилки играешь?"; //<1 min
            public string command_lastScoreSuka_shitMisses() => "Ппц обидные миссы..."; //миссы после 90% полученного комбо
            public string command_lastScoreSuka_manyMisses() => "Писец ты кривой с таким количеством миссов..."; //миссы после 90% полученного комбо
            public string command_lastScoreSuka_manySliders() => "Что по слайдерам?"; //слайдеров больше, чем нот
            public string command_lastScoreSuka_mapTitleTooLong() => "Что за стремное название карты?"; //>40 символов в названии
            public string command_lastScoreSuka_mapFailed() => "Не позорься, пройди ее хотя бы. На этом все."; //проиграл карту
            public string command_lastScoreSuka_tooEasyMapForPlayer() => "Сейчас бы с таким ранком такие легкие мапы играть";
            public string command_lastScoreSuka_tooHardMapForPlayer() => "Тебе еще рано играть такое";

            public string map_ranked()
            {
                throw new NotImplementedException();
            }

            public string map_loved()
            {
                throw new NotImplementedException();
            }

            public string map_unranked()
            {
                throw new NotImplementedException();
            }

            public string map_pending()
            {
                throw new NotImplementedException();
            }

            public string map_approved()
            {
                throw new NotImplementedException();
            }

            public string map_wip()
            {
                throw new NotImplementedException();
            }

            public string map_qualified()
            {
                throw new NotImplementedException();
            }

            public string map_graveyard()
            {
                throw new NotImplementedException();
            }
        }
        public class en : ILocalization
        {
            public string settings() => "Settings:";
            public string settings_language_changedSuccessfully() => "The language has been successfully changed to English.";
            public string settings_language_ru() => "Русский";
            public string settings_language_en() => "English";
            public string command_start() =>
                $"Bot-assistant for osu! player\n" +
                $"/help - to get a list of all commands.\n\n" +
                $"If you find any problems or suggestions to expand the functionality of the bot, write to my creator - @Shoukkoo";
            public string command_help() =>
                $"Bot-assistant for osu! player\n\n" +
                $"Commands:\n" +
                $"<b>Important! If your nickname has spaces, replace them with \"_\". For example \"Blue Archive\" -> \"Blue_Archive\"</b>\n\n" +
                $"/set [nickname] - add\\change your nickname in the bot.\n" +
                $"/user [nickname] - brief information about the player.\n" +
                $"/last [nickname] [count] - last played games.\n" +
                $"/lss [nickname] [count] - last played games. (non-standard alternative)\n" +
                $"/score [beatmap_link] - your records on this map.\n" +
                $"/userbest [nickname] [mode] - best player games.\n" +
                $"/compare [nickname1] [nickname2] [mode] - compare players.\n" +
                $"/chat_stats - top 10 players in the chat.\n\n" +
                $"[mode]: 0-std, 1-taiko, 2-catch, 3-mania\n" +
                $"If you send a map link, the bot will send a brief information about it";
            public string command_last() =>
               "{}. <b>({})</b> <a href=\"https://osu.ppy.sh/beatmaps/{}\">{} [{}]</a> <b>({})</b>\n" +
                   "{} / {} / {} / {}❌ - <b><i>{}</i></b>%\n" +
                   "<b>{}</b> <i>{}/{}</i> <b><u>~{}pp</u></b> (<b><u>~{}pp</u></b> if FC)\n({}) {}% completion\n\n";
            public string command_lastScoreSuka() =>
                "<b>{}</b>" +
                "\n{}" +
                "\n{} = {}" +
                "\n\n{}";
            public string command_set() => "Now you are <b>{}</b>";
            public string command_score() =>
               "<b>({})</b> <a href=\"https://osu.ppy.sh/beatmaps/{}\">{} [{}]</a> <b>({})</b>\n" +
                   "{} / {} / {} / {}❌ - <b><i>{}</i></b>%\n" +
                   "<b>{}</b> <i>{}/{}</i> <b><u>{}pp</u></b>\n({})\n\n";
            public string command_user() =>
                 "<b>Standard</b>\n" +
              "<a href=\"{}\"><i>{}</i></a>\n\n" +
              "<b>rank</b>: <i>#{} (#{} {})</i>\n" +
              "<b>pp</b>: <i>{} {}</i>\n" +
              "<b>accuracy</b>: <i>{}%</i>\n" +
              "<b>plays</b>: <i>{}</i>\n" +
              "<b>playtime</b>: <i>{}h</i>\n\n" +
              "<i>{}</i> <b>SSH</b> - <i>{}</i> <b>SH</b>\n" +
              "<i>{}</i> <b>SS</b> - <i>{}</i> <b>S</b> - <i>{}</i> <b>A</b>";
            public string command_compare() =>
               "<pre>" +
             "{}\n\n" +
             "{}  {}\n" +
             "{}  #{}\n" +
             "{}  #{}\n" +
             "{}  {}pp\n" +
             "{}  {}\n" +
             "{}  {}h\n" +
             "</pre>";
            public string command_userbest() =>
     "{}. (<b>{}</b>) <a href=\"http://osu.ppy.sh/b/{}\">{} [{}]</a> <b>{}</b>\n" +
        "{} / {} / {} / {}❌ - <b><i>{}</i></b>%\n" +
        "<b>{}</b> <i>{}/{}</i> <b><u>{}pp</u></b>\n\n";
            public string command_chatstats_title() => "Top 10 osu! players in the group:\n\n";
            public string command_chatstats_row() => "<b>{}. {}</b>: <i>{}pp</i>\n";
            public string command_chatstats_end() => "\nUse <b>/user</b> to update your <b>pp</b> in the given list.";
            public string send_mapInfo() =>
                  "[{}] - {}* - {} - {} - <b>{}</b>\n" +
                  "<b>CS</b>: {} | <b>AR</b>: {} | <b>OD</b>: {} | <b>HP</b>: {} | <b>BPM</b>: {}\n" +
                  "100% - {}pp | 98% - {}pp | 96% - {}pp\n<b>{}</b>";
            public string waiting() => "Wait a bit...";

            public string error_noUser() => "Who are you? Use\n/set [nickname]";
            public string error_userNotFound() => "Trolling occurred...\nPlayer not found";
            public string error_noRecords() => "Trolling occurred...\nRecords are not found";
            public string error_argsLength() => "Trolling occurred...\nIncorrect number of arguments";
            public string error_noPreviousScores() => "No previous scores";

            public string command_lastScoreSuka_rankD() => "Noob";
            public string command_lastScoreSuka_rankC() => "Goof";
            public string command_lastScoreSuka_rankB() => "Nerd";
            public string command_lastScoreSuka_rankA() => "Not bad";
            public string command_lastScoreSuka_rankS() => "Cool";
            public string command_lastScoreSuka_longDuration() => "Very long map!"; //>=5min
            public string command_lastScoreSuka_lowDuration() => "Are you playing 30 second farm games again?"; //<1 min
            public string command_lastScoreSuka_shitMisses() => "Hurtful misses"; //миссы после 90% полученного комбо
            public string command_lastScoreSuka_manyMisses() => "Scribe you crooked with so many misses..."; //миссы после 90% полученного комбо
            public string command_lastScoreSuka_manySliders() => "What about sliders? Too many of them!"; //слайдеров больше, чем нот
            public string command_lastScoreSuka_mapTitleTooLong() => "What's with the name of the map?"; //>40 символов в названии
            public string command_lastScoreSuka_mapFailed() => "Don't be embarrassed, at least pass it. That's all."; //проиграл карту
            public string command_lastScoreSuka_tooEasyMapForPlayer() => "This map is too easy for you";
            public string command_lastScoreSuka_tooHardMapForPlayer() => "It's too early for you to play this map";

            public string map_ranked()
            {
                throw new NotImplementedException();
            }

            public string map_loved()
            {
                throw new NotImplementedException();
            }

            public string map_unranked()
            {
                throw new NotImplementedException();
            }

            public string map_pending()
            {
                throw new NotImplementedException();
            }

            public string map_approved()
            {
                throw new NotImplementedException();
            }

            public string map_wip()
            {
                throw new NotImplementedException();
            }

            public string map_qualified()
            {
                throw new NotImplementedException();
            }

            public string map_graveyard()
            {
                throw new NotImplementedException();
            }
        }
    }
}
