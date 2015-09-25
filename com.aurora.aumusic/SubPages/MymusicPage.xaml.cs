using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace com.aurora.aumusic
{
    public sealed partial class MymusicPage : Page
    {

        public MymusicPage()
        {
            this.InitializeComponent();

            AlbumsFlowControls.ItemsSource = Enumerable.Range(0, 30).Select(i => new AlbumItem { Text = i.ToString() });
        }
    }

    public sealed class AlbumFlowPanel : Panel
    {


        protected override Size MeasureOverride(Size availableSize)
        {
            //横向瀑布流

            //三组流宽度记录
            KeyValuePair<double, int>[] flowLength = new KeyValuePair<double, int>[3];
            foreach(int index in Enumerable.Range(0,3))
            {
                flowLength[index] = new KeyValuePair<double, int>(0.0, index);
            }

            //每组宽度为总宽度1/3
            double flowWidth = availableSize.Width / 3;

            //子控件高为组高，宽无限制
            Size childMeasureSize = new Size(flowWidth, double.PositiveInfinity);

            //子控件遍历计算宽度
            foreach(UIElement childElement in Children)
            {
                childElement.Measure(childMeasureSize);
                Size childSize = childElement.DesiredSize;
                //得到子控件宽
                double childLength = childSize.Height;
                //暂存最短流宽度
                var tempPair = flowLength[0];
                //最短流宽度重新计算
                flowLength[0] = new KeyValuePair<double, int>(tempPair.Key + childLength, tempPair.Value);
                //重新按流宽度排列键值对        这里以Key 的值作为排列依据，flowWidth[0]为Key最小的键值对，P可替换为任意字母
                flowLength = flowLength.OrderBy(P => P.Key).ToArray();
            }
            //返回 宽：最长流的宽；高：传入的高
            return new Size(availableSize.Width, flowLength.Last().Key);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            KeyValuePair<double, int>[] flowLength = new KeyValuePair<double, int>[3];
            double flowWidth = finalSize.Width / 3;

            double[] xWidth = new double[3];
            foreach (int index in Enumerable.Range(0, 3))
            {
                flowLength[index] = new KeyValuePair<double, int>(0.0, index);
                xWidth[index] = index * flowWidth;
            }

            foreach (UIElement childElem in Children)
            {
                // 获取子控件期望大小
                Size elemSize = childElem.DesiredSize;
                double elemLength = elemSize.Height;

                //得到最短流宽度
                var pair = flowLength[0];
                double chosenFlowHeight = pair.Key;
                int chosenFlowIdx = pair.Value;

                // 设置即将放置的控件坐标
                Point pt = new Point(xWidth[chosenFlowIdx], chosenFlowHeight);

                // 调用Arrange进行子控件布局。并让子控件利用上整个流的宽度。
                childElem.Arrange(new Rect(pt, new Size(flowWidth, elemSize.Height)));

                // 重新计算最短流。
                flowLength[0] = new KeyValuePair<double, int>(chosenFlowHeight + elemLength, chosenFlowIdx);
                flowLength = flowLength.OrderBy(p => p.Key).ToArray();
            }
            return finalSize;
        }
    }
}
