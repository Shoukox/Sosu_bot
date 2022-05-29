using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace danbooruApi.danbooru.Classes
{
    public class Tag
    {
        public int id { get; set; }
        public string name { get; set; }
        public int? post_count { get; set; }
        public int? category { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public bool is_locked { get; set; }
        public bool is_deprecated { get; set; }

    }


}
