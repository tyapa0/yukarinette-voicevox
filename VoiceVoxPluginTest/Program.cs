using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IrrKlang;
using Newtonsoft.Json;

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

            using (var engine = new ISoundEngine(SoundOutputDriver.AutoDetect, SoundEngineOptionFlag.DefaultOptions))
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
                        { "speaker", s.SpeakerId.ToString() },
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
                        { "speaker", s.SpeakerId.ToString() },
                    };
                    var content3 = new StringContent(json3, new UTF8Encoding(false), "application/json");
                    var response3 = await client.PostAsync(
                        $"/synthesis?{await new FormUrlEncodedContent(parameters3).ReadAsStringAsync()}", content3);
                    if (!response3.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Failed 3");
                    }

                    using (var memory = new MemoryStream())
                    {
                        await response3.Content.CopyToAsync(memory);
                        await memory.FlushAsync();
                        memory.Seek(0, SeekOrigin.Begin);

                        var sound = engine.GetSoundSource("sound.wav");
                        if (sound != null)
                        {
                            engine.RemoveSoundSource("sound.wav");
                        }
                        engine.AddSoundSourceFromIOStream(memory, "sound.wav");
                        engine.Play2D("sound.wav");
                    }
                }
            }
        }

        class Speaker
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("speaker_id")]
            public int SpeakerId { get; set; }
        }
    }
}
