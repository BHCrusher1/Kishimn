using Microsoft.UI.Xaml.Navigation;

namespace Kishimn
{
    /// <summary>
    /// アプリケーション全体の起動とナビゲーションを管理するクラス。
    /// </summary>
    public partial class App : Application
    {
        // ファイルピッカー初期化などで利用するメインウィンドウ参照を保持する。
        public static Window MainWindow { get; private set; } = Window.Current;

        // アプリケーションの初期化を行う。
        public App()
        {
            InitializeComponent();
        }

        // アプリ起動時にルートフレームを作成し、メインページへ遷移する。
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            MainWindow ??= new Window();

            if (MainWindow.Content is not Frame rootFrame)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                MainWindow.Content = rootFrame;
            }

            _ = rootFrame.Navigate(typeof(MainPage), e.Arguments);
            MainWindow.Activate();
        }

        // ページ遷移失敗時に例外を送出して異常を検知する。
        private static void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
