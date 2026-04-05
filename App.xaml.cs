namespace Kishimn
{
    /// <summary>
    /// 既定の <see cref="Application"/> クラスを提供し、アプリケーションのライフサイクルを管理します。
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// メインウィンドウを保持します。
        /// </summary>
        private MainWindow? mainWindow;

        /// <summary>
        /// <see cref="App"/> の新しいインスタンスを初期化します。
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 現在のメインウィンドウを取得します。
        /// </summary>
        public MainWindow MainWindow => mainWindow ?? throw new InvalidOperationException("メインウィンドウはまだ初期化されていません。");

        /// <summary>
        /// アプリケーション起動時にメインウィンドウを生成して表示します。
        /// </summary>
        /// <param name="args">起動時引数を保持するイベント引数。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            mainWindow ??= new MainWindow();
            mainWindow.Activate();
        }
    }
}
