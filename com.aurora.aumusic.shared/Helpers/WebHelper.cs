//Copyright(C) 2015 Aurora Studio

//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
//to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
//WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.



using System;
/// <summary>
/// Usings
/// </summary>
using System.IO;
using System.Net;
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
            catch (Exception)
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
            catch (Exception)
            {
                wrGETURL.Abort();
                wrGETURL = null;
                throw;
            }

        }
    }

}
