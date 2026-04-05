using Kishimn.Models;
using System.Globalization;
using System.Text;

namespace Kishimn.Services
{
    /// <summary>
    /// 画面設定から FFmpeg の引数および表示用コマンドラインを構築します。
    /// </summary>
    internal static class FfmpegCommandBuilder
    {
        /// <summary>
        /// FFmpeg の引数文字列を生成します。
        /// </summary>
        /// <param name="settings">コマンド生成に使用する設定。</param>
        /// <returns>FFmpeg に渡す引数文字列。</returns>
        public static string BuildArguments(FfmpegSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            List<string> args = ["-y", "-i", Quote(settings.InputPath), "-c:v", settings.Encoder.CodecName];

            if (!string.IsNullOrWhiteSpace(settings.FrameRate.Value))
            {
                args.Add("-r");
                args.Add(settings.FrameRate.Value);
            }

            if (settings.RateMode == RateModeKind.Quality)
            {
                AppendQualityArguments(args, settings.Encoder.CodecName, settings.Encoder.QualityArgumentKind, settings.Quality);
            }
            else if (settings.BitrateKbps is int bitrateKbps && bitrateKbps > 0)
            {
                string bitrateText = bitrateKbps.ToString(CultureInfo.InvariantCulture);
                string bufferText = (bitrateKbps * 2).ToString(CultureInfo.InvariantCulture);
                args.Add("-b:v");
                args.Add($"{bitrateText}k");
                args.Add("-maxrate");
                args.Add($"{bitrateText}k");
                args.Add("-bufsize");
                args.Add($"{bufferText}k");
            }

            if (settings.Encoder.RequiresHvc1Tag)
            {
                args.Add("-tag:v");
                args.Add("hvc1");
            }

            if (settings.Container == ContainerKind.Mp4 && settings.UseFastStart)
            {
                args.Add("-movflags");
                args.Add("+faststart");
            }

            AppendAudioArguments(args, settings.AudioOption);

            args.Add(Quote(settings.OutputPath));
            return string.Join(' ', args);
        }

        /// <summary>
        /// 表示用コマンドライン文字列を生成します。
        /// </summary>
        /// <param name="ffmpegExecutablePath">使用する FFmpeg 実行ファイルのフルパス。</param>
        /// <param name="settings">コマンド生成に使用する設定。</param>
        /// <returns>表示用コマンドライン。</returns>
        public static string BuildCommandLine(string ffmpegExecutablePath, FfmpegSettings settings)
        {
            return $"{Quote(ffmpegExecutablePath)} {BuildArguments(settings)}";
        }

        /// <summary>
        /// 品質モード引数を追加します。
        /// </summary>
        /// <param name="args">追加対象の引数リスト。</param>
        /// <param name="qualityKind">品質指定の種類。</param>
        /// <param name="qualityValue">品質値。</param>
        private static void AppendQualityArguments(List<string> args, string codecName, string qualityKind, int qualityValue)
        {
            string valueText = qualityValue.ToString(CultureInfo.InvariantCulture);

            switch (qualityKind)
            {
                case "crf":
                    args.Add("-crf");
                    args.Add(valueText);
                    break;
                case "global_quality":
                    args.Add("-global_quality");
                    args.Add(valueText);
                    break;
                case "cq":
                    args.Add("-cq");
                    args.Add(valueText);
                    break;
                case "qp":
                    args.Add("-qp_i");
                    args.Add(valueText);
                    args.Add("-qp_p");
                    args.Add(valueText);
                    if (string.Equals(codecName, "h264_amf", StringComparison.Ordinal))
                    {
                        args.Add("-qp_b");
                        args.Add(valueText);
                    }
                    break;
                default:
                    args.Add("-crf");
                    args.Add(valueText);
                    break;
            }
        }

        /// <summary>
        /// 音声モード引数を追加します。
        /// </summary>
        /// <param name="args">追加対象の引数リスト。</param>
        /// <param name="audioOption">音声オプション。</param>
        private static void AppendAudioArguments(List<string> args, AudioOptionKind audioOption)
        {
            switch (audioOption)
            {
                case AudioOptionKind.None:
                    args.Add("-an");
                    break;
                case AudioOptionKind.Copy:
                    args.Add("-c:a");
                    args.Add("copy");
                    break;
                case AudioOptionKind.LoudnormYoutube:
                    args.Add("-af");
                    args.Add(Quote("loudnorm=I=-14:LRA=11:TP=-1.5"));
                    args.Add("-c:a");
                    args.Add("aac");
                    args.Add("-b:a");
                    args.Add("192k");
                    break;
                case AudioOptionKind.LoudnormArib:
                    args.Add("-af");
                    args.Add(Quote("loudnorm=I=-24:LRA=7:TP=-2"));
                    args.Add("-c:a");
                    args.Add("aac");
                    args.Add("-b:a");
                    args.Add("192k");
                    break;
                default:
                    args.Add("-c:a");
                    args.Add("copy");
                    break;
            }
        }

        /// <summary>
        /// コマンドライン引数のために引用符で囲みます。
        /// </summary>
        /// <param name="value">対象文字列。</param>
        /// <returns>引用符で囲まれた文字列。</returns>
        private static string Quote(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "\"\"";
            }

            StringBuilder builder = new(value.Length + 2);
            builder.Append('"');
            builder.Append(value.Replace("\"", "\\\"", StringComparison.Ordinal));
            builder.Append('"');
            return builder.ToString();
        }
    }
}
