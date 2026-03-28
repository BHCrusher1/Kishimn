namespace Kishimn.Views
{
    /// <summary>
    /// MainPageで利用する型定義と定数データを保持する部分クラス。
    /// </summary>
    public sealed partial class MainPage
    {
        // コンテナの選択肢一覧。
        private static readonly IReadOnlyList<OptionItem<string>> ContainerOptions =
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
            new("AV1 (CPU libaom-av1)", "libaom-av1", VideoCodecFamily.AV1, RateParamKind.Crf, 15, 50, 35),
            new("AV1 (CPU libsvtav1)", "libsvtav1", VideoCodecFamily.AV1, RateParamKind.Crf, 15, 50, 35),
            new("AV1 (AMD AMF)", "av1_amf", VideoCodecFamily.AV1, RateParamKind.AmfAv1Qp, 0, 51, 28),
            new("AV1 (Intel QSV)", "av1_qsv", VideoCodecFamily.AV1, RateParamKind.GlobalQuality, 1, 51, 28),
            new("AV1 (NVIDIA NVENC)", "av1_nvenc", VideoCodecFamily.AV1, RateParamKind.Cq, 0, 51, 28)
        ];

        // フレームレートの選択肢一覧。
        private static readonly IReadOnlyList<OptionItem<string>> FrameRateOptions =
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
        private static readonly IReadOnlyList<OptionItem<RateMode>> RateModeOptions =
        [
            new("品質", RateMode.Quality),
            new("ビットレート", RateMode.Bitrate)
        ];

        // 音声オプションの選択肢一覧。
        private static readonly IReadOnlyList<OptionItem<AudioMode>> AudioOptions =
        [
            new("音声無し (-an)", AudioMode.None),
            new("コピー (-c:a copy)", AudioMode.Copy),
            new("音量調整 (YouTube)", AudioMode.YouTube),
            new("音量調整 (ARIB TR-B32)", AudioMode.Arib)
        ];

        // 既定値。
        private const string DefaultContainer = "mp4";
        private const int DefaultVideoEncoderIndex = 0;
        private const string DefaultFrameRate = "original";
        private const RateMode DefaultRateMode = RateMode.Quality;
        private const AudioMode DefaultAudioOption = AudioMode.Copy;
        private const int DefaultOptionFallbackIndex = 0;
        private const int DefaultBitrateKbps = 2500;

        // 表示用ラベルと内部値を保持する共通項目。
        private sealed record OptionItem<TValue>(string Label, TValue Value);

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

        // レート指定モードを表す列挙型。
        private enum RateMode
        {
            Quality,
            Bitrate
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
