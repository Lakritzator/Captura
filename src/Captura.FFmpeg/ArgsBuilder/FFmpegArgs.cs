﻿using System.Collections.Generic;

namespace Captura.FFmpeg.ArgsBuilder
{
    public abstract class FFmpegArgs
    {
        protected readonly List<string> Args = new List<string>();

        public virtual string GetArgs()
        {
            return string.Join(" ", Args);
        }
    }
}