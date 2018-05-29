using System;
using System.Collections.Generic;
using Venz.Extensions;
using Venz.Images;
using Venz.UI.Animation;
using Venz.UI.Xaml;
using VkPhotos.Model;
using Windows.Foundation;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace VkPhotos.View
{
    public sealed partial class PictureViewContent: OverlaidContent
    {
        private ContentContext Context;
        private Action PropertiesBackKeyAction;

        private FrameworkElement SourceControl;
        private IPictureStreamReadinessInformation PictureReadiness;
        private IRandomAccessStream PictureStream;



        public PictureViewContent(FrameworkElement sourceControl, Picture preview, Photo photo)
        {
            InitializeComponent();
            PropertiesBackKeyAction = new Action(() => OnCloseDetailsButtonClicked(this, new RoutedEventArgs()));
            SourceControl = sourceControl;
            SourceControl.Opacity = 0;

            Context = new ContentContext(photo);
            if (photo.Original is IPictureStreamReadinessInformation)
            {
                PictureReadiness = (IPictureStreamReadinessInformation)photo.Original;
                PictureReadiness.Ready += OnPictureReady;
            }

            PicturePreviewControl.SetValue(PictureLoader.SourceProperty, preview);
            PictureControl.SetValue(PictureLoader.SourceProperty, photo.Original);
            Transition = new ContinuumTransition();
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            var imageSize = Context.PictureSize.GetUniformedSize(args.NewSize);
            PicturePreviewControl.Width = imageSize.Width;
            PicturePreviewControl.Height = imageSize.Height;
            PictureControl.Width = imageSize.Width;
            PictureControl.Height = imageSize.Height;
            DetailsControl.Width = (args.NewSize.Width > 320) ? 320 : args.NewSize.Width;
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            SourceControl.Opacity = 1;
        }

        protected override void OnClosing()
        {
            base.OnClosing();
            PictureControl.Opacity = 0;
            BackgroundControl.Opacity = 0;
            Venz.UI.Xaml.Page.Active.RemoveBackKeyAction(PropertiesBackKeyAction);
            if (PictureReadiness != null)
                PictureReadiness.Ready -= OnPictureReady;
        }

        protected override void OnDisplayed()
        {
            base.OnDisplayed();
            PictureControl.Opacity = 1;
        }

        private async void OnSaveButtonClicked(Object sender, RoutedEventArgs args)
        {
            var filePicker = new FileSavePicker();
            filePicker.FileTypeChoices.Add("Picture", new List<String>() { ".jpg", ".png" });
            filePicker.DefaultFileExtension = ".jpg";
            filePicker.SuggestedFileName = Context.SuggestedFileName;
            var file = await filePicker.PickSaveFileAsync();
            if (file != null)
            {
                using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                {
                    PictureStream.Seek(0);
                    await RandomAccessStream.CopyAsync(PictureStream, stream);
                }
            }
        }

        private void OnPropertiesButtonClicked(Object sender, RoutedEventArgs e)
        {
            CommandBar.Visibility = Visibility.Collapsed;
            DetailsControl.Opacity = 1;
            DetailsControl.SetValue(Canvas.ZIndexProperty, 1);
            Venz.UI.Xaml.Page.Active.AddBackKeyAction(PropertiesBackKeyAction);
            var storyboard = CreateDetailsShowingTransition(DetailsControl);
            storyboard.Begin();
        }

        private void OnCloseDetailsButtonClicked(Object sender, RoutedEventArgs args)
        {
            Venz.UI.Xaml.Page.Active.RemoveBackKeyAction(PropertiesBackKeyAction);
            var storyboard = CreateDetailsHidingTransition(DetailsControl);
            storyboard.Begin();
            storyboard.Completed += (storyboardSender, storyboardArgs) =>
            {
                CommandBar.Visibility = Visibility.Visible;
                DetailsControl.SetValue(Canvas.ZIndexProperty, 0);
                DetailsControl.Opacity = 0;
                storyboard.Stop();
            };
        }

        private async void OnPictureReady(IPictureStreamReadinessInformation sender, IRandomAccessStream stream)
        {
            PictureStream = stream;
            await App.Dispatcher.RunAsync(() => SaveButton.IsEnabled = true);
        }

        private void OnCommandBarOpening(Object sender, Object e) => ((CommandBar)sender).Opacity = 1;

        private void OnCommandBarClosing(Object sender, Object e) => ((CommandBar)sender).Opacity = 0.65;

        private void OnPicturesContainerControlManipulationDelta(Object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs args)
        {
            if (DetailsControl.Opacity == 1)
                return;

            var positionY = args.Position.Y + args.Cumulative.Translation.Y;
            if ((positionY < 0) || (positionY > Window.Current.Bounds.Height))
            {
                args.Complete();
                return;
            }

            var transform = (CompositeTransform)((FrameworkElement)sender).RenderTransform;
            transform.TranslateY += args.Delta.Translation.Y;
            var opacity = 1 - Math.Abs(transform.TranslateY) / 170;
            if (opacity < 0)
                opacity = 0;
            BackgroundControl.Opacity = opacity;
            CommandBar.Visibility = Visibility.Collapsed;
        }

        private void OnPicturesContainerControlManipulationCompleted(Object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs args)
        {
            if (DetailsControl.Opacity == 1)
                return;

            if (BackgroundControl.Opacity < 0.5)
            {
                Close(skipAnimation: false);
            }
            else
            {
                var storyboard = CreateSlideBackTransition((FrameworkElement)sender, BackgroundControl);
                storyboard.Begin();
                CommandBar.Visibility = Visibility.Visible;
            }
        }

        private static Storyboard CreateSlideBackTransition(FrameworkElement target, FrameworkElement background)
        {
            var translateY = new TranslateY(target) { From = ((CompositeTransform)target.RenderTransform).TranslateY, Duration = 250, EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 8 } };
            var opacity = new Opacity(background) { From = background.Opacity, To = 1, Duration = 250, EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 8 } };
            var storyboard = new Storyboard();
            storyboard.Children.Add(translateY.GetTimeline());
            storyboard.Children.Add(opacity.GetTimeline());
            return storyboard;
        }

        private static Storyboard CreateDetailsShowingTransition(FrameworkElement target)
        {
            var translateX = new TranslateX(target) { From = target.ActualWidth, Duration = 500, EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 8 } };
            var storyboard = new Storyboard();
            storyboard.Children.Add(translateX.GetTimeline());
            return storyboard;
        }

        private static Storyboard CreateDetailsHidingTransition(FrameworkElement target)
        {
            var translateX = new TranslateX(target) { To = target.ActualWidth, Duration = 500, EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 8 } };
            var storyboard = new Storyboard();
            storyboard.Children.Add(translateX.GetTimeline());
            return storyboard;
        }

        private class ContinuumTransition: IOverlaidContentTransition
        {
            public Storyboard CreateShowing(OverlaidContent target)
            {
                var content = (PictureViewContent)target;
                var duration = 250;
                var exponentialEase = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 5 };
                var transform = content.PicturePreviewControl.GetTransformTo(content.SourceControl);

                var translateX = new TranslateX(content.PicturePreviewControl) { From = transform.TranslateX / 2, Duration = duration, EasingFunction = exponentialEase };
                var translateY = new TranslateY(content.PicturePreviewControl) { From = transform.TranslateY / 2, Duration = duration, EasingFunction = exponentialEase };
                var scaleX = new ScaleX(content.PicturePreviewControl) { From = transform.ScaleX + (1 - transform.ScaleX) / 2, To = 1, Duration = duration, EasingFunction = exponentialEase };
                var scaleY = new ScaleY(content.PicturePreviewControl) { From = transform.ScaleY + (1 - transform.ScaleY) / 2, To = 1, Duration = duration, EasingFunction = exponentialEase };

                var storyboard = new Storyboard();
                storyboard.Children.Add(translateX.GetTimeline());
                storyboard.Children.Add(translateY.GetTimeline());
                storyboard.Children.Add(scaleX.GetTimeline());
                storyboard.Children.Add(scaleY.GetTimeline());
                return storyboard;
            }

            public Storyboard CreateHiding(OverlaidContent target)
            {
                var content = (PictureViewContent)target;
                var duration = 250;
                var exponentialEase = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 5 };
                var transform = content.PicturePreviewControl.GetTransformTo(content.SourceControl);
                ApplyUniformToFillRule(content.PicturePreviewControl.RenderSize, content.SourceControl.RenderSize, transform);

                var translateX = new TranslateX(content.PicturePreviewControl) { To = transform.TranslateX, Duration = duration, EasingFunction = exponentialEase };
                var translateY = new TranslateY(content.PicturePreviewControl) { To = transform.TranslateY, Duration = duration, EasingFunction = exponentialEase };
                var scaleX = new ScaleX(content.PicturePreviewControl) { From = 1, To = transform.ScaleX, Duration = duration, EasingFunction = exponentialEase };
                var scaleY = new ScaleY(content.PicturePreviewControl) { From = 1, To = transform.ScaleY, Duration = duration, EasingFunction = exponentialEase };
                var background = new ValueSetterAnimation(content.LayoutControl, nameof(Background)) { Value = new SolidColorBrush(Colors.Transparent) };

                var storyboard = new Storyboard();
                storyboard.Children.Add(translateX.GetTimeline());
                storyboard.Children.Add(translateY.GetTimeline());
                storyboard.Children.Add(scaleX.GetTimeline());
                storyboard.Children.Add(scaleY.GetTimeline());
                storyboard.Children.Add(background.GetTimeline());
                return storyboard;
            }

            private void ApplyUniformToFillRule(Size sourceSize, Size targetSize, CompositeTransform transform)
            {
                if (sourceSize.Width > sourceSize.Height)
                {
                    var newScaleX = targetSize.Height / sourceSize.Height;
                    transform.TranslateX -= (sourceSize.Width * newScaleX - sourceSize.Width * transform.ScaleX) / 2;
                    transform.ScaleX = newScaleX;
                }
                else if (sourceSize.Height > sourceSize.Width)
                {
                    var newScaleY = targetSize.Width / sourceSize.Width;
                    transform.TranslateY -= (sourceSize.Height * newScaleY - sourceSize.Height * transform.ScaleY) / 2;
                    transform.ScaleY = newScaleY;
                }
            }
        }

        private class ContentContext
        {
            private Photo Photo;

            public Uri Author => Photo.Owner.GetUri();
            public String Date => Photo.Date.ToLocalTime().ToString("dd MMMM yyyy, HH:mm");
            public String Text => Photo.Text;
            public Size PictureSize => Photo.Size;
            public String SuggestedFileName => $"{Constants.ApplicationTitle}_{Photo.Owner}_{Photo.Id}";
            public Visibility TextVisibility => !String.IsNullOrWhiteSpace(Text) ? Visibility.Visible : Visibility.Collapsed;

            public ContentContext(Photo photo)
            {
                Photo = photo;
            }
        }
    }
}
