using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace KuGouUWP.Class
{
    public class kugou
    {
        public class search_music
        {
            public string filename, songname, mvhash, hash, singername, hash320;
            public int filesize;
        }
        public class search_mv
        {
            public string filename, imgurl, hash, singername;
        }
        public class search_album
        {
            public string albumname, publishtime, singername, imgurl, singerid, intro;
            public int albumid, buycount;
        }
        public static async Task<string> get_song_url(string hash)
        {
            try
            {
                var sign = MD5.GetMd5String(hash + "kgcloudv2");
                var request = new System.Net.Http.HttpClient();
                var json =await request.GetStringAsync("http://trackercdn.kugou.com/i/v2/?cmd=23&hash=" + hash + "&key=" + sign + "&pid=1&vipToken=&behavior=play");
                JsonObject obj = JsonObject.Parse(json);
                var url = obj["url"];
                return url.GetString();
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async static Task<string> get_musicurl_by_hash(string hash)
        {
            try
            {
                var sign = MD5.GetMd5String(hash + "kgcloudv2");
                System.Net.Http.HttpClient httpget = new System.Net.Http.HttpClient();
                var json = await httpget.GetStringAsync("http://trackercdn.kugou.com/i/v2/?cmd=23&hash=" + hash + "&key=" + sign + "&pid=1&vipToken=&behavior=play");
                JsonObject obj = JsonObject.Parse(json);
                return obj.GetNamedString("url");
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async static Task<List<search_music>> searchmusic(string keyword,int page)
        {
            try
            {
                System.Net.Http.HttpClient httpget = new System.Net.Http.HttpClient();
                var json = await httpget.GetStringAsync("http://mobilecdn.kugou.com/api/v3/search/song?iscorrect=1&pagesize=20&plat=20&sver=3&showtype=14&page=" + page + "&keyword=" + keyword);
                json = json.Replace("320hash", "hash320");
                JsonObject obj = JsonObject.Parse(json);
                json = obj.GetNamedObject("data").GetNamedArray("info").ToString();
                return data.DataContractJsonDeSerialize<List<search_music>>(json);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async static Task<List<search_mv>> searchmv(string keyword, int page)
        {
            try
            {
                System.Net.Http.HttpClient httpget = new System.Net.Http.HttpClient();
                var json = await httpget.GetStringAsync("http://mobilecdn.kugou.com/api/v3/search/mv?iscorrect=1&pagesize=20&plat=20&sver=3&showtype=14&page=" + page + "&keyword=" + keyword);
                JsonObject obj = JsonObject.Parse(json);
                json = obj.GetNamedObject("data").GetNamedArray("info").ToString();
                return data.DataContractJsonDeSerialize<List<search_mv>>(json);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async static Task<List<search_album>> searchalbum(string keyword, int page)
        {
            try
            {
                System.Net.Http.HttpClient httpget = new System.Net.Http.HttpClient();
                var json = await httpget.GetStringAsync("http://mobilecdn.kugou.com/api/v3/search/album?iscorrect=1&pagesize=20&plat=20&sver=3&showtype=14&page=" + page + "&keyword=" + keyword);
                JsonObject obj = JsonObject.Parse(json);
                json = obj.GetNamedObject("data").GetNamedArray("info").ToString();
                return data.DataContractJsonDeSerialize<List<search_album>>(json);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
