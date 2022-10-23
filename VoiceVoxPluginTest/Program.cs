using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace VoiceVoxPluginTest
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            foreach (var process in Process.GetProcesses().OrderBy(p => p.ProcessName)) 
            {
                Console.WriteLine(process.ProcessName);
            }

            Console.ReadLine();

            try
            {
                await Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.ReadLine();
        }

        static async Task Run()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://127.0.0.1:50021");
            var response1 = await client.GetAsync("/speakers");
            if (!response1.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed 1");
            }

            using (var engine = new ISoundEngine())
            {
                while (true)
                {
                    var word = Console.ReadLine();
                    if (string.IsNullOrEmpty(word))
                    {
                        break;
                    }

                    var str = await response1.Content.ReadAsStringAsync();
                    var speakers = JsonConvert.DeserializeObject<IEnumerable<Speaker>>(str) ?? new List<Speaker>();
                    var s = speakers.ElementAt(0);
                    var parameters2 = new Dictionary<string, string>()
                    {
                        { "text", word },
                        { "speaker", s.Styles[0].SpeakerId.ToString() },
                    };

                    var response2 = await client.PostAsync(
                        $"/audio_query?{await new FormUrlEncodedContent(parameters2).ReadAsStringAsync()}",
                        new StringContent(""));
                    if (!response2.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Failed 2");
                    }

                    var json2 = await response2.Content.ReadAsStringAsync();
                    var obj = JsonConvert.DeserializeObject<object>(json2);
                    var json3 = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    Console.WriteLine(json3);

                    var parameters3 = new Dictionary<string, string>()
                    {
                        { "speaker", s.Styles[0].SpeakerId.ToString() },
                    };
                    var content3 = new StringContent(json3, new UTF8Encoding(false), "application/json");
                    var response3 = await client.PostAsync(
                        $"/synthesis?{await new FormUrlEncodedContent(parameters3).ReadAsStringAsync()}", content3);
                    if (!response3.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Failed 3");
                    }

                    //naudio test code
                    MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
                    var devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                    string devname = devices[0].FriendlyName;

                    using (var memory = new MemoryStream())
                    {
                        await response3.Content.CopyToAsync(memory);
                        await memory.FlushAsync();
                        memory.Seek(0, SeekOrigin.Begin);

                        //var player = new System.Media.SoundPlayer(memory);
                        //player.PlaySync();

                        WaveFileReader waveReader = new WaveFileReader(memory);
                        WaveOut waveOut = new WaveOut();

                        waveOut.DeviceNumber = 0;
                        waveOut.Init(waveReader);
                        waveOut.Play();
                        while (waveOut.PlaybackState == PlaybackState.Playing) ;
                    }
                }
            }
        }

        public class Speaker
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("speaker_uuid")]
            public string SpeakerUUId { get; set; }

            [JsonProperty("styles")]
            public VoiceVoxSpeakerStyle[] Styles { get; set; }
        }
        public class VoiceVoxSpeakerStyle
        {
            [JsonProperty("name")]
            public string StyleName { get; set; }

            [JsonProperty("id")]
            public int SpeakerId { get; set; }
        }
        class ISoundEngine : IDisposable
        {
            public ISoundEngine() { }

            public bool Play2D(MemoryStream memory)
            {

                return true;
            }
            public void Dispose() { }
        }
    }
}
