using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using com.aurora.aumusic.shared.Songs;
using com.aurora.aumusic.shared.Lrc;
using System.Diagnostics;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public static async void TestMethod1()
        {
            Song song = new Song();
            song.Title = "鳥の詩";
            song.Artists = new string[] { "Lia" };
            LrcRequestModel lrcresult = (await LrcHelper.isLrcExist(song));
            if (lrcresult.count > 0)
            {
                Debug.Write(await LrcHelper.Fetch(lrcresult, song));
            }
        }
    }
}
