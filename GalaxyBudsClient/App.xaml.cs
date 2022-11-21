using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Scripting.Experiment;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;
using Application = Avalonia.Application;

namespace GalaxyBudsClient
{
    public class App : Application
    {
        public override void Initialize()
        {
            DataContext = this;
            
            AvaloniaXamlLoader.Load(this);
            
            if (Loc.IsTranslatorModeEnabled())
            {
                SettingsProvider.Instance.Locale = Locales.custom;
            }
            
            ThemeUtils.Reload();
            Loc.Load();
            
            TrayManager.Init();
            MediaKeyRemoteImpl.Init();
            DeviceMessageCache.Init();
            UpdateManager.Init();
            ExperimentManager.Init();
           
            Log.Information($"Translator mode file location: {Loc.GetTranslatorModeFile()}");

            ScriptManager.Instance.RegisterUserHooks();
        }
        
        public override void OnFrameworkInitializationCompleted()
        {
            TrayManager.Init();
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = MainWindow.Instance;
                desktop.Exit += (sender, args) =>
                {
                    SettingsProvider.Instance.FirstLaunch = false;
                };
            }
            
            if (Loc.IsTranslatorModeEnabled())
            {
                DialogLauncher.ShowTranslatorTools();
            }
            
            base.OnFrameworkInitializationCompleted();
        }

        public void RestartApp(AbstractPage.Pages target)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow.Instance.DisableApplicationExit = true;
                MainWindow.Instance.OverrideMinimizeTray = false;
                MainWindow.Instance.Close();
                MainWindow.Kill();

                ThemeUtils.Reload();

                desktop.MainWindow = MainWindow.Instance;
                desktop.MainWindow.Show();
                
                MainWindow.Instance.Pager.SwitchPage(target);
                
                /* Restore crucial information */
                SPPMessageHandler.Instance.DispatchEvent(DeviceMessageCache.Instance.ExtendedStatusUpdate);
                SPPMessageHandler.Instance.DispatchEvent(DeviceMessageCache.Instance.StatusUpdate);
            }
        }

        public event Action? TrayIconClicked;

        public static readonly StyledProperty<NativeMenu> TrayMenuProperty =
            AvaloniaProperty.Register<App, NativeMenu>(nameof(TrayMenu),
                defaultBindingMode: BindingMode.OneWay, defaultValue: new NativeMenu());
        public NativeMenu TrayMenu => GetValue(TrayMenuProperty);

        private void TrayIcon_OnClicked(object? sender, EventArgs e)
        {
            TrayIconClicked?.Invoke();
        }
    }
}
