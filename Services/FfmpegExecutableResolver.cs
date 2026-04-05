namespace Kishimn.Services
{
    /// <summary>
    /// 使用する FFmpeg 実行ファイルを解決します。
    /// </summary>
    internal static class FfmpegExecutableResolver
    {
        /// <summary>
        /// 優先順位に従って FFmpeg 実行ファイルのフルパスを解決します。
        /// </summary>
        /// <returns>見つかった場合はフルパス、見つからない場合は <see langword="null"/>。</returns>
        public static string? Resolve()
        {
            string localPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe"));
            if (File.Exists(localPath))
            {
                return localPath;
            }

            string? pathEnvironment = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrWhiteSpace(pathEnvironment))
            {
                return null;
            }

            string[] directories = pathEnvironment.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (string directory in directories)
            {
                string normalizedDirectory = directory.Trim().Trim('"');
                if (string.IsNullOrWhiteSpace(normalizedDirectory))
                {
                    continue;
                }

                string candidate = Path.GetFullPath(Path.Combine(normalizedDirectory, "ffmpeg.exe"));
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
