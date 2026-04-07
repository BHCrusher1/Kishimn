using Kishimn.Models;
using Kishimn.Services;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Kishimn.Views
{
    /// <summary>
    /// FFmpeg の変換設定画面を提供します。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// コンテナ選択肢を保持します。
        /// </summary>
        private readonly IReadOnlyList<SelectionOption<ContainerKind>> containerOptions =
        [
            new("MP4", ContainerKind.Mp4),
            new("MKV", ContainerKind.Mkv),
        ];

        /// <summary>
        /// 動画エンコーダー選択肢を保持します。
        /// </summary>
        private readonly IReadOnlyList<SelectionOption<VideoEncoderProfile>> videoEncoderOptions =
        [
            new("H264 (CPU)", new("H264 (CPU)", "libx264", "crf", 0, 51, 23)),
            new("H264 (AMD AMF)", new("H264 (AMD AMF)", "h264_amf", "qp", 0, 51, 23)),
            new("H264 (Intel QSV)", new("H264 (Intel QSV)", "h264_qsv", "global_quality", 1, 51, 23)),
            new("H264 (NVIDIA NVENC)", new("H264 (NVIDIA NVENC)", "h264_nvenc", "cq", 0, 51, 23)),
            new("H265 (CPU)", new("H265 (CPU)", "libx265", "crf", 0, 51, 28, true)),
            new("H265 (AMD AMF)", new("H265 (AMD AMF)", "hevc_amf", "qp", 0, 51, 28, true)),
            new("H265 (Intel QSV)", new("H265 (Intel QSV)", "hevc_qsv", "global_quality", 1, 51, 28, true)),
            new("H265 (NVIDIA NVENC)", new("H265 (NVIDIA NVENC)", "hevc_nvenc", "cq", 0, 51, 28, true)),
            new("VP9 (CPU)", new("VP9 (CPU)", "libvpx-vp9", "crf", 0, 63, 31)),
            new("VP9 (Intel QSV)", new("VP9 (Intel QSV)", "vp9_qsv", "global_quality", 1, 63, 31)),
            new("AV1 (CPU libaom-av1)", new("AV1 (CPU libaom-av1)", "libaom-av1", "crf", 0, 63, 32)),
            new("AV1 (AMD AMF)", new("AV1 (AMD AMF)", "av1_amf", "qp", 0, 255, 34)),
            new("AV1 (Intel QSV)", new("AV1 (Intel QSV)", "av1_qsv", "global_quality", 1, 63, 34)),
            new("AV1 (NVIDIA NVENC)", new("AV1 (NVIDIA NVENC)", "av1_nvenc", "cq", 0, 63, 34)),
        ];

        /// <summary>
        /// フレームレート選択肢を保持します。
        /// </summary>
        private readonly IReadOnlyList<FrameRateOption> frameRateOptions =
        [
            new("オリジナル", null),
            new("1", "1"),
            new("5", "5"),
            new("10", "10"),
            new("15", "15"),
            new("23.976", "24000/1001"),
            new("24", "24"),
            new("25", "25"),
            new("29.97", "30000/1001"),
            new("30", "30"),
            new("50", "50"),
            new("59.94", "60000/1001"),
            new("60", "60"),
            new("72", "72"),
            new("75", "75"),
            new("90", "90"),
            new("120", "120"),
            new("144", "144"),
        ];

        /// <summary>
        /// レート指定選択肢を保持します。
        /// </summary>
        private readonly IReadOnlyList<SelectionOption<RateModeKind>> rateModeOptions =
        [
            new("品質", RateModeKind.Quality),
            new("ビットレート", RateModeKind.Bitrate),
        ];

        /// <summary>
        /// 音声選択肢を保持します。
        /// </summary>
        private readonly IReadOnlyList<SelectionOption<AudioOptionKind>> audioOptions =
        [
            new("音声無し(-an)", AudioOptionKind.None),
            new("コピー(-c:a copy)", AudioOptionKind.Copy),
            new("音量調整 (YouTube)", AudioOptionKind.LoudnormYoutube),
            new("音量調整 (ARIB TR-B32)", AudioOptionKind.LoudnormArib),
        ];

        /// <summary>
        /// イベント再入を抑制するかどうかを保持します。
        /// </summary>
        private bool suppressEvents;

        /// <summary>
        /// 使用する FFmpeg 実行ファイルのフルパスを保持します。
        /// </summary>
        private string? ffmpegExecutablePath;

        /// <summary>
        /// <see cref="MainPage"/> の新しいインスタンスを初期化します。
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            InitializeSelections();
            RefreshFfmpegExecutablePath();
            ApplyDefaults();
            UpdateCommandLine();
        }

        /// <summary>
        /// 各コンボボックスに選択肢を設定します。
        /// </summary>
        private void InitializeSelections()
        {
            ContainerComboBox.DisplayMemberPath = nameof(SelectionOption<>.Label);
            ContainerComboBox.ItemsSource = containerOptions;

            VideoEncoderComboBox.DisplayMemberPath = nameof(SelectionOption<>.Label);
            VideoEncoderComboBox.ItemsSource = videoEncoderOptions;

            FrameRateComboBox.DisplayMemberPath = nameof(FrameRateOption.Label);
            FrameRateComboBox.ItemsSource = frameRateOptions;

            RateModeComboBox.DisplayMemberPath = nameof(SelectionOption<>.Label);
            RateModeComboBox.ItemsSource = rateModeOptions;

            AudioOptionComboBox.DisplayMemberPath = nameof(SelectionOption<>.Label);
            AudioOptionComboBox.ItemsSource = audioOptions;
        }

        /// <summary>
        /// 既定値を画面に適用します。
        /// </summary>
        private void ApplyDefaults()
        {
            suppressEvents = true;

            try
            {
                SelectOption(ContainerComboBox, containerOptions, ContainerKind.Mp4);
                SelectEncoder("H264 (CPU)");
                FrameRateComboBox.SelectedIndex = 0;
                SelectOption(RateModeComboBox, rateModeOptions, RateModeKind.Quality);
                SelectOption(AudioOptionComboBox, audioOptions, AudioOptionKind.Copy);

                FastStartCheckBox.IsChecked = true;
                BitrateTextBox.Text = "6000";

                ApplyEncoderQualityRange(GetSelectedEncoder());
                UpdateRateModePanels();
                UpdateFastStartState();
            }
            finally
            {
                suppressEvents = false;
            }
        }

        /// <summary>
        /// 変換元ファイルを参照して設定します。
        /// </summary>
        private async void OnBrowseInputClick(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new();
            picker.FileTypeFilter.Add("*");
            InitializePicker(picker);

            StorageFile? file = await picker.PickSingleFileAsync();
            if (file is null)
            {
                return;
            }

            InputPathTextBox.Text = file.Path;

            if (string.IsNullOrWhiteSpace(OutputPathTextBox.Text))
            {
                string extension = GetContainerExtension(GetSelectedContainer());
                string candidatePath = Path.Combine(
                    Path.GetDirectoryName(file.Path) ?? string.Empty,
                    $"{Path.GetFileNameWithoutExtension(file.Path)}_out{extension}");
                OutputPathTextBox.Text = candidatePath;
            }

            UpdateCommandLine();
        }

        /// <summary>
        /// 保存先ファイルを参照して設定します。
        /// </summary>
        private async void OnBrowseOutputClick(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new();
            InitializePicker(picker);

            picker.SuggestedFileName =
                !string.IsNullOrWhiteSpace(InputPathTextBox.Text)
                    ? Path.GetFileNameWithoutExtension(InputPathTextBox.Text)
                    : "output";

            ContainerKind container = GetSelectedContainer();
            if (container == ContainerKind.Mp4)
            {
                picker.FileTypeChoices.Add("MP4", [".mp4"]);
                picker.DefaultFileExtension = ".mp4";
            }
            else
            {
                picker.FileTypeChoices.Add("MKV", [".mkv"]);
                picker.DefaultFileExtension = ".mkv";
            }

            StorageFile? file = await picker.PickSaveFileAsync();
            if (file is null)
            {
                return;
            }

            OutputPathTextBox.Text = file.Path;
            UpdateCommandLine();
        }

        /// <summary>
        /// コンテナ変更時に関連設定を更新します。
        /// </summary>
        private void OnContainerChanged(object sender, SelectionChangedEventArgs e)
        {
            if (suppressEvents)
            {
                return;
            }

            UpdateFastStartState();
            UpdateOutputPathExtension();
            UpdateCommandLine();
        }

        /// <summary>
        /// エンコーダー変更時に品質レンジを更新します。
        /// </summary>
        private void OnVideoEncoderChanged(object sender, SelectionChangedEventArgs e)
        {
            if (suppressEvents)
            {
                return;
            }

            ApplyEncoderQualityRange(GetSelectedEncoder());
            UpdateCommandLine();
        }

        /// <summary>
        /// レート指定変更時に入力 UI を切り替えます。
        /// </summary>
        private void OnRateModeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (suppressEvents)
            {
                return;
            }

            UpdateRateModePanels();
            UpdateCommandLine();
        }

        /// <summary>
        /// 品質スライダー変更時に表示を更新します。
        /// </summary>
        private void OnQualitySettingChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (suppressEvents)
            {
                return;
            }

            UpdateQualityText();
            UpdateCommandLine();
        }

        /// <summary>
        /// 画面設定変更時にコマンドライン表示を更新します。
        /// </summary>
        private void OnAnySettingChanged(object sender, RoutedEventArgs e)
        {
            if (suppressEvents)
            {
                return;
            }

            UpdateCommandLine();
        }

        /// <summary>
        /// 入出力以外の設定を初期化します。
        /// </summary>
        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            string inputPath = InputPathTextBox.Text;
            string outputPath = OutputPathTextBox.Text;

            ApplyDefaults();
            RefreshFfmpegExecutablePath();

            InputPathTextBox.Text = inputPath;
            OutputPathTextBox.Text = outputPath;

            UpdateCommandLine();
        }

        /// <summary>
        /// FFmpeg を外部プロセスとして起動します。
        /// </summary>
        private async void OnExecuteClick(object sender, RoutedEventArgs e)
        {
            FfmpegSettings settings = BuildSettings();
            if (string.IsNullOrWhiteSpace(settings.InputPath) || string.IsNullOrWhiteSpace(settings.OutputPath))
            {
                await ShowMessageAsync("入力不足", "変換元ファイルと保存先ファイルを設定してください。").ConfigureAwait(true);
                return;
            }

            if (string.IsNullOrWhiteSpace(ffmpegExecutablePath))
            {
                await ShowMessageAsync("実行失敗", "ffmpeg.exe が見つかりません。アプリ実行フォルダか PATH を確認してください。").ConfigureAwait(true);
                return;
            }

            ProcessStartInfo startInfo = new()
            {
                FileName = ffmpegExecutablePath,
                Arguments = FfmpegCommandBuilder.BuildArguments(settings),
                UseShellExecute = true,
                WorkingDirectory = Directory.GetCurrentDirectory(),
            };

            try
            {
                Process? process = Process.Start(startInfo);
                if (process is null)
                {
                    await ShowMessageAsync("実行失敗", "ffmpeg.exe を起動できませんでした。").ConfigureAwait(true);
                }
            }
            catch (Win32Exception ex)
            {
                await ShowMessageAsync("実行失敗", $"ffmpeg.exe の起動に失敗しました。{Environment.NewLine}{ex.Message}").ConfigureAwait(true);
            }
            catch (InvalidOperationException ex)
            {
                await ShowMessageAsync("実行失敗", $"ffmpeg.exe の起動に失敗しました。{Environment.NewLine}{ex.Message}").ConfigureAwait(true);
            }
        }

        /// <summary>
        /// 実行時に使用する設定を構築します。
        /// </summary>
        /// <returns>実行用の設定。</returns>
        private FfmpegSettings BuildSettings()
        {
            return new FfmpegSettings
            {
                InputPath = InputPathTextBox.Text.Trim(),
                OutputPath = OutputPathTextBox.Text.Trim(),
                Container = GetSelectedContainer(),
                UseFastStart = FastStartCheckBox.IsChecked == true,
                Encoder = GetSelectedEncoder(),
                FrameRate = GetSelectedFrameRate(),
                RateMode = GetSelectedRateMode(),
                Quality = (int)Math.Round(QualitySlider.Value, MidpointRounding.AwayFromZero),
                BitrateKbps = ParseBitrate(BitrateTextBox.Text),
                AudioOption = GetSelectedAudioOption(),
            };
        }

        /// <summary>
        /// コマンドライン表示用の設定を構築します。
        /// </summary>
        /// <returns>表示用の設定。</returns>
        private FfmpegSettings BuildPreviewSettings()
        {
            FfmpegSettings settings = BuildSettings();
            return new FfmpegSettings
            {
                InputPath = string.IsNullOrWhiteSpace(settings.InputPath) ? "<input>" : settings.InputPath,
                OutputPath = string.IsNullOrWhiteSpace(settings.OutputPath) ? "<output>" : settings.OutputPath,
                Container = settings.Container,
                UseFastStart = settings.UseFastStart,
                Encoder = settings.Encoder,
                FrameRate = settings.FrameRate,
                RateMode = settings.RateMode,
                Quality = settings.Quality,
                BitrateKbps = settings.BitrateKbps,
                AudioOption = settings.AudioOption,
            };
        }

        /// <summary>
        /// コマンドライン表示を更新します。
        /// </summary>
        private void UpdateCommandLine()
        {
            if (string.IsNullOrWhiteSpace(ffmpegExecutablePath))
            {
                CommandLineTextBox.Text = "ffmpeg.exe が見つかりません。アプリ実行フォルダか PATH を確認してください。";
                return;
            }

            CommandLineTextBox.Text = FfmpegCommandBuilder.BuildCommandLine(ffmpegExecutablePath, BuildPreviewSettings());
        }

        /// <summary>
        /// FFmpeg 実行ファイルの探索結果を更新します。
        /// </summary>
        private void RefreshFfmpegExecutablePath()
        {
            ffmpegExecutablePath = FfmpegExecutableResolver.Resolve();
        }

        /// <summary>
        /// コンテナに応じて FastStart チェックボックス状態を更新します。
        /// </summary>
        private void UpdateFastStartState()
        {
            bool isMp4 = GetSelectedContainer() == ContainerKind.Mp4;
            FastStartCheckBox.IsEnabled = isMp4;
            FastStartCheckBox.IsChecked = isMp4;
        }

        /// <summary>
        /// レート指定に応じて入力 UI を切り替えます。
        /// </summary>
        private void UpdateRateModePanels()
        {
            bool useQuality = GetSelectedRateMode() == RateModeKind.Quality;
            QualityPanel.Visibility = useQuality ? Visibility.Visible : Visibility.Collapsed;
            BitratePanel.Visibility = useQuality ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// エンコーダーごとの品質レンジを反映します。
        /// </summary>
        /// <param name="profile">選択中のエンコーダープロファイル。</param>
        private void ApplyEncoderQualityRange(VideoEncoderProfile profile)
        {
            suppressEvents = true;

            try
            {
                QualitySlider.Minimum = profile.QualityMin;
                QualitySlider.Maximum = profile.QualityMax;
                QualitySlider.Value = profile.QualityDefault;
                QualityMinLabelTextBlock.Text = $"高品質 ({profile.QualityMin.ToString(CultureInfo.InvariantCulture)})";
                QualityMaxLabelTextBlock.Text = $"低品質 ({profile.QualityMax.ToString(CultureInfo.InvariantCulture)})";
                UpdateQualityText();
            }
            finally
            {
                suppressEvents = false;
            }
        }

        /// <summary>
        /// 品質値表示を更新します。
        /// </summary>
        private void UpdateQualityText()
        {
            int quality = (int)Math.Round(QualitySlider.Value, MidpointRounding.AwayFromZero);
            QualityValueTextBlock.Text = quality.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 出力拡張子をコンテナに合わせて更新します。
        /// </summary>
        private void UpdateOutputPathExtension()
        {
            if (string.IsNullOrWhiteSpace(OutputPathTextBox.Text))
            {
                return;
            }

            string extension = GetContainerExtension(GetSelectedContainer());
            string? updatedPath = Path.ChangeExtension(OutputPathTextBox.Text, extension);
            if (updatedPath is not null)
            {
                OutputPathTextBox.Text = updatedPath;
            }
        }

        /// <summary>
        /// ファイルピッカーに現在ウィンドウのハンドルを設定します。
        /// </summary>
        /// <param name="picker">初期化対象ピッカー。</param>
        private static void InitializePicker(object picker)
        {
            IntPtr windowHandle = WindowNative.GetWindowHandle(((App)Application.Current).MainWindow);
            InitializeWithWindow.Initialize(picker, windowHandle);
        }

        /// <summary>
        /// コンボボックスに指定値の選択肢を設定します。
        /// </summary>
        /// <typeparam name="T">選択値の型。</typeparam>
        /// <param name="comboBox">対象コンボボックス。</param>
        /// <param name="options">選択肢一覧。</param>
        /// <param name="value">設定する値。</param>
        private static void SelectOption<T>(ComboBox comboBox, IReadOnlyList<SelectionOption<T>> options, T value)
        {
            SelectionOption<T>? found = options.FirstOrDefault(option => EqualityComparer<T>.Default.Equals(option.Value, value));
            comboBox.SelectedItem = found ?? options[0];
        }

        /// <summary>
        /// エンコーダーをラベルで選択します。
        /// </summary>
        /// <param name="label">選択対象ラベル。</param>
        private void SelectEncoder(string label)
        {
            SelectionOption<VideoEncoderProfile>? found = videoEncoderOptions.FirstOrDefault(option => string.Equals(option.Label, label, StringComparison.Ordinal));
            VideoEncoderComboBox.SelectedItem = found ?? videoEncoderOptions[0];
        }

        /// <summary>
        /// 選択中コンテナを取得します。
        /// </summary>
        /// <returns>選択中コンテナ。</returns>
        private ContainerKind GetSelectedContainer()
        {
            return (ContainerComboBox.SelectedItem as SelectionOption<ContainerKind>)?.Value ?? ContainerKind.Mp4;
        }

        /// <summary>
        /// 選択中エンコーダープロファイルを取得します。
        /// </summary>
        /// <returns>選択中エンコーダープロファイル。</returns>
        private VideoEncoderProfile GetSelectedEncoder()
        {
            return (VideoEncoderComboBox.SelectedItem as SelectionOption<VideoEncoderProfile>)?.Value ?? videoEncoderOptions[0].Value;
        }

        /// <summary>
        /// 選択中フレームレートを取得します。
        /// </summary>
        /// <returns>選択中フレームレート。</returns>
        private FrameRateOption GetSelectedFrameRate()
        {
            return FrameRateComboBox.SelectedItem as FrameRateOption ?? frameRateOptions[0];
        }

        /// <summary>
        /// 選択中レートモードを取得します。
        /// </summary>
        /// <returns>選択中レートモード。</returns>
        private RateModeKind GetSelectedRateMode()
        {
            return (RateModeComboBox.SelectedItem as SelectionOption<RateModeKind>)?.Value ?? RateModeKind.Quality;
        }

        /// <summary>
        /// 選択中音声オプションを取得します。
        /// </summary>
        /// <returns>選択中音声オプション。</returns>
        private AudioOptionKind GetSelectedAudioOption()
        {
            return (AudioOptionComboBox.SelectedItem as SelectionOption<AudioOptionKind>)?.Value ?? AudioOptionKind.Copy;
        }

        /// <summary>
        /// コンテナに対応する拡張子を返します。
        /// </summary>
        /// <param name="container">コンテナ種類。</param>
        /// <returns>拡張子文字列。</returns>
        private static string GetContainerExtension(ContainerKind container)
        {
            return container == ContainerKind.Mp4 ? ".mp4" : ".mkv";
        }

        /// <summary>
        /// ビットレート入力を数値化します。
        /// </summary>
        /// <param name="text">入力文字列。</param>
        /// <returns>有効時は kbps 値、無効時は <see langword="null"/>。</returns>
        private static int? ParseBitrate(string text)
        {
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int bitrate) && bitrate > 0
                ? bitrate
                : null;
        }

        /// <summary>
        /// メッセージダイアログを表示します。
        /// </summary>
        /// <param name="title">タイトル。</param>
        /// <param name="message">メッセージ。</param>
        private async Task ShowMessageAsync(string title, string message)
        {
            ContentDialog dialog = new()
            {
                Title = title,
                Content = message,
                CloseButtonText = "閉じる",
                XamlRoot = XamlRoot,
            };

            await dialog.ShowAsync();
        }
    }
}
