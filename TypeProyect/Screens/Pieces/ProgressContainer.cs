using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Allocation;
using osu.Framework.IO;
using OpenTK.Graphics;
using osu.Framework.Platform;
using OpenTK;
using Newtonsoft.Json;
using System.IO;
using osu.Framework.Graphics.Sprites;

namespace TypeProyect.Screens.Pieces
{
    public class ProgressContainer : Container
    {
        private Storage storage;
        private TypeProyect proyect;
        private const float bar_height = 40;
        private EquilateralTriangle triangle;
        private List<string> metadataList = new List<string>();
        private int index = -1;
        private Container progress;
        private SpriteText timeText;

        public ProgressContainer()
        {
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                timeText = new SpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Text = "0:00",
                    Font = "Exo2.0-Bold",
                    TextSize = 35,
                },
                triangle = new EquilateralTriangle
                {
                    Rotation = 180,
                    EdgeSmoothness = new Vector2(1),
                    RelativePositionAxes = Axes.X,
                    Size = new Vector2(13),
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopCentre,
                    Position = new Vector2(0, 37),
                },
                new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Colour = new Color4(25, 25, 25, 255),
                    Height = bar_height,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 40,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        progress = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0, 1),
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.White,
                                    EdgeSmoothness = new Vector2(1),
                                },
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(Storage storage, TypeProyect proyect)
        {
            this.storage = storage;
            this.proyect = proyect;
            if (!storage.Exists("list.json"))
            {
                //proyect.ShowNoSongs();
                return;
            }
            using (StreamReader r = new StreamReader(storage.GetStream("list.json")))
                metadataList = JsonConvert.DeserializeObject<List<string>>(r.ReadToEnd());
            playNext();
        }

        private void playNext()
        {
            if (index == metadataList.Count - 1)
                index = -1;

            using (StreamReader r = new StreamReader(storage.GetStream(metadataList[++index])))
            {
                AudioMetadata meta = JsonConvert.DeserializeObject<AudioMetadata>(r.ReadToEnd());
                meta.InitializeComponents(storage);
                proyect.Metadata.Value = meta;
                proyect.Audio.Track.AddItemToList(meta.Track);
                meta.Track.Restart();
            }
        }

        protected override void Update()
        {
            base.Update();
            
            var track = proyect.Metadata.Value?.Track;

            if (track?.IsLoaded ?? false)
            {
                //Change time position

                if (track.HasCompleted && !track.Looping)
                    playNext();
            }
        }
    }
}
