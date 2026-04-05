using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;

namespace Kishimn.Views
{
    /// <summary>
    /// アプリケーションの最上位ウィンドウを表します。
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        /// <summary>
        /// <see cref="MainWindow"/> の新しいインスタンスを初期化します。
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Title = "Kishimn";
            SystemBackdrop = new MicaBackdrop();

            AppWindow appWindow = GetAppWindow();
            appWindow.Title = "Kishimn";
        }

        /// <summary>
        /// 現在のウィンドウに紐づく <see cref="AppWindow"/> を取得します。
        /// </summary>
        /// <returns>ウィンドウハンドルに対応する <see cref="AppWindow"/>。</returns>
        private AppWindow GetAppWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(windowId);
        }
    }
}
