using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuGouUWP.Class
{
     public class MusicClass
    {
        public enum music_type
        {
            kuogu,local
        }
        public class kugou_music
        {
            public string title, singername, ablumname, hash, mvhash, filename,url;
            public int filesize, ablumid, singerid;
        }
        public class local_music
        {
            public Windows.Storage.StorageFile file;
            public Windows.Storage.FileProperties.MusicProperties data;
        }
        public class music
        {
            public music_type type;
            public kugou_music kg_music;
            public local_music l_music;
        }
        public static async Task<music> get_local_music(string uri)
        {
            try
            {
                var music = new music();
                music.type = music_type.local;
                music.l_music = new local_music();
                music.l_music.file = await Windows.Storage.StorageFile.GetFileFromPathAsync(uri);
                music.l_music.data =await music.l_music.file.Properties.GetMusicPropertiesAsync();
                return music;
            }
            catch (Exception)
            {
                return null;
            }

        }
        public static async Task<music> get_kg_music(string hash)
        {
            try
            {
                var music = new music();
                music.type = music_type.kuogu;
                music.kg_music = new kugou_music();
                music.kg_music.url = await kugou.get_musicurl_by_hash(hash);
                return music;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
