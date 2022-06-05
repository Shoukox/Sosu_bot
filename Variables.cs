using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sosu.Types;
using Sosu.osu.V1;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Sosu
{
    public class Variables
    {
        public static osuApi osuApi;
        public static danbooruApi.danbooru.danbooru danbooruApi;
        public static Telegram.Bot.Types.User bot;
        public static Database db;
        public static List<long> WHITELIST = new List<long>() { 728384906 };

        public static Dictionary<string[], Func<ITelegramBotClient, Message, Task>> commands;
        public static Dictionary<string, Func<ITelegramBotClient, CallbackQuery, Task>> callbacks;
        public static List<Types.osuUser> osuUsers;
        public static List<Types.Chat> chats;
    }
}
