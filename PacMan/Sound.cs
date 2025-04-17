using System;
using NAudio.Wave;

namespace PacMan
{
    public class Sound
    {
        private string filePath;
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private LoopStream loopStream;

        public Sound(string path)
        {
            filePath = path;
        }

        public void Play()
        {
            Stop(); // ���ɍĐ����Ȃ��~

            outputDevice = new WaveOutEvent();
            audioFile = new AudioFileReader(filePath);
            outputDevice.Init(audioFile);
            outputDevice.Play();
        }

        public void Loop()
        {
            Stop(); // ���ɍĐ����Ȃ��~

            outputDevice = new WaveOutEvent();
            audioFile = new AudioFileReader(filePath);
            loopStream = new LoopStream(audioFile);
            outputDevice.Init(loopStream);
            outputDevice.Play();
        }

        public void Stop()
        {
            outputDevice?.Stop();
            outputDevice?.Dispose();
            outputDevice = null;

            audioFile?.Dispose();
            audioFile = null;

            loopStream?.Dispose();
            loopStream = null;
        }
    }
}
