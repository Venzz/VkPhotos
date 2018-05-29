using System;
using Venz.UI.Xaml;
using VkPhotos.ViewModel;
using Windows.UI.Xaml.Controls;

namespace VkPhotos.View
{
    public sealed partial class SettingsPage: Venz.UI.Xaml.Page
    {
        private SettingsContext Context = new SettingsContext();

        public SettingsPage()
        {
            InitializeComponent();
            DataContext = Context;
        }

        protected override async void SetState(FrameNavigation.Parameter navigationParameter, FrameNavigation.Parameter stateParameter)
        {
            base.SetState(navigationParameter, stateParameter);
            await Context.InitializeAsync();
        }

        private void OnPastPeriodToggled(Object sender, Windows.UI.Xaml.RoutedEventArgs args)
        {
            if (((ToggleSwitch)sender).IsOn)
                Context.EnablePastPeriod();
            else
                Context.DisablePastPeriod();
        }

        private void OnPeriodToggled(Object sender, Windows.UI.Xaml.RoutedEventArgs args)
        {
            if (((ToggleSwitch)sender).IsOn)
                Context.EnablePeriod();
            else
                Context.DisablePeriod();
        }

        private void OnPastPeriodChanged(Object sender, SelectionChangedEventArgs args) => Context.SetPastPeriod(args.AddedItems[0]);

        private async void OnClearCacheButtonClicked(Object sender, Windows.UI.Xaml.RoutedEventArgs args) => await Context.ClearCacheAsync();
    }
}