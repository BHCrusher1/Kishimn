namespace Kishimn
{
    /// <summary>
    /// 既定の <see cref="Application"/> クラスを補完するアプリケーション固有の動作を提供します。
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// アプリケーションで表示するメイン ウィンドウを保持します。
        /// </summary>
        private Window? window;

        /// <summary>
        /// アプリケーションのシングルトン インスタンスを初期化します。
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// エンドユーザーによる通常起動時に呼び出されます。
        /// </summary>
        /// <param name="e">起動要求に関する情報です。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            window ??= new Window();
            window.Activate();
        }
    }
}
