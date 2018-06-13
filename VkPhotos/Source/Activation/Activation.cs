using Venz.UI.Xaml;
using VkPhotos.Model;
using Windows.ApplicationModel.Activation;

namespace VkPhotos
{
    public class Activation
    {
        public ActivationType Type { get; private set; } = ActivationType.Default;
        public MapViewSettings SharedLinkParameter { get; private set; }
        public FrameNavigation.Parameter NavigationParameter { get; private set; } = new FrameNavigation.Parameter("default");

        private Activation() { }

        public static Activation Create(IActivatedEventArgs args)
        {
            if (!(args is ProtocolActivatedEventArgs))
                return new Activation();

            var uri = ((ProtocolActivatedEventArgs)args).Uri;
            var activation = new Activation() { Type = ActivationType.SharedLink };
            activation.SharedLinkParameter = SharedLink.Parse(uri);
            if (activation.SharedLinkParameter != null)
                activation.NavigationParameter = new FrameNavigation.Parameter("share");
            return activation;
        }
    }
}
