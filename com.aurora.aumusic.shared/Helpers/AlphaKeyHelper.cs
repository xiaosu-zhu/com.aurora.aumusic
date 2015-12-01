using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization.Collation;

namespace com.aurora.aumusic.shared.Helpers
{
    public class AlphaKeyGroup<T> : List<T>
    {
        const string GlobeGroupKey = "?";
        public string Key { get; private set; }
        //public List<T> this { get; private set; }
        public AlphaKeyGroup(string key)
        {
            Key = key;
        }
        private static List<AlphaKeyGroup<T>> CreateDefaultGroups(CharacterGroupings slg)
        {
            List<AlphaKeyGroup<T>> list = new List<AlphaKeyGroup<T>>();
            foreach (CharacterGrouping cg in slg)
            {
                if (cg.Label == "")
                    continue;
                else if (cg.Label == "...")
                {
                    list.Add(new AlphaKeyGroup<T>(GlobeGroupKey));
                }
                else
                    list.Add(new AlphaKeyGroup<T>(cg.Label));
            }
            return list;
        }
        public static List<AlphaKeyGroup<T>> CreateGroups(IEnumerable<T> items, Func<T, string> keySelector, bool sort)
        {
            CharacterGroupings slg = new CharacterGroupings();
            CharacterGrouping cg = slg[0];
            List<AlphaKeyGroup<T>> list = CreateDefaultGroups(slg);
            foreach (T item in items)
            {
                int index = 0;
                string label = slg.Lookup(keySelector(item));
                index = list.FindIndex(alphagroupkey => (alphagroupkey.Key.Equals(label, StringComparison.CurrentCultureIgnoreCase)));
                if (index == -1)
                    index = list.FindIndex(x => x.Key == "&");
                if (index < list.Count)
                    list[index].Add(item);
            }
            if (sort)
            {
                foreach (AlphaKeyGroup<T> group in list)
                {
                    group.Sort((c0, c1) => { return keySelector(c0).CompareTo(keySelector(c1)); });
                }
            }
            return list;
        }
    }

    public class ArtistsKeyGroup<T> : List<T>
    {
        const string GlobeGroupKey = "?";
        public string[] Key { get; private set; }
        public string[] Artworks { get; private set; }
        //public List<T> this { get; private set; }
        public ArtistsKeyGroup(string[] key)
        {
            Key = key;
        }
        public void SetArtworks(string[] artworks)
        {
            Artworks = artworks;
        }

        public static List<ArtistsKeyGroup<T>> CreateGroups(IEnumerable<T> items, Func<T, string[]> keySelector, bool sort)
        {
            List<ArtistsKeyGroup<T>> list = new List<ArtistsKeyGroup<T>>();
            foreach (T item in items)
            {
                int index = 0;
                string[] label = keySelector(item);
                index = list.FindIndex(group =>
                {
                    int i = 0;
                    if (group.Key.Length != label.Length)
                        return false;
                    foreach (var artist in group.Key)
                    {
                        if (artist != label[i])
                            return false;
                        i++;
                    }
                    return true;
                });
                if (index == -1)
                {
                    list.Add(new ArtistsKeyGroup<T>(label));
                    list[list.Count - 1].Add(item);
                }
                else if (index < list.Count)
                    list[index].Add(item);
            }
            if (sort)
            {
                foreach (ArtistsKeyGroup<T> group in list)
                {
                    group.Sort((c0, c1) => { return keySelector(c0)[0].CompareTo(keySelector(c1)[0]); });
                }
            }
            return list;
        }

    }
}
