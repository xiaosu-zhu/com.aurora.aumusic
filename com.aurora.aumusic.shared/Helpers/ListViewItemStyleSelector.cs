using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace com.aurora.aumusic
{
    public class ListViewItemStyleSelector : StyleSelector
    {
        protected override Style SelectStyleCore(object item,
            DependencyObject container)
        {
            Style st = new Style();
            st.TargetType = typeof(ListViewItem);
            Setter backGroundSetter = new Setter();
            backGroundSetter.Property = ListViewItem.BackgroundProperty;
            ListView listView =
                ItemsControl.ItemsControlFromItemContainer(container)
                  as ListView;
            int index =
                listView.IndexFromContainer(container);
            if (index % 2 == 0)
            {
                backGroundSetter.Value = (Color)Application.Current.Resources["SystemBackgroundAltHighColor"];
            }
            else
            {
                backGroundSetter.Value = (Color)Application.Current.Resources["SystemAltHighColor"];
            }
            st.Setters.Add(backGroundSetter);
            Setter paddingSetter = new Setter();
            paddingSetter.Property = ListViewItem.PaddingProperty;
            paddingSetter.Value = 0;
            st.Setters.Add(paddingSetter);
            Setter alignSetter = new Setter();
            alignSetter.Property = ListViewItem.HorizontalContentAlignmentProperty;
            alignSetter.Value = "Stretch";
            st.Setters.Add(alignSetter);
            return st;
        }
    }
}
