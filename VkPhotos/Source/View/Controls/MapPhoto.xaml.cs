using System;
using Venz.Images;
using Venz.UI.Animation;
using VkPhotos.Model;
using VkPhotos.Model.Map;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace VkPhotos.View.Controls
{
    public sealed partial class MapPhoto: UserControl, IMapObjectElement
    {
        private Photo Context;
        private Lazy<Storyboard> ShowAnimation;
        private Lazy<Storyboard> InteractionIndicationAnimation;

        public UInt32 Id => (Context == null) ? 0 : Context.Id;
        public Point NormalizedAnchorPoint => new Point(0.5, 1);



        public MapPhoto()
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

        public void SetContext(Object context)
        {
            if (!(context is Photo))
                throw new ArgumentException();

            if (Context == context)
                return;

            Context = (Photo)context;
            PictureLoader.SetSource(ImageControl, Context.Preview);
        }

        public Object GetContext() => Context;

        public FrameworkElement GetImageControl() => ImageControl;

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