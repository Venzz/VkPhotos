using System;
using System.Threading.Tasks;
using Venz.Core;
using Venz.Telemetry;
using Venz.Telemetry.Yandex;
using Venz.UI.Xaml;
using VkPhotos.Model;
using VkPhotos.View;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Media;

namespace VkPhotos
{
    public sealed partial class App: Application
    {
        public static Diagnostics Diagnostics { get; } = new Diagnostics("App");
        public static Settings Settings { get; } = new Settings();
        public static ApplicationModel Model { get; } = new ApplicationModel();



        public App()
        {
            InitializeComponent();
        }

        protected override Task StartAsync(Frame frame, ActivationKind activationKind, ApplicationExecutionState previousExecutionState, PrelaunchStage prelaunchStage)
        {
            SetupTelemetry();
            return Task.CompletedTask;
        }

        protected override Task InitializeAsync() => Task.Run(() =>
        {
            var modelInitializationTask = Model.InitializeAsync();
            return Task.WhenAll(modelInitializationTask);
        });

        protected override Task OnManuallyActivatedAsync(Frame frame, Boolean newInstance, PrelaunchStage prelaunchStage, String args)
        {
            frame.Navigation.Navigate(typeof(MapPage), new FrameNavigation.Parameter("default"));
            return Task.CompletedTask;
        }

        protected override async Task OnUriActivatedAsync(Frame frame, Boolean newInstance, PrelaunchStage prelaunchStage, ProtocolActivatedEventArgs args)
        {
            var activation = Activation.Create(args);
            if (activation.SharedLinkParameter != null)
                Model.MapView.SetSharedLinkSettings(activation.SharedLinkParameter);

            frame.Navigation.Navigate(typeof(MapPage), activation.NavigationParameter);
            if (activation.Type == ActivationType.SharedLink)
            {
                if (activation.SharedLinkParameter != null)
                {
                    App.Diagnostics.Telemetry.Log("Shared link", "status", "opened");
                    if (!newInstance)
                        await SharedLink.TryApplySettingsAsync();
                }
                else
                {
                    App.Diagnostics.Telemetry.Log("Shared link", "status", "failed");
                    await SharedLink.ShowLinkCreationFailedDialogAsync();
                }
            }
        }

        private void SetupTelemetry()
        {
            #if DEBUG
            var debugTelemetry = System.Diagnostics.Debugger.IsAttached ? (ITelemetryService)new DebugTelemetryService() : new ToastTelemetryService();
            var yandexTelemetry = new YandexMetricaTelemetryService(PrivateData.YandexMetrikaApiKey, SystemInfo.ApplicationPackageVersion.ToString());
            yandexTelemetry.Start();
            Diagnostics.Info.Add(debugTelemetry);
            Diagnostics.Debug.Add(debugTelemetry);
            Diagnostics.Debug.Add(yandexTelemetry);
            Diagnostics.Error.Add(yandexTelemetry);
            #else
            var yandexTelemetry = new YandexMetricaTelemetryService(PrivateData.YandexMetrikaApiKey, SystemInfo.ApplicationPackageVersion.ToString());
            yandexTelemetry.Start();
            Diagnostics.Debug.Add(yandexTelemetry);
            Diagnostics.Error.Add(yandexTelemetry);
            #endif
        }

        public static class Theme
        {
            public static SolidColorBrush AccentBrush => (SolidColorBrush)App.Current.Resources["AppAccentBrush"];
        }
    }
}