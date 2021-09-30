using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using IrrKlang;
using Newtonsoft.Json;
using VoiceVoxPlugin.Data;
using VoiceVoxPlugin.Properties;
using VoiceVoxPlugin.UI;
using VoiceVoxPlugin.ViewModel;
using Yukarinette;

namespace VoiceVoxPlugin
{
    public class Plugin : IYukarinetteInterface
    {
        public override string Name => "VoiceVox 連携プラグイン";
        public override string GUID => "D90253B4-7074-44F9-A957-C393FC23260D";
        private Window Window { get; } = new Window();
        private SettingWindow SettingWindow { get; set; }
        private HttpClient HttpClient { get; } = new HttpClient();
        private List<SoundDevice> SoundDevices { get; } = new List<SoundDevice>();

        private void Logging(string text)
        {
            YukarinetteLogger.Instance?.Info(text);
        }

        public override void Loaded()
        {
            Logging("Loaded Start.");

            var setting = SettingWindowViewModel.Instance;
            setting.SpeakerId = Settings.Default.SpeakerId;
            setting.SoundDeviceId = Settings.Default.SoundDeviceId;
            // setting.SpeedScale = Settings.Default.SpeedScale;

            using (var soundDevices = new ISoundDeviceList(SoundDeviceListType.PlaybackDevice))
            {
                foreach (var i in Enumerable.Range(0, soundDevices.DeviceCount))
                {
                    var id = soundDevices.getDeviceID(i);
                    var description = soundDevices.getDeviceDescription(i);
                    var soundDevice = new SoundDevice(id, description);
                    SoundDevices.Add(soundDevice);
                }
            }

            Logging("Loaded End.");
        }

        public override void Closed()
        {
            Logging("Loaded Start.");

            var setting = SettingWindowViewModel.Instance;
            Settings.Default.SpeakerId = setting.SpeakerId;
            Settings.Default.SoundDeviceId = setting.SoundDeviceId;
            Settings.Default.Save();

            Logging("Loaded End.");
        }

        public override void Setting()
        {
            Logging("Setting Start.");

            var window = new OptionWindow(Settings.Default.ExitWhenFinished, Settings.Default.ExePath);
            if (window.ShowDialog() ?? false)
            {
                Settings.Default.ExitWhenFinished = window.ExitWhenFinished;
                Settings.Default.ExePath = window.ExePath;
            }

            Logging("Setting End.");
        }

        public override async void SpeechRecognitionStart()
        {
            Logging("SpeechRecognitionStart Start.");

            var processes = Process.GetProcessesByName("VOICEVOX");
            if (!processes.Any())
            {
                if (!string.IsNullOrWhiteSpace(Settings.Default.ExePath))
                {
                    Process.Start(Settings.Default.ExePath);
                }
            }

            var isSuccess = false;
            if (HttpClient.BaseAddress == null)
            {
                HttpClient.BaseAddress = new Uri("http://127.0.0.1:50021");
            }

            SettingWindowViewModel.Instance.SpeakerId = Settings.Default.SpeakerId;
            SettingWindowViewModel.Instance.SoundDeviceId = Settings.Default.SoundDeviceId;
            foreach (var i in Enumerable.Range(0, 120))
            {
                try
                {
                    var response = await HttpClient.GetAsync("/speakers"); ;
                    var str = await response.Content.ReadAsStringAsync();
                    var speakers = JsonConvert.DeserializeObject<IEnumerable<Speaker>>(str);
                    var list1 = (speakers ?? Enumerable.Empty<Speaker>()).ToList();
                    var list2 = SoundDevices.ToList();
                    foreach (var s in list1)
                    {
                        SettingWindowViewModel.Instance.Speakers.Add(s);
                    }
                    foreach (var s in list2)
                    {
                        SettingWindowViewModel.Instance.SoundDevices.Add(s);
                    }

                    isSuccess = true;
                }
                catch (HttpRequestException ex)
                {

                    await Task.Delay(1_000);
                    continue;
                }

                break;
            }

            if (!isSuccess)
            {
                MessageBox.Show("VOICE VOX に接続できませんでした");
            }

            Window.Dispatcher.Invoke(() =>
            {
                SettingWindow = new SettingWindow();
                SettingWindow.Show();
            });

            Logging("SpeechRecognitionStart End.");
        }

        public override void SpeechRecognitionStop()
        {
            Logging("SpeechRecognitionStop Start.");

            Window.Dispatcher.Invoke(() =>
            {
                SettingWindow?.Close();
                Settings.Default.SpeakerId = SettingWindowViewModel.Instance.SpeakerId;
                Settings.Default.SoundDeviceId = SettingWindowViewModel.Instance.SoundDeviceId;
                SettingWindowViewModel.Clear();

                if (Settings.Default.ExitWhenFinished)
                {
                    var processes = Process.GetProcessesByName("VOICEVOX");
                    foreach (var process in processes)
                    {
                        process.Kill();
                    }
                }
            });

            Logging("SpeechRecognitionStop End.");
        }

        public override async void Speech(string text)
        {
            Logging($"Speech Start. {text}");

            text = text.Replace(" ", "").Replace("　", "");

            var parameters1 = new Dictionary<string, string>
            {
                { "text", text },
                { "speaker", SettingWindowViewModel.Instance.SpeakerId.ToString() },
            };

            var response1 = await HttpClient.PostAsync(
                $"/audio_query?{await new FormUrlEncodedContent(parameters1).ReadAsStringAsync()}",
                new StringContent(""));

            if (response1.IsSuccessStatusCode)
            {
                var json = await response1.Content.ReadAsStringAsync();
                // var d = JsonConvert.DeserializeObject<dynamic>(json);
                // var j = JsonConvert.SerializeObject(d, Formatting.Indented);
                // MessageBox.Show(j);
                var result = JsonConvert.DeserializeObject<QueryResult>(json) ?? new QueryResult();
                result.SpeedScale = SettingWindowViewModel.Instance.SpeedScale;
                result.PitchScale = SettingWindowViewModel.Instance.PitchScale;
                result.IntonationScale = SettingWindowViewModel.Instance.IntonationScale;
                result.VolumeScale = SettingWindowViewModel.Instance.VolumeScale;

                var parameters2 = new Dictionary<string, string>
                {
                    { "speaker", SettingWindowViewModel.Instance.SpeakerId.ToString() },
                };

                var sendJson = JsonConvert.SerializeObject(result, Formatting.Indented);
                var sendContent = new StringContent(sendJson, new UTF8Encoding(false), "application/json");
                var response2 = await HttpClient.PostAsync(
                    $"/synthesis?{await new FormUrlEncodedContent(parameters2).ReadAsStringAsync()}",
                    sendContent);

                using (var memory = new MemoryStream())
                {
                    await response2.Content.CopyToAsync(memory);
                    await memory.FlushAsync();
                    memory.Seek(0, SeekOrigin.Begin);
                
                    using (var engine = new ISoundEngine(SoundOutputDriver.AutoDetect, SoundEngineOptionFlag.DefaultOptions, SettingWindowViewModel.Instance.SoundDeviceId))
                    {
                        var sound = engine.GetSoundSource("sound.wav");
                        if (sound != null)
                        {
                            engine.RemoveSoundSource("sound.wav");
                        }
                        
                        engine.AddSoundSourceFromIOStream(memory, "sound.wav");
                        var status = engine.Play2D("sound.wav");
                        while (!status.Finished)
                        {
                            await Task.Delay(1);
                        }
                    }
                }
            }

            Logging("Speech End.");
        }
    }
}