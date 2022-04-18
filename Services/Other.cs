using OppaiSharp;
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sosu.Services
{
    class Other
    {
        public static double[] ppCalc(long beatmap_id, double accuracy, Mods mods, int misses, int combo)
        {
            byte[] data = new WebClient().DownloadData($"https://osu.ppy.sh/osu/{beatmap_id}");
            var stream = new MemoryStream(data, false);
            var reader = new StreamReader(stream);
            var beatmapp = Beatmap.Read(reader);
            var pp = new PPv2(new PPv2Parameters(beatmapp, accuracy: accuracy / 100, misses, combo, mods: mods));
            var ppifFc = new PPv2(new PPv2Parameters(beatmapp, accuracy: accuracy / 100, 0, -1, mods: mods));
            return new double[] { pp.Total, ppifFc.Total };
        }
        //public static double ppCalc1(long beatmap_id, double accuracy, Mods mods, int count100, int count50, int misses, int combo)
        //{
        //    var str = $"{mods}";
        //    string m = "";
        //    if (!str.Contains("NoMod"))
        //    {
        //        foreach (var item in str.Replace(",", ""))
        //        {
        //            if (char.IsUpper(item)) m += "-m " + char.ToLower(item);
        //        }
        //    }
        //    var process = Process.Start(
        //      new ProcessStartInfo
        //      {
        //          FileName = @"pp\PerformanceCalculator.exe",
        //          Arguments = $"simulate osu {beatmap_id} -j -X {misses} -M {count50} -G {count100} -a {accuracy.ToString("N2", System.Globalization.CultureInfo.CurrentUICulture)} {m}",
        //          //CreateNoWindow = true,
        //          RedirectStandardOutput = true,
        //          UseShellExecute = false,
        //      });
        //    Types.PerformanceCalculatorModell info = Newtonsoft.Json.JsonConvert.DeserializeObject<Types.PerformanceCalculatorModell>(process.StandardOutput.ReadToEnd());
        //    process.Close();
        //    return info.performance_attributes.pp;
        //}
        public static string Capitalize(string text)
        {
            if (text.Length <= 1)
                return text;
            else
            {
                return text.Substring(0, 1).ToUpper() + text.Substring(1);
            }
        }
        public static int GetMax(params int[] numbers) => numbers.Max();
        public static string GetUrlFromText(string text)
        {
            string pattern = "https://"; string url = "";
            int ind = text.IndexOf(pattern);
            for (int i = ind; i <= text.Length - 1; i++)
            {
                if (text[i] != ' ')
                {
                    url += text[i];
                }
                else
                {
                    break;
                }
            }
            return url;
        }
        public static int GetBeatmapIdFromLink(string beatmapUrl)
        {
            return int.Parse(beatmapUrl.Split("/").Last());
        }
        public static string GetModsStringFromEnumMods(string enumMods)
        {
            string answer = "";
            foreach(var item in enumMods)
            {
                if (char.IsUpper(item)) answer += item;
            }
            return answer;
        }
        public static void SaveData()
        {
            string path = $"{Directory.GetCurrentDirectory()}\\config.txt";
            using (StreamWriter sw = new StreamWriter(path, false))
            {
                foreach (var item in Variables.osuUsers)
                {
                    sw.Write($"{item.osuName}==={item.telegramId}\n");
                }
            }
        }
        public static void LoadData()
        {
            //string path = $"{Directory.GetCurrentDirectory()}\\config.txt";
            //if (System.IO.File.Exists(path))
            //{
            //    using (StreamReader sr = new StreamReader(path))
            //    {
            //        while (!sr.EndOfStream)
            //        {
            //            string line = sr.ReadLine();
            //            if (line.Contains("==="))
            //            {
            //                string[] splitted = line.Split("===");
            //                Variables.osuUsers.Add(new osubot.Bot.Types.osuUser{ osuName = splitted[0], telegramId = long.Parse(splitted[1]) });
            //            }
            //        }
            //    }
            //}

            var osuusers = Variables.db.GetData("SELECT * FROM osuusers", 3);
            var osuchats = Variables.db.GetData("SELECT * FROM osuchats", 4);

            foreach (var item in osuusers)
            {
                Variables.osuUsers.Add(new Sosu.Types.osuUser(telegramId: (long)(item[0]), osuName: (string)item[1], pp: (double)item[2]));
            }
            foreach (var item in osuchats)
            {
                var members = item[2] is DBNull ? new List<long>() : ((long[])item[2]).ToList();
                Variables.chats.Add(new Sosu.Types.Chat(new Telegram.Bot.Types.Chat { Id = (long)(item[1]) }, (int)(item[0]), members = members, (string)(item[3])));
            }

            Console.WriteLine($"groups: {Variables.chats.Count}, users: {Variables.osuUsers.Count}");
        }
    }
}
