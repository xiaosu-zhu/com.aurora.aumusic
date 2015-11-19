using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared
{
    public static class ChineseConverter
    {
        private const int LocaleSystemDefault = 0x0800;
        private const int LcmapSimplifiedChinese = 0x02000000;
        private const int LcmapTraditionalChinese = 0x04000000;

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int LCMapString(int locale, int dwMapFlags, string lpSrcStr, int cchSrc,
                                              [Out] string lpDestStr, int cchDest);

        public static string ToSimplified(string argSource)
        {
            var t = new String(' ', argSource.Length);
            LCMapString(LocaleSystemDefault, LcmapSimplifiedChinese, argSource, argSource.Length, t, argSource.Length);
            return t;
        }

        public static string ToTraditional(string argSource)
        {
            var t = new String(' ', argSource.Length);
            LCMapString(LocaleSystemDefault, LcmapTraditionalChinese, argSource, argSource.Length, t, argSource.Length);
            return t;
        }
    }
}
