namespace Kishimn.Models
{
    /// <summary>
    /// 動画レート指定方式を表します。
    /// </summary>
    internal enum RateModeKind
    {
        /// <summary>
        /// 品質優先の方式です。
        /// </summary>
        Quality,

        /// <summary>
        /// ビットレート指定方式です。
        /// </summary>
        Bitrate,
    }
}
