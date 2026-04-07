namespace Kishimn.Models
{
    /// <summary>
    /// FFmpeg コマンド構築に使用する設定を保持します。
    /// </summary>
    internal sealed class FfmpegSettings
    {
        /// <summary>
        /// 入力ファイルパスを取得または設定します。
        /// </summary>
        public required string InputPath { get; init; }

        /// <summary>
        /// 出力ファイルパスを取得または設定します。
        /// </summary>
        public required string OutputPath { get; init; }

        /// <summary>
        /// コンテナ形式を取得または設定します。
        /// </summary>
        public required ContainerKind Container { get; init; }

        /// <summary>
        /// MP4 の FastStart を使用するかどうかを取得または設定します。
        /// </summary>
        public required bool UseFastStart { get; init; }

        /// <summary>
        /// 動画エンコーダープロファイルを取得または設定します。
        /// </summary>
        public required VideoEncoderProfile Encoder { get; init; }

        /// <summary>
        /// フレームレート設定を取得または設定します。
        /// </summary>
        public required FrameRateOption FrameRate { get; init; }

        /// <summary>
        /// レート指定方式を取得または設定します。
        /// </summary>
        public required RateModeKind RateMode { get; init; }

        /// <summary>
        /// 品質値を取得または設定します。
        /// </summary>
        public required int Quality { get; init; }

        /// <summary>
        /// 動画ビットレート (kbps) を取得または設定します。
        /// </summary>
        public required int? BitrateKbps { get; init; }

        /// <summary>
        /// 音声オプションを取得または設定します。
        /// </summary>
        public required AudioOptionKind AudioOption { get; init; }

        /// <summary>
        /// 追加の FFmpeg 引数を取得または設定します。
        /// </summary>
        public required string AdditionalOptions { get; init; }
    }
}
