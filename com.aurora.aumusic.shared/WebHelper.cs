using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared
{
    public static class WebHelper
    {
        public async static Task<T> WebGETAsync<T>(string url, T data)
        {
            WebRequest wrGETURL;
            wrGETURL = WebRequest.Create(url);
            try
            {
                wrGETURL.Method = "GET";
                Stream objStream;
                objStream = (await wrGETURL.GetResponseAsync()).GetResponseStream();

                StreamReader objReader = new StreamReader(objStream);

                string sLine = "";
                sLine = await objReader.ReadToEndAsync();
                wrGETURL.Abort();
                wrGETURL = null;
                return JsonHelper.FromJson<T>(sLine);
            }
            catch (WebException)
            {
                wrGETURL.Abort();
                wrGETURL = null;
                throw;
            }

        }
        public async static Task<Stream> WebDOWNAsync(string url)
        {
            WebRequest wrGETURL;
            wrGETURL = WebRequest.Create(url);
            try
            {
                wrGETURL.Method = "GET";
                Stream objStream;
                objStream = (await wrGETURL.GetResponseAsync()).GetResponseStream();
                wrGETURL.Abort();
                wrGETURL = null;
                return objStream;
            }
            catch (WebException)
            {
                wrGETURL.Abort();
                wrGETURL = null;
                throw;
            }

        }
    }

}
