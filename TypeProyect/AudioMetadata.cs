using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;

namespace TypeProyect
{
    public class AudioMetadata
    {
        [JsonProperty("covers")]
        private List<string> covers = new List<string>();

        [JsonProperty("audio")]
        private string audio = "01.mp3";

        [JsonProperty("title")]
        public string Title = "May I Help You?";

        [JsonProperty("titleUnicode")]
        public string TitleUnicode = "めいあいへるぷゆー？";

        [JsonProperty("artist")]
        public string Artist = "Yamagami Lucy..., Miyoshi Saya, Chihaya Megumi";

        [JsonProperty("artistUnicode")]
        public string ArtistUnicode = "山神ルーシー…、三好紗耶、千早恵";

        [JsonProperty("lyrics")]
        public Lyrics<LyricPhrase> Lyrics;

        [JsonProperty("sideLyrics")]
        public SideLyrics SideLyrics;

        [JsonIgnore]
        public List<Texture> Covers = new List<Texture>();

        [JsonIgnore]
        public Track Track;

        public void InitializeComponents(Storage storage)
        {
            Track = new TrackBass(storage.GetStream(audio));
            var te = new TextureStore(new RawTextureLoaderStore(new StorageBackedResourceStore(storage)), false);
            foreach (var s in covers)
            {
                Covers.Add(te.Get(s));
            }
        }
    }
}
