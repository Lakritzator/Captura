using System;
using System.IO;
using System.Windows.Media;
using Captura.Base.Audio;
using Captura.Base.Services;
using Captura.Core.Settings.Models;

namespace Captura.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AudioPlayer : IAudioPlayer
    {
        private readonly SoundSettings _settings;
        private readonly MediaPlayer _mediaPlayer;

        public AudioPlayer(SoundSettings settings)
        {
            _settings = settings;
            _mediaPlayer = new MediaPlayer();
        }

        private void PlaySound(string path)
        {
            if (!File.Exists(path))
                return;

            _mediaPlayer.Open(new Uri(path));
            _mediaPlayer.Play();
        }

        public void Play(SoundKind soundKind)
        {
            if (_settings.Items.TryGetValue(soundKind, out var value))
                PlaySound(value);
        }
    }
}