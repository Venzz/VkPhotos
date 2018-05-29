using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

namespace VkPhotos.View.Controls
{
    public sealed partial class MapSettings: UserControl
    {
        private MapControl Context;

        public MapSettings()
        {
            InitializeComponent();
        }

        public void SetContext(MapControl context)
        {
            Context = context;
            OnMapStyleChanged(context.Style);
        }

        private void OnMapStyleChanged(MapStyle value)
        {
            switch (value)
            {
                case MapStyle.Aerial:
                    AerialControl.BorderBrush = App.Theme.AccentBrush;
                    RoadControl.BorderBrush = null;
                    break;
                case MapStyle.Road:
                    AerialControl.BorderBrush = null;
                    RoadControl.BorderBrush = App.Theme.AccentBrush;
                    break;
            }
        }

        private void OnAerialTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Context.Style = MapStyle.Aerial;
            OnMapStyleChanged(MapStyle.Aerial);
        }

        private void OnRoadTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Context.Style = MapStyle.Road;
            OnMapStyleChanged(MapStyle.Road);
        }
    }
}
