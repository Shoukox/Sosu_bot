using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using danbooruApi.danbooru.Classes;
using System.IO;

namespace danbooruApi.danbooru
{
    public class danbooru
    {
        private readonly string host = "https://danbooru.donmai.us/";

        public Post[] SearchByTags(string tags, int page = 1, int limit = 5)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string url = host + $"posts.json?tags={tags}&page={page}&limit={limit}";
                    string json = wc.DownloadString(url);
                    Post[] post = Newtonsoft.Json.JsonConvert.DeserializeObject<Post[]>(json);
                    return post;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public Post RandomPost()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string url = host + $"posts/random";
                    string json = wc.DownloadString(url);
                    Post post = Newtonsoft.Json.JsonConvert.DeserializeObject<Post>(json);
                    return post;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public Post RandomPostByTags(string tags)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string url = host + $"posts/random.json?tags={tags}";
                    string json = wc.DownloadString(url);
                    Post post = Newtonsoft.Json.JsonConvert.DeserializeObject<Post>(json);
                    return post;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public Stream ImageUrlToStream(string url)
        {
            try
            {
                byte[] imageData = null;
                using (WebClient wc = new WebClient())
                {
                    imageData = wc.DownloadData(url);
                    return new MemoryStream(imageData);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public danbooru()
        {

        }
    }
}
