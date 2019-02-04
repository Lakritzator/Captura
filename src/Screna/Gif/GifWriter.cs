using System;
using System.IO;
using Captura.Base.Images;
using Captura.Base.Video;
using Screna.Frames;

namespace Screna.Gif
{
    /// <summary>
    /// Creates a GIF using .Net GIF encoding and additional animation headers.
    /// </summary>
    public class GifWriter : IVideoFileWriter
    {
        #region Fields

        private const long SourceGlobalColorInfoPosition = 10,
            SourceImageBlockPosition = 789;

        private readonly BinaryWriter _writer;
        private bool _firstFrame = true;
        private readonly int _defaultFrameDelay, _repeat;
        #endregion

        /// <summary>
        /// Creates a new instance of GifWriter.
        /// </summary>
        /// <param name="outStream">The <see cref="Stream"/> to output the Gif to.</param>
        /// <param name="frameRate">Fame Rate.</param>
        /// <param name="repeat">No of times the Gif should repeat... -1 to repeat indefinitely.</param>
        public GifWriter(Stream outStream, int frameRate, int repeat = -1)
        {
            if (repeat < -1)
                throw new ArgumentOutOfRangeException(nameof(repeat));

            _writer = new BinaryWriter(outStream ?? throw new ArgumentNullException(nameof(outStream)));
            _defaultFrameDelay = 1000 / frameRate;
            _repeat = repeat;
        }

        /// <summary>
        /// Creates a new instance of GifWriter.
        /// </summary>
        /// <param name="fileName">The path to the file to output the Gif to.</param>
        /// <param name="frameRate">Frame Rate.</param>
        /// <param name="repeat">No of times the Gif should repeat... -1 to repeat indefinitely.</param>
        public GifWriter(string fileName, int frameRate, int repeat = -1)
            : this(new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read), frameRate, repeat) { }

        /// <summary>
        /// <see cref="GifWriter"/> does not Support Audio.
        /// </summary>
        public void WriteAudio(byte[] buffer, int count) { }

        private MemoryStream _gifStream = new MemoryStream();

        private int _width, _height;

        /// <summary>
        /// Adds a frame to this animation.
        /// </summary>
        public void WriteFrame(IBitmapFrame frame, int delay)
        {
            _gifStream.Position = 0;
            
            if (_firstFrame)
            {
                if (frame is RepeatFrame)
                    return;

                _width = frame.Width;
                _height = frame.Height;
            }

            if (frame is DrawingFrameBase frameBase)
            {
                using (frame)
                    frameBase.SaveGif(_gifStream);
            }

            // Steal the global color table info
            if (_firstFrame)
                InitHeader(_gifStream, _writer, _width, _height);

            WriteGraphicControlBlock(_gifStream, _writer, delay);
            WriteImageBlock(_gifStream, _writer, !_firstFrame, 0, 0, _width, _height);
            
            if (_firstFrame)
                _firstFrame = false;
        }

        /// <summary>
        /// Writes a Image frame.
        /// </summary>
        /// <param name="image">Image frame to write.</param>
        public void WriteFrame(IBitmapFrame image) => WriteFrame(image, _defaultFrameDelay);
        
        /// <summary>
        /// <see cref="GifWriter"/> does not support Audio.
        /// </summary>
        public bool SupportsAudio => false;

        #region Write

        private void InitHeader(Stream sourceGif, BinaryWriter writer, int width, int height)
        {
            // File Header
            writer.Write("GIF".ToCharArray()); // File type
            writer.Write("89a".ToCharArray()); // File Version

            writer.Write((short)width); // Initial Logical Width
            writer.Write((short)height); // Initial Logical Height

            sourceGif.Position = SourceGlobalColorInfoPosition;
            writer.Write((byte)sourceGif.ReadByte()); // Global Color Table Info
            writer.Write((byte)0); // Background Color Index
            writer.Write((byte)0); // Pixel aspect ratio
            WriteColorTable(sourceGif, writer);

            // App Extension Header for Repeating
            if (_repeat == -1)
                return;

            writer.Write(unchecked((short)0xff21)); // Application Extension Block Identifier
            writer.Write((byte)0x0b); // Application Block Size
            writer.Write("NETSCAPE2.0".ToCharArray()); // Application Identifier
            writer.Write((byte)3); // Application block length
            writer.Write((byte)1);
            writer.Write((short)_repeat); // Repeat count for images.
            writer.Write((byte)0); // terminator
        }

        private static void WriteColorTable(Stream sourceGif, BinaryWriter writer)
        {
            sourceGif.Position = 13; // Locating the image color table
            var colorTable = new byte[768];
            sourceGif.Read(colorTable, 0, colorTable.Length);
            writer.Write(colorTable, 0, colorTable.Length);
        }

        private static void WriteGraphicControlBlock(Stream sourceGif, BinaryWriter writer, int frameDelay)
        {
            sourceGif.Position = 781; // Locating the source GCE
            var blockhead = new byte[8];
            sourceGif.Read(blockhead, 0, blockhead.Length); // Reading source GCE

            writer.Write(unchecked((short)0xf921)); // Identifier
            writer.Write((byte)0x04); // Block Size
            writer.Write((byte)(blockhead[3] & 0xf7 | 0x08)); // Setting disposal flag
            writer.Write((short)(frameDelay / 10)); // Setting frame delay
            writer.Write(blockhead[6]); // Transparent color index
            writer.Write((byte)0); // Terminator
        }

        private byte[] _buffer;
        private static readonly byte[] Header = new byte[11];

        private void WriteImageBlock(Stream sourceGif, BinaryWriter writer, bool includeColorTable, int x, int y, int width, int height)
        {
            sourceGif.Position = SourceImageBlockPosition; // Locating the image block
            
            sourceGif.Read(Header, 0, Header.Length);
            writer.Write(Header[0]); // Separator
            writer.Write((short)x); // Position X
            writer.Write((short)y); // Position Y
            writer.Write((short)width); // Width
            writer.Write((short)height); // Height

            if (includeColorTable) // If first frame, use global color table - else use local
            {
                sourceGif.Position = SourceGlobalColorInfoPosition;
                writer.Write((byte)(sourceGif.ReadByte() & 0x3f | 0x80)); // Enabling local color table
                WriteColorTable(sourceGif, writer);
            }
            else writer.Write((byte)(Header[9] & 0x07 | 0x07)); // Disabling local color table

            writer.Write(Header[10]); // LZW Min Code Size

            // Read/Write image data
            sourceGif.Position = SourceImageBlockPosition + Header.Length;

            var dataLength = sourceGif.ReadByte();
            while (dataLength > 0)
            {
                if (_buffer == null || _buffer.Length < dataLength)
                    _buffer = new byte[dataLength];
                                                
                sourceGif.Read(_buffer, 0, dataLength);
                
                writer.Write((byte)dataLength);
                writer.Write(_buffer, 0, dataLength);
                dataLength = sourceGif.ReadByte();
            }

            writer.Write((byte)0); // Terminator

            writer.Flush();
        }
        #endregion

        /// <summary>
        /// Frees all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            // Complete File
            _writer.Write((byte)0x3b); // File Trailer

            _writer.BaseStream.Dispose();
            _writer.Dispose();

            _gifStream.Dispose();

            _gifStream = null;
            _buffer = null;
        }
    }
}
