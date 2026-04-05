namespace Kishimn.Models
{
    /// <summary>
    /// 動画エンコーダーの選択肢情報を表します。
    /// </summary>
    /// <param name="label">画面表示ラベル。</param>
    /// <param name="codecName">FFmpeg のコーデック名。</param>
    /// <param name="qualityArgumentKind">品質引数の種別。</param>
    /// <param name="qualityMin">品質値の最小値。</param>
    /// <param name="qualityMax">品質値の最大値。</param>
    /// <param name="qualityDefault">品質値の既定値。</param>
    /// <param name="requiresHvc1Tag">H.265 で <c>-tag:v hvc1</c> を追加するかどうか。</param>
    internal sealed class VideoEncoderProfile(
        string label,
        string codecName,
        string qualityArgumentKind,
        int qualityMin,
        int qualityMax,
        int qualityDefault,
        bool requiresHvc1Tag = false)
    {
        /// <summary>
        /// 画面表示ラベルを取得します。
        /// </summary>
        public string Label { get; } = label;

        /// <summary>
        /// FFmpeg のコーデック名を取得します。
        /// </summary>
        public string CodecName { get; } = codecName;

        /// <summary>
        /// 品質引数の種別を取得します。
        /// </summary>
        public string QualityArgumentKind { get; } = qualityArgumentKind;

        /// <summary>
        /// 品質値の最小値を取得します。
        /// </summary>
        public int QualityMin { get; } = qualityMin;

        /// <summary>
        /// 品質値の最大値を取得します。
        /// </summary>
        public int QualityMax { get; } = qualityMax;

        /// <summary>
        /// 品質値の既定値を取得します。
        /// </summary>
        public int QualityDefault { get; } = qualityDefault;

        /// <summary>
        /// H.265 で <c>-tag:v hvc1</c> を追加するかどうかを取得します。
        /// </summary>
        public bool RequiresHvc1Tag { get; } = requiresHvc1Tag;
    }
}
