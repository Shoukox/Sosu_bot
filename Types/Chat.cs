using System.Collections.Generic;

namespace Sosu.Types
{
    public class Chat
    {
        public Telegram.Bot.Types.Chat chat { get; set; }
        public long lastBeatmap_id { get; set; }
        public List<long> members { get; set; }
        public string language { get; set; }

        public Chat(Telegram.Bot.Types.Chat chat, long lastBeatmap_id, List<long> members = null, string language = "ru")
        {
            this.chat = chat;
            this.lastBeatmap_id = lastBeatmap_id;
            this.members = members ?? new List<long>();
            this.language = language;
        }
    }
}
