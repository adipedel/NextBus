using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace NextBus.Mobile.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            // תפיסת מופע החלון של Windows
            var mauiWindow = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
            if (mauiWindow?.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
            {
                var handle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                var windowId = Win32Interop.GetWindowIdFromWindow(handle);
                var appWindow = AppWindow.GetFromWindowId(windowId);

                if (appWindow?.TitleBar != null)
                {
                    // מפריד לחלוטין את ה-TitleBar של Windows מתוכן האפליקציה
                    appWindow.TitleBar.ExtendsContentIntoTitleBar = false;
                    appWindow.Title = "NextBus";
                }
            }
        }
    }
}