using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace com.aurora.aumusic
{
    public class ButtonWithoutKeySelect : Button
    {
        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            e.Handled = false;
        }
        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            e.Handled = false;
        }
    }
}
