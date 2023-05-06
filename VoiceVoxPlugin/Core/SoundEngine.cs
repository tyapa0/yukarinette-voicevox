using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.IO;
using System.Windows;

namespace VoiceVoxPlugin.Core
{
    class ISoundEngine : IDisposable
    {
        private WaveFileReader waveReader;
        private WaveOut waveOut;
        private int numDeviceId;

        public ISoundEngine(string deviceID) {
            numDeviceId = 0;

            //文字列に一致するデバイスIDを探す
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
  
                if (deviceID.StartsWith(WaveOut.GetCapabilities(i).ProductName))
                {
                    numDeviceId = i;
                    //MessageBox.Show(numDeviceId.ToString() + ":" + WaveOut.GetCapabilities(i).ProductName + " # " + deviceID, "numDeviceId", System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    break;
                }
            }
        }
        public bool IsCurrentlyPlaying() {
            if (waveOut.PlaybackState == PlaybackState.Playing) {
                return true;
            }
            else {
                return false;
            }
        }
        public bool Play2D(MemoryStream memory)
        {
            waveReader = new WaveFileReader(memory);
            waveOut = new WaveOut();

            // MessageBox.Show(numDeviceId.ToString(), "DeviceNumber", System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);
            waveOut.DeviceNumber = numDeviceId;
            waveOut.Init(waveReader);
            waveOut.Play();
            return true;
        }
        public void Dispose() { }
        //public string SoundDeviceId { get; }
    }

    public class ISoundDeviceList : IDisposable
    {
        private MMDeviceCollection devices;

        public ISoundDeviceList() {
            MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
            devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            DeviceCount = devices.Count;
        }

        public string getDeviceID(int num) { return devices[num].FriendlyName; }

        public string getDeviceDescription(int num) { return devices[num].FriendlyName; }

        public int DeviceCount { get; }

        public void Dispose() { }
    }
}
