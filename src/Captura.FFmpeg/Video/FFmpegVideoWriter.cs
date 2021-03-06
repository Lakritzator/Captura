﻿using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading.Tasks;
using Captura.Base.Images;
using Captura.Base.Services;
using Captura.Base.Video;
using Captura.FFmpeg.ArgsBuilder;
using Captura.FFmpeg.Settings;

namespace Captura.FFmpeg.Video
{
    /// <summary>
    /// Encode Video using FFmpeg.exe
    /// </summary>
    public class FFmpegWriter : IVideoFileWriter
    {
        private readonly NamedPipeServerStream _audioPipe;

        private readonly Process _ffmpegProcess;
        private readonly NamedPipeServerStream _ffmpegIn;
        private byte[] _videoBuffer;

        private static string GetPipeName() => $"captura-{Guid.NewGuid()}";

        /// <summary>
        /// Creates a new instance of <see cref="FFmpegWriter"/>.
        /// </summary>
        public FFmpegWriter(FFmpegVideoWriterArgs args)
        {
            var settings = ServiceProvider.Get<FFmpegSettings>();

            _videoBuffer = new byte[args.ImageProvider.Width * args.ImageProvider.Height * 4];

            Console.WriteLine($"Video Buffer Allocated: {_videoBuffer.Length}");

            var videoPipeName = GetPipeName();

            var argsBuilder = new FFmpegArgsBuilder();

            argsBuilder.AddInputPipe(videoPipeName)
                .AddArg("-thread_queue_size 512")
                .AddArg($"-framerate {args.FrameRate}")
                .SetFormat("rawvideo")
                .AddArg("-pix_fmt rgb32")
                .SetVideoSize(args.ImageProvider.Width, args.ImageProvider.Height);

            var output = argsBuilder.AddOutputFile(args.FileName)
                .AddArg(args.VideoArgsProvider(args.VideoQuality))
                .SetFrameRate(args.FrameRate);
            
            if (settings.Resize)
            {
                var width = settings.ResizeWidth;
                var height = settings.ResizeHeight;

                if (width % 2 == 1)
                    ++width;

                if (height % 2 == 1)
                    ++height;

                output.AddArg($"-vf scale={width}:{height}");
            }

            if (args.AudioProvider != null)
            {
                var audioPipeName = GetPipeName();

                argsBuilder.AddInputPipe(audioPipeName)
                    .AddArg("-thread_queue_size 512")
                    .SetFormat("s16le")
                    .SetAudioCodec("pcm_s16le")
                    .SetAudioFrequency(args.Frequency)
                    .SetAudioChannels(args.Channels);

                output.AddArg(args.AudioArgsProvider(args.AudioQuality));

                // UpdatePeriod * Frequency * (Bytes per Second) * Channels * 2
                var audioBufferSize = (int)((1000.0 / args.FrameRate) * 44.1 * 2 * 2 * 2);

                _audioPipe = new NamedPipeServerStream(audioPipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, audioBufferSize);
            }

            _ffmpegIn = new NamedPipeServerStream(videoPipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, _videoBuffer.Length);

            output.AddArg(args.OutputArgs);

            _ffmpegProcess = FFmpegService.StartFFmpeg(argsBuilder.GetArgs(), args.FileName);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _ffmpegIn.Dispose();

            _audioPipe?.Dispose();

            _ffmpegProcess.WaitForExit();

            _videoBuffer = null;
        }

        /// <summary>
        /// Gets whether audio is supported.
        /// </summary>
        public bool SupportsAudio { get; } = true;

        private bool _firstAudio = true;

        private Task _lastAudio;

        /// <summary>
        /// Write audio block to Audio Stream.
        /// </summary>
        /// <param name="buffer">Buffer containing audio data.</param>
        /// <param name="length">Length of audio data in bytes.</param>
        public void WriteAudio(byte[] buffer, int length)
        {
            if (_ffmpegProcess.HasExited)
            {
                throw new Exception("An Error Occurred with FFmpeg");
            }

            if (_firstAudio)
            {
                if (!_audioPipe.WaitForConnection(5000))
                {
                    throw new Exception("Cannot connect Audio pipe to FFmpeg");
                }

                _firstAudio = false;
            }

            _lastAudio?.Wait();

            _lastAudio = _audioPipe.WriteAsync(buffer, 0, length);
        }

        private bool _firstFrame = true;

        private Task _lastFrameTask;

        /// <summary>
        /// Writes an Image frame.
        /// </summary>
        public void WriteFrame(IBitmapFrame frame)
        {
            if (_ffmpegProcess.HasExited)
            {
                frame.Dispose();
                throw new Exception($"An Error Occurred with FFmpeg, Exit Code: {_ffmpegProcess.ExitCode}");
            }
            
            if (_firstFrame)
            {
                if (!_ffmpegIn.WaitForConnection(5000))
                {
                    throw new Exception("Cannot connect Video pipe to FFmpeg");
                }

                _firstFrame = false;
            }

            _lastFrameTask?.Wait();

            if (!(frame is RepeatFrame))
            {
                using (frame)
                {
                    frame.CopyTo(_videoBuffer, _videoBuffer.Length);
                }
            }

            _lastFrameTask = _ffmpegIn.WriteAsync(_videoBuffer, 0, _videoBuffer.Length);
        }
    }
}
