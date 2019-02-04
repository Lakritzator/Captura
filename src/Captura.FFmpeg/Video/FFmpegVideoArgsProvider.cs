namespace Captura.FFmpeg.Video
{
    /// <summary>
    /// Provides Command-line args for FFmpeg Video encoding.
    /// </summary>
    /// <param name="videoQuality">Video Quality... 1 to 100.</param>
    public delegate string FFmpegVideoArgsProvider(int videoQuality);
}