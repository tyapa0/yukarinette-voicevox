using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using IrrKlang;
using Newtonsoft.Json;
using VoiceVoxPlugin.Core;
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
        private HttpClient HttpClient { get; } = new HttpClient() { Timeout = new TimeSpan(0, 1, 0) };
        private List<SoundDevice> SoundDevices { get; } = new List<SoundDevice>();
        private bool IsBusy { get; set; }
        private ISoundEngine Engine { get; set; }
        private string VOICE_VOX_URL = "http://127.0.0.1:50021";

        private void Logging(string text)
        {
            YukarinetteLogger.Instance?.Info(text);
        }

        public override void Loaded()
        {
            Logging("Loaded Start.");
            Settings.Default.Reload();  

            var setting = SettingWindowViewModel.Instance;
            setting.SpeakerId = Settings.Default.SpeakerId;

            SoundDevices.AddRange(FetchSoundDevices());

            Logging("Loaded End.");
        }

        private IEnumerable<SoundDevice> FetchSoundDevices()
        {
            using (var soundDevices = new ISoundDeviceList(SoundDeviceListType.PlaybackDevice))
            {
                foreach (var i in Enumerable.Range(0, soundDevices.DeviceCount))
                {
                    var id = soundDevices.getDeviceID(i);
                    var description = soundDevices.getDeviceDescription(i);
                    var soundDevice = new SoundDevice(id, description);

                    yield return soundDevice;
                }
            }
        }

        public override void Closed()
        {
            Logging("Closed Start.");

            Logging("Closed End.");
        }

        public override void Setting()
        {
            Logging("Setting Start.");

            var window = new OptionWindow(SoundDevices);
            window.ShowDialog();

            Logging("Setting End.");
        }

        public override void SpeechRecognitionStart()
        {
            Logging("SpeechRecognitionStart Start.");
            IsBusy = true;

            Engine = new ISoundEngine(
                SoundOutputDriver.AutoDetect,
                SoundEngineOptionFlag.DefaultOptions,
                Settings.Default.SoundDeviceId);

            try
            {
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
                    HttpClient.BaseAddress = new Uri(VOICE_VOX_URL);
                }

                SettingWindowViewModel.Instance.SpeakerId = Settings.Default.SpeakerId;

                var loop = Math.Max(1, Settings.Default.VoiceVoxTimeout);
                foreach (var i in Enumerable.Range(0, loop))
                {
                    if (!IsBusy)
                    {
                        throw new MyException("");
                    }

                    try
                    {
                        var response = HttpClient.GetAsync("/speakers").Synchronous();
                        var str = response.Content.ReadAsStringAsync().Synchronous();
                        var speakers = JsonConvert.DeserializeObject<IEnumerable<Speaker>>(str);
                        var list1 = (speakers ?? Enumerable.Empty<Speaker>()).ToList();
                        foreach (var s in list1)
                        {
                            SettingWindowViewModel.Instance.Speakers.Add(s);
                        }

                        isSuccess = true;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(1_000);
                        continue;
                    }

                    break;
                }

                if (!isSuccess)
                {
                    throw new MyException("VOICE VOX に接続できませんでした");
                }

                SettingWindowViewModel.Instance.SpeedScale = Settings.Default.SpeedScale;
                SettingWindowViewModel.Instance.PitchScale = Settings.Default.PitchScale;
                SettingWindowViewModel.Instance.IntonationScale = Settings.Default.IntonationScale;
                SettingWindowViewModel.Instance.VolumeScale = Settings.Default.VolumeScale;

                Window.Dispatcher.Invoke(() =>
                {
                    SettingWindow = new SettingWindow();
                    SettingWindow.Show();
                });
            }
            catch (MyException ex)
            {
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    MessageBox.Show(ex.Message, "起動エラー", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                using (Engine)
                {
                }
                Engine = null;
            }

            Logging("SpeechRecognitionStart End.");
        }

        public override void SpeechRecognitionStop()
        {
            Logging("SpeechRecognitionStop Start.");
            IsBusy = false;

            using (Engine)
            {
            }
            Engine = null;

            Window.Dispatcher.Invoke(() =>
            {
                SettingWindow?.Close();
                SettingWindow = null;
                Settings.Default.SpeakerId = SettingWindowViewModel.Instance.SpeakerId;
                // Settings.Default.SoundDeviceId = SettingWindowViewModel.Instance.SoundDeviceId;
                Settings.Default.SpeedScale = SettingWindowViewModel.Instance.SpeedScale;
                Settings.Default.PitchScale = SettingWindowViewModel.Instance.PitchScale;
                Settings.Default.IntonationScale = SettingWindowViewModel.Instance.IntonationScale;
                Settings.Default.VolumeScale = SettingWindowViewModel.Instance.VolumeScale;
                Settings.Default.Save();
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

        public override void Speech(string text)
        {
            Logging($"Speech Start. {text}");
            text = text.Replace(" ", "").Replace("　", "");

            if (Engine == null)
            {
                Logging("Engine is null");
                Logging("Speech End.");
                return;
            }

            const int MAX = 10;
            QueryResult result = null;

            foreach (var i in Enumerable.Range(1, MAX))
            {
                if (result != null)
                {
                    break;
                }

                try
                {
                    var parameters1 = new Dictionary<string, string>
                    {
                        { "text", text },
                        { "speaker", SettingWindowViewModel.Instance.SpeakerId.ToString() },
                    };
                    
                    var stringParameter1 = string.Join("&", parameters1.Select(p => $"{Uri.EscapeUriString(p.Key)}={Uri.EscapeUriString(p.Value)}"));
                    var uri1 = $"/audio_query?{stringParameter1}";
                    var response1 = HttpClient.PostAsync(uri1, new StringContent("")).Synchronous();

                    if (!response1.IsSuccessStatusCode)
                    {
                        break;
                    }

                    var json = response1.Content.ReadAsStringAsync().Synchronous();
                    result = JsonConvert.DeserializeObject<QueryResult>(json);
                }
                catch (Exception ex)
                {
                    if (i == MAX)
                    {
                        Logging(ex.ToString());
                        MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        Logging(ex.ToString());
                        continue;
                    }
                }

                break;
            }

            if (result == null)
            {
                Logging("Speech End.");
                return;
            }

            result.SpeedScale = SettingWindowViewModel.Instance.SpeedScale;
            result.PitchScale = SettingWindowViewModel.Instance.PitchScale;
            result.IntonationScale = SettingWindowViewModel.Instance.IntonationScale;
            result.VolumeScale = SettingWindowViewModel.Instance.VolumeScale;

            foreach (var i in Enumerable.Range(1, MAX))
            {
                try
                {
                    var parameters2 = new Dictionary<string, string>
                    {
                        { "speaker", SettingWindowViewModel.Instance.SpeakerId.ToString() },
                    };

                    var sendJson = JsonConvert.SerializeObject(result, Formatting.Indented);
                    var sendContent = new StringContent(sendJson, new UTF8Encoding(false), "application/json");
                    var stringParameter2 = string.Join("&", parameters2.Select(p => $"{Uri.EscapeUriString(p.Key)}={Uri.EscapeUriString(p.Value)}"));
                    var uri2 = $"/synthesis?{stringParameter2}";
                    var response2 = HttpClient.PostAsync(uri2, sendContent).Synchronous();
                    if (!response2.IsSuccessStatusCode)
                    {
                        return;
                    }

                    using (var memory = new MemoryStream())
                    {
                        response2.Content.CopyToAsync(memory).Synchronous();
                        memory.Flush();
                        memory.Seek(0, SeekOrigin.Begin);

                        var soundName = $"sound{DateTime.Now:yyyyMMddHHmmssfff}.wav";
                        Engine.AddSoundSourceFromIOStream(memory, soundName);
                        Engine.Play2D(soundName);

                        while (Engine.IsCurrentlyPlaying(soundName))
                        {
                            Thread.Sleep(1);
                        }

                        Engine.RemoveSoundSource(soundName);
                    }
                }
                catch (Exception ex)
                {
                    if (i == MAX)
                    {
                        Logging(ex.ToString());
                        MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        continue;
                    }
                }

                break;
            }

            Logging("Speech End.");
        }
    }
}