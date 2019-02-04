namespace Captura.FFmpeg.Audio
{
    /// <summary>
    /// Provides FFmpeg Audio encoding Command-line args.
    /// </summary>
    /// <param name="audioQuality">Audio Quality... 1 to 100.</param>
    /// <returns>FFmpeg Audio encoding Command-line args</returns>
    public delegate string FFmpegAudioArgsProvider(int audioQuality);
}
