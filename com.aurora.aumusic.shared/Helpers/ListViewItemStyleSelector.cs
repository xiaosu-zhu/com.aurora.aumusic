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
