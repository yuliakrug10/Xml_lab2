using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace Xml_lab2
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if WINDOWS
            builder.ConfigureLifecycleEvents(events =>
            {
                events.AddWindows(windows =>
                {
                    windows.OnWindowCreated(window =>
                    {
                        window.AppWindow.Closing += async (sender, args) =>
                        {
                            MainPage page = Shell.Current.CurrentPage as MainPage;
                            if (page == null || page.IsExiting)
                            {
                                return;
                            }

                            args.Cancel = true;
                            await page.AskExitAsync();
                        };
                    });
                });
            });
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
