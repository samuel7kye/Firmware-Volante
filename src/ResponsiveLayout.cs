using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace VolanteStudio {
    static class ResponsiveLayout {
        public static void Apply(Window window) {
            var content=(Grid)window.FindName("PageContent");
            content.SizeChanged+=(s,e)=>Update(window,content.ActualWidth);
        }
        static void Update(Window window,double width) {
            bool compact=width<1050;
            var home=(Grid)window.FindName("HomePage");
            var panels=home.Children.Cast<UIElement>().Take(3).ToArray();
            home.MinHeight=compact?1100:600;
            home.ColumnDefinitions[0].Width=new GridLength(1,GridUnitType.Star);
            home.ColumnDefinitions[1].Width=new GridLength(compact?1:1.45,GridUnitType.Star);
            home.ColumnDefinitions[2].Width=compact?new GridLength(0):new GridLength(1,GridUnitType.Star);
            home.RowDefinitions[0].Height=compact?new GridLength(440):new GridLength(1,GridUnitType.Star);
            home.RowDefinitions[1].Height=compact?new GridLength(1,GridUnitType.Star):new GridLength(0);
            for(int i=0;i<3;i++) {
                Grid.SetRow(panels[i],compact && i!=1?1:0);
                Grid.SetColumn(panels[i],compact?(i==2?1:0):i);
                Grid.SetColumnSpan(panels[i],compact && i==1?2:1);
                ((FrameworkElement)panels[i]).Margin=compact?new Thickness(i==2?8:0,8,i==0?8:0,8):new Thickness(i==1?20:0,0,i==1?20:0,0);
            }
            foreach(string name in new[]{"EffectsPage","ProfilesPage","DevicePage"}) {
                var page=(Grid)window.FindName(name);
                bool stacked=width<760;
                if(page.RowDefinitions.Count==0) {
                    page.RowDefinitions.Add(new RowDefinition { Height=GridLength.Auto });
                    page.RowDefinitions.Add(new RowDefinition { Height=GridLength.Auto });
                }
                page.ColumnDefinitions[1].Width=new GridLength(stacked?0:24);
                page.ColumnDefinitions[2].Width=stacked?new GridLength(0):new GridLength(1,GridUnitType.Star);
                var right=(FrameworkElement)page.Children[1];
                Grid.SetRow(right,stacked?1:0);Grid.SetColumn(right,stacked?0:2);
                right.Margin=new Thickness(0,stacked?16:0,0,0);
                page.MinHeight=600;
            }
        }
    }
}
