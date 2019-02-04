using System;
using System.Diagnostics;
using System.IO;
using Captura.Base.Audio;
using Captura.FFmpeg.ArgsBuilder;

namespace Captura.FFmpeg.Audio
{
    internal class FFmpegAudioWriter : IAudioFileWriter
    {
        private readonly Process _ffmpegProcess;
        private readonly Stream _ffmpegIn;
        
        public FFmpegAudioWriter(string fileName, int audioQuality, FFmpegAudioArgsProvider audioArgsProvider, int frequency = 44100, int channels = 2)
        {
            var argsBuilder  = new FFmpegArgsBuilder();

            argsBuilder.AddStdIn()
                .SetFormat("s16le")
                .SetAudioCodec("pcm_s16le")
                .SetAudioFrequency(frequency)
                .SetAudioChannels(channels)
                .DisableVideo();

            argsBuilder.AddOutputFile(fileName)
                .AddArg(audioArgsProvider(audioQuality));

            _ffmpegProcess = FFmpegService.StartFFmpeg(argsBuilder.GetArgs(), fileName);
            
            _ffmpegIn = _ffmpegProcess.StandardInput.BaseStream;
        }

        public void Dispose()
        {
            Flush();

            _ffmpegIn.Close();
            _ffmpegProcess.WaitForExit();
        }

        public void Flush()
        {
            _ffmpegIn.Flush();
        }

        public void Write(byte[] data, int offset, int count)
        {
            if (_ffmpegProcess.HasExited)
            {
                throw new Exception("An Error Occurred with FFmpeg");
            }

            _ffmpegIn.Write(data, offset, count);
        }
    }
}
