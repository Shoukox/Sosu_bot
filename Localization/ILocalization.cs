using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sosu.Localization
{
    public interface ILocalization
    {
        public string command_start();
        public string command_help();
        public string command_last();
        public string command_lastScoreSuka();
        public string command_lastScoreSuka_longDuration();
        public string command_lastScoreSuka_lowDuration();
        public string command_lastScoreSuka_shitMisses();
        public string command_lastScoreSuka_manyMisses();
        public string command_lastScoreSuka_manySliders();
        public string command_lastScoreSuka_mapTitleTooLong();
        public string command_lastScoreSuka_mapFailed();
        public string command_lastScoreSuka_tooEasyMapForPlayer();
        public string command_lastScoreSuka_tooHardMapForPlayer();

        public string command_lastScoreSuka_rankD();
        public string command_lastScoreSuka_rankC();
        public string command_lastScoreSuka_rankB();
        public string command_lastScoreSuka_rankA();
        public string command_lastScoreSuka_rankS();
        public string command_set();
        public string command_score();
        public string command_user();
        public string command_compare();
        public string command_userbest();
        public string command_chatstats_title();
        public string command_chatstats_row();
        public string command_chatstats_end();
        public string settings();
        public string settings_language_ru();
        public string settings_language_en();
        public string settings_language_changedSuccessfully();
        public string send_mapInfo();
        public string waiting();

        public string error_noUser();
        public string error_userNotFound();
        public string error_noRecords();
        public string error_argsLength();
        public string error_noPreviousScores();
    }
}
