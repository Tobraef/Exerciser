using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Plugin.SimpleAudioPlayer;

namespace App1.Sound
{
    public class SoundHandler
    {
        private ISimpleAudioPlayer player_ = CrossSimpleAudioPlayer.Current;
        private Stream currentStream_;
        private long currentPosition_;

        public SoundHandler()
        {
            player_.PlaybackEnded += (o, e) => PlaybackFinished();
            player_.Loop = false;
            PlaybackFinished += () =>
            {
                currentStream_?.Close();
                currentStream_ = null;
            };
        }

        Stream GetStreamFromFile(string filename)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream("App1.Sound." + filename);

            return stream;
        }

        private void PlaySound(string rawFileName = "")
        {
            player_.Loop = false;
            var stream = GetStreamFromFile(rawFileName);
            if (stream == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to to find file");
                return;
            }
            player_.Load(stream);
            player_.Play();
        }

        public event Action PlaybackFinished;

        public void Play(string file)
        {
            if (currentStream_ != null)
            {
                currentStream_.Close();
            }
            currentStream_ = new FileStream(file, FileMode.Open);
            player_.Load(currentStream_);
            player_.Play();
        }

        public void Stop()
        {
            player_.Stop();
        }

        public void Resume()
        {
            player_.Load(currentStream_);
            player_.Play();
        }

        public void PlayGo()
        {
            PlaySound("letsgo.wav");
        }

        public void PlayStop()
        {
            PlaySound("stop.wav");
        }
    }
}
