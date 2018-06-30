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
        public List<string> CoverFiles = new List<string>();

        [JsonProperty("audio")]
        public string AudioFile = "";

        [JsonProperty("title")]
        public string Title = "";

        [JsonProperty("titleUnicode")]
        public string TitleUnicode = "";

        [JsonProperty("artist")]
        public string Artist = "";

        [JsonProperty("artistUnicode")]
        public string ArtistUnicode = "";

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
            Track = new TrackBass(storage.GetStream(AudioFile));
            var te = new TextureStore(new RawTextureLoaderStore(new StorageBackedResourceStore(storage)), false);
            foreach (var s in CoverFiles)
            {
                Covers.Add(te.Get(s));
            }
        }
    }
}
