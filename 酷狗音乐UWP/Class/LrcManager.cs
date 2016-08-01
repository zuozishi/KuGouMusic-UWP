using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KuGouMusicUWP.Class
{
    public class Lrc
    {
        /// <summary>
        /// 歌曲
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 艺术家
        /// </summary>
        public string Artist { get; set; }
        /// <summary>
        /// 专辑
        /// </summary>
        public string Album { get; set; }
        /// <summary>
        /// 歌词作者
        /// </summary>
        public string LrcBy { get; set; }
        /// <summary>
        /// 偏移量
        /// </summary>
        public string Offset { get; set; }

        /// <summary>
        /// 歌词
        /// </summary>
        public Dictionary<double, string> LrcWord = new Dictionary<double, string>();

        /// <summary>
        /// 获得歌词信息
        /// </summary>
        /// <param name="LrcPath">歌词路径</param>
        /// <returns>返回歌词信息(Lrc实例)</returns>
        public static Lrc InitLrc(List<string> LrcData)
        {
            try
            {
                Lrc lrc = new Lrc();
                foreach (var line in LrcData)
                {
                    if (line.StartsWith("[ti:"))
                    {
                        lrc.Title = SplitInfo(line);
                    }
                    else if (line.StartsWith("[ar:"))
                    {
                        lrc.Artist = SplitInfo(line);
                    }
                    else if (line.StartsWith("[al:"))
                    {
                        lrc.Album = SplitInfo(line);
                    }
                    else if (line.StartsWith("[by:"))
                    {
                        lrc.LrcBy = SplitInfo(line);
                    }
                    else if (line.StartsWith("[offset:"))
                    {
                        lrc.Offset = SplitInfo(line);
                    }
                    else
                    {
                        Regex regex = new Regex(@"\[([0-9.:]*)\]+(.*)", RegexOptions.Compiled);
                        MatchCollection mc = regex.Matches(line);
                        double time = TimeSpan.Parse("00:" + mc[0].Groups[1].Value).TotalMilliseconds;
                        string word = mc[0].Groups[2].Value;
                        lrc.LrcWord.Add(time, word);
                    }
                }
                return lrc;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 处理信息(私有方法)
        /// </summary>
        /// <param name="line"></param>
        /// <returns>返回基础信息</returns>
        static string SplitInfo(string line)
        {
            return line.Substring(line.IndexOf(":") + 1).TrimEnd(']');
        }
    }
}
