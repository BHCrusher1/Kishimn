namespace Kishimn.Models
{
    /// <summary>
    /// FFmpeg コマンド生成に必要な画面設定を表します。
    /// </summary>
    internal sealed class FfmpegSettings
    {
        /// <summary>
        /// 変換元ファイルパスを取得または設定します。
        /// </summary>
        public required string InputPath { get; init; }

        /// <summary>
        /// 保存先ファイルパスを取得または設定します。
        /// </summary>
        public required string OutputPath { get; init; }

        /// <summary>
        /// 出力コンテナ形式を取得または設定します。
        /// </summary>
        public required ContainerKind Container { get; init; }

        /// <summary>
        /// MP4 最適化を適用するかどうかを取得または設定します。
        /// </summary>
        public required bool UseFastStart { get; init; }

        /// <summary>
        /// 動画エンコーダープロファイルを取得または設定します。
        /// </summary>
        public required VideoEncoderProfile Encoder { get; init; }

        /// <summary>
        /// フレームレートを取得または設定します。
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
        /// 音声設定を取得または設定します。
        /// </summary>
        public required AudioOptionKind AudioOption { get; init; }
    }
}
