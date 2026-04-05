namespace Kishimn.Models
{
    /// <summary>
    /// フレームレート選択肢を表します。
    /// </summary>
    internal sealed class FrameRateOption(string label, string? value)
    {
        /// <summary>
        /// 画面表示名を取得します。
        /// </summary>
        public string Label { get; } = label;

        /// <summary>
        /// FFmpeg に渡す値を取得します。
        /// </summary>
        public string? Value { get; } = value;
    }
}
