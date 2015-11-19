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
            song.Title = "后青春期的诗";
            song.Artists = new string[] { "五月天" };
            Debug.WriteLine((await LrcHelper.isLrcExist(song)).ToString());
        }
    }
}
