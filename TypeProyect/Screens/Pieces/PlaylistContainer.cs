using Id3;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using System.Collections.Generic;
using System.IO;

namespace TypeProyect.Screens.Pieces
{
    public class PlaylistContainer : Container
    {
        private List<AudioMetadata> metadataList = new List<AudioMetadata>();
        private TypeProyect proyect;
        private Storage storage;
        private int index = -1;

        public PlaylistContainer()
        {
            AlwaysPresent = true;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(Storage storage, TypeProyect proyect)
        {
            this.storage = storage;
            this.proyect = proyect;
            if (!storage.Exists("list.json"))
                return;
            using (StreamReader r = new StreamReader(storage.GetStream("list.json")))
                foreach (string s in JsonConvert.DeserializeObject<List<string>>(r.ReadToEnd()))
                    using (StreamReader sr = new StreamReader(storage.GetStream(s)))
                        metadataList.Add(JsonConvert.DeserializeObject<AudioMetadata>(sr.ReadToEnd()));
            Schedule(PlayNext);
        }


        public void AddSong(AudioMetadata meta, string path)
        {
            string number = $"{ metadataList.Count + 1}";
            string s = "";
            using (var mp3 = new Mp3(path))
            {
                Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
                if (tag.Pictures.Count > 0)
                {
                    s = tag.Pictures[0].GetExtension();
                    if (s == "jpeg" || s == "jpg")
                        s = ".jpg";
                    else if (s == "png")
                        s = ".png";
                    using (var ds = storage.GetStream($"{number}{s}", FileAccess.Write, FileMode.Create))
                        tag.Pictures[0].SaveImage(ds);
                }

            }
            meta.CoverFiles.Add($"{number}{s}");
            using (var d = new FileStream(path, FileMode.Open))
            {
                using (var e = storage.GetStream($"{number}{Path.GetExtension(path)}", FileAccess.Write, FileMode.Create))
                {
                    byte[] arr = new byte[d.Length];
                    d.Read(arr, 0, arr.Length);
                    e.Write(arr, 0, arr.Length);
                }
            }
            meta.AudioFile = $"{number}{Path.GetExtension(path)}";
            metadataList.Add(meta);
            using (StreamWriter stream = new StreamWriter(storage.GetStream($"{number}.json", FileAccess.ReadWrite, FileMode.Create)))
                stream.Write(JsonConvert.SerializeObject(meta, Formatting.Indented));
            using (StreamWriter mainFile = new StreamWriter(storage.GetStream("list.json", FileAccess.Write, FileMode.Create)))
                mainFile.Write(JsonConvert.SerializeObject(metadataList, Formatting.Indented));
            if (!(proyect.Metadata.Value?.Track.IsRunning ?? false))
                PlayNext();
        }



        public void PlayNext()
        {
            if (metadataList.Count == 0)
                return;
            if (index == metadataList.Count - 1)
                index = -1;

            play(true);
        }

        private void playBefore()
        {
            if (index == 0)
                index = metadataList.Count + 1;

            play(false);
        }

        private void play(bool next)
        {
            AudioMetadata meta = metadataList[next ? ++index : --index];
            meta.InitializeComponents(storage);
            proyect.Audio.Track.AddItemToList(meta.Track);
            if (proyect.Metadata.Value?.Track != null)
            {
                proyect.Metadata.Value.Track.Stop();
                proyect.Metadata.Value.Track.Dispose();
            }
            proyect.Metadata.Value = meta;
            meta.Track.Restart();
        }
    }
}
