using System;
using System.Collections.Generic;
using System.Linq;
using Venz.UI.Animation;
using VkPhotos.Model.Map;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace VkPhotos.View.Controls
{
    public sealed partial class MapCluster: UserControl, IMapClusterElement
    {
        private Lazy<Storyboard> ShowAnimation;
        private Lazy<Storyboard> InteractionIndicationAnimation;

        public IEnumerable<Object> Objects { get; private set; }



        public MapCluster()
        {
            InitializeComponent();
            ShowAnimation = new Lazy<Storyboard>(CreateShowAnimation);
            InteractionIndicationAnimation = new Lazy<Storyboard>(CreateInteractionIndicationAnimation);
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
            InteractionIndicationAnimation.Value.Begin();
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            InteractionIndicationAnimation.Value.Stop();
        }

        public void SetContext(IReadOnlyCollection<GeoObject> context)
        {
            Objects = context.Select(a => a.Value).ToList();
            AmountControl.Text = context.Count.ToString();
        }

        public void Show()
        {
            if (Opacity == 0)
            {
                IsHitTestVisible = true;
                ShowAnimation.Value.Begin();
            }
        }

        public void Hide()
        {
            ShowAnimation.Value.Stop();
            IsHitTestVisible = false;
        }

        private Storyboard CreateShowAnimation()
        {
            var storyboard = new Storyboard();
            storyboard.Children.Add(new Opacity(this) { To = 1, Duration = 500 }.GetTimeline());
            return storyboard;
        }

        private Storyboard CreateInteractionIndicationAnimation()
        {
            var scaleX = new ScaleX(LayoutControl) { From = 1, To = 1.3, Duration = 250, EasingFunction = new BackEase() { EasingMode = EasingMode.EaseOut } };
            var scaleY = new ScaleY(LayoutControl) { From = 1, To = 1.3, Duration = 250, EasingFunction = new BackEase() { EasingMode = EasingMode.EaseOut } };

            var storyboard = new Storyboard();
            storyboard.Children.Add(scaleX.GetTimeline());
            storyboard.Children.Add(scaleY.GetTimeline());
            return storyboard;
        }
    }
}
