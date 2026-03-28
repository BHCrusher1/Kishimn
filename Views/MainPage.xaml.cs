using System.Diagnostics;
using System.Globalization;
using Windows.Storage.Pickers;

namespace Kishimn.Views
{
    /// <summary>
    /// FFmpegの設定をGUIから操作するメイン画面。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // コンテナの選択肢一覧。
        private static readonly IReadOnlyList<OptionItem> ContainerOptions =
        [
            new("MP4", "mp4"),
            new("MKV", "mkv")
        ];

        // 動画エンコーダーの選択肢一覧。
        // 表示名 / FFmpegエンコーダー名 / コーデックファミリー / 品質指定パラメータ種別 / 品質値の最小値 / 品質値の最大値 / 品質値のデフォルト値
        private static readonly IReadOnlyList<EncoderOption> VideoEncoderOptions =
        [
            new("H264 (CPU)", "libx264", VideoCodecFamily.H264, RateParamKind.Crf, 16, 40, 23),
            new("H264 (AMD AMF)", "h264_amf", VideoCodecFamily.H264, RateParamKind.AmfH264Qp, 0, 51, 23),
            new("H264 (Intel QSV)", "h264_qsv", VideoCodecFamily.H264, RateParamKind.GlobalQuality, 1, 51, 23),
            new("H264 (NVIDIA NVENC)", "h264_nvenc", VideoCodecFamily.H264, RateParamKind.Cq, 0, 51, 23),
            new("H265 (CPU)", "libx265", VideoCodecFamily.H265, RateParamKind.Crf, 16, 40, 28),
            new("H265 (AMD AMF)", "hevc_amf", VideoCodecFamily.H265, RateParamKind.AmfHevcQp, 0, 51, 28),
            new("H265 (Intel QSV)", "hevc_qsv", VideoCodecFamily.H265, RateParamKind.GlobalQuality, 1, 51, 28),
            new("H265 (NVIDIA NVENC)", "hevc_nvenc", VideoCodecFamily.H265, RateParamKind.Cq, 0, 51, 28),
            new("VP9 (CPU)", "libvpx-vp9", VideoCodecFamily.VP9, RateParamKind.Crf, 15, 50, 31),
            new("VP9 (Intel QSV)", "vp9_qsv", VideoCodecFamily.VP9, RateParamKind.GlobalQuality, 1, 51, 31),
            new("AV1 (CPU)", "libsvtav1", VideoCodecFamily.AV1, RateParamKind.Crf, 15, 50, 35),
            new("AV1 (AMD AMF)", "av1_amf", VideoCodecFamily.AV1, RateParamKind.AmfAv1Qp, 0, 51, 28),
            new("AV1 (Intel QSV)", "av1_qsv", VideoCodecFamily.AV1, RateParamKind.GlobalQuality, 1, 51, 28),
            new("AV1 (NVIDIA NVENC)", "av1_nvenc", VideoCodecFamily.AV1, RateParamKind.Cq, 0, 51, 28)
        ];

        // フレームレートの選択肢一覧。
        private static readonly IReadOnlyList<OptionItem> FrameRateOptions =
        [
            new("オリジナル", "original"),
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
            new("144", "144")
        ];

        // レート指定の選択肢一覧。
        private static readonly IReadOnlyList<string> RateModeOptions =
        [
            "品質",
            "ビットレート"
        ];

        // 音声オプションの選択肢一覧。
        private static readonly IReadOnlyList<string> AudioOptions =
        [
            "音声無し (-an)",
            "コピー (-c:a copy)",
            "音量調整 (YouTube)",
            "音量調整 (ARIB TR-B32)"
        ];

        // 既定値。
        private const string DefaultContainer = "mp4";
        private const string DefaultFrameRate = "original";
        private const int DefaultBitrateKbps = 2500;

        // UI初期化時のイベント多重実行を抑止するフラグ。
        private bool _isInitializing;

        // 初期化処理を行い、選択肢をUIへ反映する。
        public MainPage()
        {
            InitializeComponent();
            InitializeSelections();
            ResetBodySettings();
            UpdateCommandPreview();
        }

        // 画面起動時に全選択肢をコード側からUIへバインドする。
        private void InitializeSelections()
        {
            _isInitializing = true;

            ContainerComboBox.ItemsSource = ContainerOptions;
            ContainerComboBox.DisplayMemberPath = nameof(OptionItem.Label);
            ContainerComboBox.SelectedValuePath = nameof(OptionItem.Value);

            FrameRateComboBox.ItemsSource = FrameRateOptions;
            FrameRateComboBox.DisplayMemberPath = nameof(OptionItem.Label);
            FrameRateComboBox.SelectedValuePath = nameof(OptionItem.Value);

            RateModeRadioButtons.ItemsSource = RateModeOptions;
            RateModeRadioButtons.SelectedIndex = 0;

            AudioOptionRadioButtons.ItemsSource = AudioOptions;
            AudioOptionRadioButtons.SelectedIndex = 1;

            VideoEncoderComboBox.ItemsSource = VideoEncoderOptions;
            VideoEncoderComboBox.DisplayMemberPath = nameof(EncoderOption.Label);

            _isInitializing = false;
        }

        // コンテナ選択に応じてMP4固有オプションの表示状態を切り替える。
        private void RefreshContainerUi()
        {
            bool isMp4 = SelectedContainerValue() == "mp4";
            Mp4OptionsPanel.Visibility = isMp4 ? Visibility.Visible : Visibility.Collapsed;
        }

        // レート指定に応じて品質スライダーとビットレート入力の表示を切り替える。
        private void RefreshRateModeUi()
        {
            bool qualityMode = IsQualityMode();
            QualitySliderPanel.Visibility = qualityMode ? Visibility.Visible : Visibility.Collapsed;
            BitratePanel.Visibility = qualityMode ? Visibility.Collapsed : Visibility.Visible;
        }

        // 選択中エンコーダーの品質範囲をスライダーに反映する。
        private void RefreshEncoderQualityRange(bool resetToDefault = false)
        {
            EncoderOption encoder = SelectedEncoder();
            QualitySlider.Minimum = encoder.QualityMin;
            QualitySlider.Maximum = encoder.QualityMax;

            if (resetToDefault || QualitySlider.Value < encoder.QualityMin || QualitySlider.Value > encoder.QualityMax)
            {
                QualitySlider.Value = encoder.QualityDefault;
            }

            // 品質値表示を「品質値: 数値」の形式に統一する。
            int qualityValue = (int)Math.Round(QualitySlider.Value);
            QualityValueTextBlock.Text = $"品質値: {qualityValue.ToString(CultureInfo.InvariantCulture)}";

            // スライダー左右に現在エンコーダーの品質範囲を明示する。
            QualityMinLabelTextBlock.Text = $"高品質 ({encoder.QualityMin.ToString(CultureInfo.InvariantCulture)})";
            QualityMaxLabelTextBlock.Text = $"低品質 ({encoder.QualityMax.ToString(CultureInfo.InvariantCulture)})";
        }

        // Body部の値を要件の初期値へ戻す。
        private void ResetBodySettings()
        {
            _isInitializing = true;

            ContainerComboBox.SelectedValue = DefaultContainer;
            FastStartCheckBox.IsChecked = true;
            VideoEncoderComboBox.SelectedIndex = 0;
            FrameRateComboBox.SelectedValue = DefaultFrameRate;
            RateModeRadioButtons.SelectedIndex = 0;
            BitrateTextBox.Text = DefaultBitrateKbps.ToString(CultureInfo.InvariantCulture);
            AudioOptionRadioButtons.SelectedIndex = 1;

            _isInitializing = false;

            RefreshContainerUi();
            // リセット時は現在エンコーダーの品質デフォルト値へ必ず戻す。
            RefreshEncoderQualityRange(resetToDefault: true);
            RefreshRateModeUi();
        }

        // 設定変更時にコマンドプレビューを再生成する共通ハンドラ。
        private void OnAnySettingChanged(object sender, object e)
        {
            if (_isInitializing)
            {
                return;
            }

            int qualityValue = (int)Math.Round(QualitySlider.Value);
            QualityValueTextBlock.Text = $"品質値: {qualityValue.ToString(CultureInfo.InvariantCulture)}";
            UpdateCommandPreview();
        }

        // コンテナ変更時に関連UIを更新してコマンドプレビューへ反映する。
        private void OnContainerChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            RefreshContainerUi();
            UpdateCommandPreview();
        }

        // エンコーダー変更時に品質範囲を更新してコマンドプレビューへ反映する。
        private void OnVideoEncoderChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            RefreshEncoderQualityRange();
            UpdateCommandPreview();
        }

        // レート指定変更時に表示切替を行い、コマンドプレビューへ反映する。
        private void OnRateModeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            RefreshRateModeUi();
            UpdateCommandPreview();
        }

        // 入力ファイルの参照ダイアログを開いて選択結果を反映する。
        private async void OnBrowseInputClick(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new();
            picker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));

            var file = await picker.PickSingleFileAsync();
            if (file is null)
            {
                return;
            }

            InputPathTextBox.Text = file.Path;
        }

        // 出力ファイルの参照ダイアログを開いて選択結果を反映する。
        private async void OnBrowseOutputClick(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new();
            picker.SuggestedFileName = BuildSuggestedOutputName();
            picker.FileTypeChoices.Add("Video", [$".{SelectedContainerValue()}"]);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));

            var file = await picker.PickSaveFileAsync();
            if (file is null)
            {
                return;
            }

            // FileSavePickerが作成した空ファイルは、誤解を避けるため選択直後に削除する。
            try
            {
                FileInfo selectedFileInfo = new(file.Path);
                if (selectedFileInfo.Exists && selectedFileInfo.Length == 0)
                {
                    File.Delete(file.Path);
                }
            }
            catch
            {
                // 削除に失敗しても保存先パスの選択自体は有効なため、ここでは処理を継続する。
            }

            OutputPathTextBox.Text = file.Path;
        }

        // Body部の設定だけを初期値へ戻し、プレビューを更新する。
        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            ResetBodySettings();
            UpdateCommandPreview();
        }

        // 生成済みコマンドでffmpegを外部プロセスとして実行する。
        private async void OnExecuteClick(object sender, RoutedEventArgs e)
        {
            // 実行前に ffmpeg パスと引数の妥当性を検証する。
            if (!TryBuildExecutionCommand(out string ffmpegPath, out string arguments, out string errorMessage))
            {
                await ShowMessageAsync(errorMessage);
                return;
            }

            try
            {
                // ffmpeg を外部プロセスとして起動する。
                ProcessStartInfo startInfo = new()
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(OutputPathTextBox.Text.Trim()) ?? AppContext.BaseDirectory
                };

                _ = Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                await ShowMessageAsync($"ffmpegの起動に失敗しました。{Environment.NewLine}{ex.Message}");
            }
        }

        // 現在設定からコマンドプレビュー文字列を作成して表示する。
        private void UpdateCommandPreview()
        {
            string input = string.IsNullOrWhiteSpace(InputPathTextBox.Text) ? "<input>" : InputPathTextBox.Text.Trim();
            string output = string.IsNullOrWhiteSpace(OutputPathTextBox.Text) ? "<output>" : OutputPathTextBox.Text.Trim();
            string ffmpegPath = ResolveFfmpegPath() ?? "ffmpeg.exe";

            string arguments = BuildFfmpegArguments(input, output);
            CommandLineTextBox.Text = $"{FormatExecutable(ffmpegPath)} {arguments}";
        }

        // 実行コマンドを構築し、妥当性検証の結果を返す。
        private bool TryBuildExecutionCommand(out string ffmpegPath, out string arguments, out string errorMessage)
        {
            // 戻り値の初期化を行う。
            ffmpegPath = ResolveFfmpegPath() ?? string.Empty;
            arguments = string.Empty;
            errorMessage = string.Empty;

            // ffmpeg 実行ファイル存在チェックを行う。
            if (string.IsNullOrWhiteSpace(ffmpegPath))
            {
                errorMessage = "ffmpeg.exe が見つかりません。実行ファイルと同じフォルダ、または PATH を確認してください。";
                return false;
            }

            // 入力ファイル存在チェックを行う。
            string inputPath = InputPathTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                errorMessage = "変換元ファイルを指定してください。";
                return false;
            }

            if (!File.Exists(inputPath))
            {
                errorMessage = "変換元ファイルが存在しません。";
                return false;
            }

            // 出力パス妥当性チェックを行う。
            string outputPath = OutputPathTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                errorMessage = "保存先ファイルを指定してください。";
                return false;
            }

            // 入力と出力が同一パスなら拒否する。
            if (string.Equals(Path.GetFullPath(inputPath), Path.GetFullPath(outputPath), StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "変換元ファイルと保存先ファイルは別のパスにしてください。";
                return false;
            }

            // 出力先ディレクトリ存在チェックを行う。
            string? outputDirectory = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrWhiteSpace(outputDirectory) || !Directory.Exists(outputDirectory))
            {
                errorMessage = "保存先ディレクトリが存在しません。";
                return false;
            }

            // ビットレートモードでは値の妥当性を検証する。
            if (!IsQualityMode() && !TryParseBitrateKbps(out _))
            {
                errorMessage = "ビットレートは 1 以上の整数 (kbps) を入力してください。";
                return false;
            }

            // 実行用引数を構築して成功を返す。
            arguments = BuildFfmpegArguments(inputPath, outputPath);
            return true;
        }

        // 現在設定をFFmpeg引数へ変換する。
        private string BuildFfmpegArguments(string input, string output)
        {
            List<string> args =
            [
                "-i",
                Quote(input)
            ];

            EncoderOption encoder = SelectedEncoder();
            args.Add("-c:v");
            args.Add(encoder.EncoderName);

            // H265 は Apple 互換のため hvc1 タグを付与する。
            if (encoder.CodecFamily == VideoCodecFamily.H265)
            {
                args.Add("-tag:v");
                args.Add("hvc1");
            }

            // フレームレート指定がある場合のみ -r を追加する。
            string frameRate = SelectedFrameRateValue();
            if (frameRate != DefaultFrameRate)
            {
                args.Add("-r");
                args.Add(frameRate);
            }

            // レートモードごとに動画品質関連オプションを分割して追加する。
            if (IsQualityMode())
            {
                int quality = (int)Math.Round(QualitySlider.Value);
                AddQualityOptions(args, encoder, quality);
            }
            else
            {
                int bitrateKbps = ParseBitrateKbps();
                AddBitrateOptions(args, bitrateKbps);
            }

            // MP4 かつ有効時のみ faststart を追加する。
            if (SelectedContainerValue() == "mp4")
            {
                if (FastStartCheckBox.IsChecked == true)
                {
                    args.Add("-movflags");
                    args.Add("+faststart");
                }
            }

            // 音声オプションを追加する。
            AddAudioOptions(args, SelectedAudioMode());

            args.Add(Quote(output));
            return string.Join(' ', args);
        }

        // 品質モード向け動画オプションを追加する。
        private static void AddQualityOptions(List<string> args, EncoderOption encoder, int quality)
        {
            switch (encoder.RateParameter)
            {
                case RateParamKind.Crf:
                    args.Add("-crf");
                    args.Add(quality.ToString(CultureInfo.InvariantCulture));
                    break;
                case RateParamKind.Cq:
                    args.Add("-cq");
                    args.Add(quality.ToString(CultureInfo.InvariantCulture));
                    break;
                case RateParamKind.GlobalQuality:
                    args.Add("-global_quality");
                    args.Add(quality.ToString(CultureInfo.InvariantCulture));
                    break;
                case RateParamKind.AmfH264Qp:
                    args.Add("-qp_i");
                    args.Add(quality.ToString(CultureInfo.InvariantCulture));
                    args.Add("-qp_p");
                    args.Add(quality.ToString(CultureInfo.InvariantCulture));
                    args.Add("-qp_b");
                    args.Add(quality.ToString(CultureInfo.InvariantCulture));
                    break;
                case RateParamKind.AmfHevcQp:
                case RateParamKind.AmfAv1Qp:
                    args.Add("-qp_i");
                    args.Add(quality.ToString(CultureInfo.InvariantCulture));
                    args.Add("-qp_p");
                    args.Add(quality.ToString(CultureInfo.InvariantCulture));
                    break;
            }
        }

        // ビットレートモード向け動画オプションを追加する。
        private static void AddBitrateOptions(List<string> args, int bitrateKbps)
        {
            args.Add("-b:v");
            args.Add($"{bitrateKbps}k");
            args.Add("-maxrate");
            args.Add($"{bitrateKbps}k");
            args.Add("-bufsize");
            args.Add($"{bitrateKbps * 2}k");
        }

        // 音声モードごとのオプションを追加する。
        private static void AddAudioOptions(List<string> args, AudioMode mode)
        {
            switch (mode)
            {
                case AudioMode.None:
                    args.Add("-an");
                    break;
                case AudioMode.Copy:
                    args.Add("-c:a");
                    args.Add("copy");
                    break;
                case AudioMode.YouTube:
                    args.Add("-af");
                    args.Add("loudnorm=I=-14:LRA=11:TP=-1.5");
                    args.Add("-c:a");
                    args.Add("aac");
                    args.Add("-b:a");
                    args.Add("192k");
                    break;
                case AudioMode.Arib:
                    args.Add("-af");
                    args.Add("loudnorm=I=-24:LRA=7:TP=-2");
                    args.Add("-c:a");
                    args.Add("aac");
                    args.Add("-b:a");
                    args.Add("192k");
                    break;
            }
        }

        // 現在の設定から保存ダイアログ用の推奨ファイル名を作成する。
        private string BuildSuggestedOutputName()
        {
            string inputPath = InputPathTextBox.Text.Trim();
            string baseName = string.IsNullOrWhiteSpace(inputPath)
                ? "output"
                : Path.GetFileNameWithoutExtension(inputPath);

            return $"{baseName}_out";
        }

        // ビットレート入力値を安全な範囲の整数に正規化する。
        private int ParseBitrateKbps()
        {
            if (TryParseBitrateKbps(out int bitrate))
            {
                return bitrate;
            }

            return DefaultBitrateKbps;
        }

        // ビットレート入力値を整数として検証する。
        private bool TryParseBitrateKbps(out int bitrate)
        {
            return int.TryParse(BitrateTextBox.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out bitrate) && bitrate > 0;
        }

        // 引数に空白が含まれる値を安全に引用符で囲む。
        private static string Quote(string value)
            => $"\"{value.Replace("\"", "\\\"")}\"";

        // 実行パスに空白が含まれる場合のみクォートする。
        private static string FormatExecutable(string executable)
            => executable.Contains(' ', StringComparison.Ordinal) ? Quote(executable) : executable;

        // ffmpeg.exeの探索優先順を満たす実行ファイルパスを返す。
        private static string? ResolveFfmpegPath()
        {
            // 1) 出力exeと同じフォルダの ffmpeg.exe を最優先する。
            string localPath = Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe");
            if (File.Exists(localPath))
            {
                return localPath;
            }

            // 2) PATH 内の ffmpeg.exe を順番に探索する。
            string? pathValue = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrWhiteSpace(pathValue))
            {
                return null;
            }

            string[] directories = pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (string directory in directories)
            {
                string sanitized = directory.Trim().Trim('"');
                if (string.IsNullOrWhiteSpace(sanitized))
                {
                    continue;
                }

                try
                {
                    string candidate = Path.Combine(sanitized, "ffmpeg.exe");
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }
                catch
                {
                    // 無効な PATH エントリは無視して探索を継続する。
                }
            }

            // 見つからない場合は null を返す。
            return null;
        }

        // 現在のレート指定が品質モードかどうかを返す。
        private bool IsQualityMode()
            => RateModeRadioButtons.SelectedIndex == 0;

        // 現在選択中のコンテナ値を取得する。
        private string SelectedContainerValue()
            => ContainerComboBox.SelectedValue as string ?? DefaultContainer;

        // 現在選択中のフレームレート値を取得する。
        private string SelectedFrameRateValue()
            => FrameRateComboBox.SelectedValue as string ?? DefaultFrameRate;

        // 現在選択中のエンコーダー情報を取得する。
        private EncoderOption SelectedEncoder()
            => VideoEncoderComboBox.SelectedItem as EncoderOption ?? VideoEncoderOptions[0];

        // 現在選択中の音声モード値を取得する。
        private AudioMode SelectedAudioMode()
        {
            return AudioOptionRadioButtons.SelectedIndex switch
            {
                0 => AudioMode.None,
                2 => AudioMode.YouTube,
                3 => AudioMode.Arib,
                _ => AudioMode.Copy
            };
        }

        // エラーメッセージをダイアログ表示する。
        private async Task ShowMessageAsync(string message)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Title = "Kishimn",
                Content = message,
                CloseButtonText = "OK"
            };

            _ = await dialog.ShowAsync();
        }

        // 表示用ラベルと内部値を保持する共通項目。
        private sealed record OptionItem(string Label, string Value);

        // 動画コーデック分類を表す列挙型。
        private enum VideoCodecFamily
        {
            H264,
            H265,
            VP9,
            AV1
        }

        // 品質指定で利用するFFmpegパラメータ種別を表す列挙型。
        private enum RateParamKind
        {
            Crf,
            Cq,
            GlobalQuality,
            AmfH264Qp,
            AmfHevcQp,
            AmfAv1Qp
        }

        // 音声モードを表す列挙型。
        private enum AudioMode
        {
            None,
            Copy,
            YouTube,
            Arib
        }

        // 動画エンコーダーごとの制約値を保持する定義。
        private sealed record EncoderOption(
            string Label,
            string EncoderName,
            VideoCodecFamily CodecFamily,
            RateParamKind RateParameter,
            int QualityMin,
            int QualityMax,
            int QualityDefault);
    }
}
