namespace Kishimn.Models
{
    /// <summary>
    /// コンボボックスで使用する表示値と内部値の組み合わせを表します。
    /// </summary>
    /// <typeparam name="T">保持する値の型。</typeparam>
    internal sealed class SelectionOption<T>(string label, T value)
    {
        /// <summary>
        /// 画面表示用の名称を取得します。
        /// </summary>
        public string Label { get; } = label;

        /// <summary>
        /// 内部処理で利用する値を取得します。
        /// </summary>
        public T Value { get; } = value;
    }
}
