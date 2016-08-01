using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuGouMusicUWP.Class
{
    public class localmusic
    {
        public static async void get_loacl_music(Windows.Storage.StorageFolder folder)
        {
            var files = await folder.GetFilesAsync();
            var data = files.ToList();
        }
    }
}
