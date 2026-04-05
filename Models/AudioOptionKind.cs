namespace Kishimn.Models
{
    /// <summary>
    /// 音声設定の種類を表します。
    /// </summary>
    internal enum AudioOptionKind
    {
        /// <summary>
        /// 音声なしです。
        /// </summary>
        None,

        /// <summary>
        /// 音声コピーです。
        /// </summary>
        Copy,

        /// <summary>
        /// YouTube 向け音量調整です。
        /// </summary>
        LoudnormYoutube,

        /// <summary>
        /// ARIB TR-B32 向け音量調整です。
        /// </summary>
        LoudnormArib,
    }
}
