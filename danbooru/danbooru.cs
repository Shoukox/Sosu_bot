using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using danbooruApi.danbooru.Classes;
using System.IO;
using System.Linq;

namespace danbooruApi.danbooru
{
    public class danbooru
    {
        private readonly string host = "https://danbooru.donmai.us/";
        private readonly string file_type = "filetype:png,jpg";
        private readonly string file_size = "filesize:200kb..9.5M";

        public Tag FindBestTag(string tags)
        {

            try
            {
                using (WebClient wc = new WebClient())
                {
                    string url = host + $"tags.json?search[name_matches]={tags}*&search[order]=count";
                    string json = wc.DownloadString(url);
                    Tag tag = Newtonsoft.Json.JsonConvert.DeserializeObject<Tag[]>(json).ToArray()[0];
                    return tag;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public Post RandomPostByTags(string tags, string[] ratings = null)
        {
            if (ratings == null) ratings = new string[] { "e", "q", "g", "s" };
            
            tags = tags.Replace(" ", "_");
            if (tags != "")
            {
                var bestTag = FindBestTag(tags);
                if (bestTag == null) return null;

                tags = bestTag.name;
            }

            try
            {
                using (WebClient wc = new WebClient())
                {
                    string url = host + $"posts/random.json?tags={tags} {file_size} rating:{string.Join(",", ratings)}";
                    string json = wc.DownloadString(url);
                    Post post = Newtonsoft.Json.JsonConvert.DeserializeObject<Post>(json);
                    if (post.file_url == null) return null;
                    post.bestTag = tags;
                    return post;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public Post RandomPostById(long id, string[] ratings = null)
        {
            if (ratings == null) ratings = new string[] { "e", "q", "g", "s" };

            try
            {
                using (WebClient wc = new WebClient())
                {
                    string url = host + $"posts/{id}.json?tags={file_size} rating:{string.Join(",", ratings)}";
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
