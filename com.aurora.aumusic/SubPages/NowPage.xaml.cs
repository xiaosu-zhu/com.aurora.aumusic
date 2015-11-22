using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace com.aurora.aumusic
{
    public sealed partial class NowPage : Page
    {
        Song CurrentSong;

        private bool _loved = false;
        private bool _broken = false;

        private bool Broken
        {
            get
            {
                return _broken;
            }
        }

        private bool Loved
        {
            get
            {
                return _loved;
            }
            set
            {
                if (_loved == true && value == false)
                    _broken = true;
                if (_loved == false && value == true)
                    _broken = false;
                _loved = value;
            }
        }

        public NowPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
                CurrentSong = new Song(e.Parameter as SongModel);
        }

        private void OneStarButton_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "OneStarButton": OneStar.Begin(); break;
                case "TwoStarButton": TwoStar.Begin(); break;
                case "ThreeStarButton": ThreeStar.Begin(); break;
                case "FourStarButton": FourStar.Begin(); break;
                case "FiveStarButton": FiveStar.Begin(); break;
            }
        }

        private void OneStarButton_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            switch (CurrentSong.Rating)
            {
                case 0: NoStar.Begin(); break;
                case 1: OneStarSet.Begin(); break;
                case 2: TwoStarSet.Begin(); break;
                case 3: ThreeStarSet.Begin(); break;
                case 4: FourStarSet.Begin(); break;
                case 5: FiveStarSet.Begin(); break;
            }
        }

        private void OneStarButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "OneStarButton": OneStarSet.Begin(); CurrentSong.Rating = 1; break;
                case "TwoStarButton": TwoStarSet.Begin(); CurrentSong.Rating = 2; break;
                case "ThreeStarButton": ThreeStarSet.Begin(); CurrentSong.Rating = 3; break;
                case "FourStarButton": FourStarSet.Begin(); CurrentSong.Rating = 4; break;
                case "FiveStarButton": FiveStarSet.Begin(); CurrentSong.Rating = 5; break;
            }
        }

        private void LoveButton_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            LoveButtonOver.Begin();
        }

        private void LoveButton_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!Loved && !Broken)
            {
                LoveButtonNormal.Begin();
            }
            else if (Loved)
            {
                LoveButtonLove.Begin();
            }
            else if (Broken)
            {
                LoveButtonBreak.Begin();
            }
        }

        private void LoveButton_Click(object sender, RoutedEventArgs e)
        {
            Loved = !Loved;
            if (Loved)
            {
                LoveButtonLove.Begin();
            }
            else if (Broken)
            {
                LoveButtonBreak.Begin();
            }
        }
    }
}
