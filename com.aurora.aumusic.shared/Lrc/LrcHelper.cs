//Copyright(C) 2015 Aurora Studio

//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
//to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
//WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.



/// <summary>
/// Usings
/// </summary>
using com.aurora.aumusic.shared.Songs;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace com.aurora.aumusic.shared.Lrc
{
    public class LrcHelper
    {
        private static char[] NipponChar = new char[]{'あ','か','さ','た','な','は','ま',
            'や','ら','わ','い','き','し','ち','に','ひ','み','り','う','く','す',
            'つ','ぬ','ふ','む','ゆ','る','ん','え','け','せ','て','ね','へ','め',
            'れ','お','こ','そ','と','の','ほ','も','よ','ろ','を','が','ざ','だ',
            'ば','ぱ','ぎ','じ','ぢ','び','ぴ','ぐ','ず','づ','ぶ','げ','ぜ','で','べ','ぺ','ご','ぞ','ど','ぼ','ぽ'};

        public static async Task<string> Fetch(LrcRequestModel lrcresult, Song song)
        {
            if (lrcresult == null || lrcresult.count == 0)
                return null;
            string url = lrcresult.result[0].lrc;

            return await SaveLrctoStorage(await WebHelper.WebDOWNAsync(url), song);
        }

        private static async Task<string> SaveLrctoStorage(Stream stream, Song song)
        {
            StreamReader objReader = new StreamReader(stream);
            string sLine = "";
            sLine = await objReader.ReadToEndAsync();
            var uri = song.Title + "-" + song.Artists[0] + "-" + song.Album + ".lrc";
            await FileHelper.SaveFile(sLine, uri);
            return uri;
        }

        public static async Task<LrcRequestModel> isLrcExist(Song song)
        {
            try
            {
                var url = genreqest(song);
                var result = await WebHelper.WebGETAsync(url, new LrcRequestModel());
                if (result != null && result.count == 0)
                {
                    url = genreqestthin(song);
                    result = await WebHelper.WebGETAsync(url, new LrcRequestModel());
                }
                return result;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        private static string genreqest(Song song)
        {
#if DEBUG
            bool isNippon = false;
            foreach (var item in NipponChar)
            {
                if (song.Title.Contains(item))
                {
                    isNippon = true;
                    break;
                }
            }
#endif
            string title = null;
#if DEBUG
            if (!isNippon)
                title = ChineseConverter.ToSimplified(song.Title);
            else
#endif
            title = song.Title;

            string artist = null;
            if (song.Artists != null && song.Artists.Length > 0 && song.Artists[0] != "Unknown Artists")
#if DEBUG
                if (!isNippon)
                    artist = ChineseConverter.ToSimplified(song.Artists[0]);
                else
#endif
                artist = song.Artists[0];
            return artist != null ? "http://geci.me/api/lyric/" + title + '/' + artist : "http://geci.me/api/lyric/" + title;
        }

        private static string genreqestthin(Song song)
        {
#if DEBUG
            bool isNippon = false;
            foreach (var item in NipponChar)
            {
                if (song.Title.Contains(item))
                {
                    isNippon = true;
                    break;
                }
            }
#endif
            string title;
#if DEBUG
            if (!isNippon)
                title = ChineseConverter.ToSimplified(song.Title);
            else
#endif
            title = song.Title;

            return "http://geci.me/api/lyric/" + title;
        }
    }
}
