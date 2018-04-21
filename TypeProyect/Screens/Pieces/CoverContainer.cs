using osu.Framework.Graphics.Containers;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.IO.Stores;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Configuration;
using System.Collections.Generic;
using osu.Framework.Audio.Track;

namespace TypeProyect.Screens.Pieces
{
    public class CoverContainer : Container
    {
        private Storage storage;
        private readonly List<Texture> textures = new List<Texture>();
        private readonly Sprite cover;
        private readonly Sprite exchangeCover;
        private readonly Container mainContainer;
        private int coverIndex = 0;

        public Bindable<int> TrackIndex = new Bindable<int>(0);
        private Bindable<Track> track = new Bindable<Track>();
        private int nextChangeTime;

        public CoverContainer(Bindable<Track> track)
        {
            this.track.BindTo(track);
            Masking = true;

            Children = new Drawable[]
            {
                new Container
                {
                    Masking = true,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new UnknownAlbumArt(),
                        exchangeCover = new Sprite
                        {
                            RelativePositionAxes = Axes.Both,
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fill,
                        }
                    }
                },
                mainContainer = new Container
                {
                    Masking = true,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new UnknownAlbumArt(),
                        cover = new Sprite
                        {
                            RelativePositionAxes = Axes.Both,
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fill,
                        }
                    }
                }
            };

            TrackIndex.ValueChanged += TrackChanged;
            track.ValueChanged += restoreChanges;
        }

        private void restoreChanges(Track t)
        {
            nextChangeTime = 30000;
        }

        private void TrackChanged(int n)
        {
            var te = new TextureStore(new RawTextureLoaderStore(new StorageBackedResourceStore(storage)), false);

            textures.Clear();
            textures.Add(te.Get($"{n:00}.jpg"));

            for (int i = 1; storage.Exists($"{n:00}-{i}.jpg"); i++)
                textures.Add(te.Get($"{n:00}-{i}.jpg"));

            exchangeCover.Texture = cover.Texture;
            cover.Texture = textures[coverIndex = 0];

            mainContainer.MoveToX(1).Then().MoveToX(0, 500, Easing.OutQuart);
        }

        protected override void Update()
        {
            base.Update();

            if (track.Value.CurrentTime > nextChangeTime && textures.Count > 1)
            {
                if (coverIndex >= textures.Count - 1)
                    coverIndex = -1;
                this.ScaleTo(new Vector2(0, 1), 750 / 2f, Easing.InExpo)
                    .Then()
                    .ScaleTo(1, 750 / 2f, Easing.OutExpo);
                Scheduler.AddDelayed(() => cover.Texture = textures[++coverIndex], 750 / 2f);
                int proposedCoverChangeTime = ((int)track.Value.CurrentTime / 30000 + 1) * 30000;
                nextChangeTime = proposedCoverChangeTime + 750 > track.Value.Length ? int.MaxValue : proposedCoverChangeTime;
            }
        }

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            this.storage = storage;
        }

        private class UnknownAlbumArt : BufferedContainer
        {
            public UnknownAlbumArt()
            {
                CacheDrawnFrameBuffer = true;
                RelativePositionAxes = Axes.Both;
                RelativeSizeAxes = Axes.Both;
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    },
                    new UnkownCircle(true)
                    {
                        Size = new Vector2(0.9f)
                    },
                    new UnkownCircle(false)
                    {
                        Size = new Vector2(0.25f)
                    },
                    new UnkownCircle(true)
                    {
                        Size = new Vector2(0.23f)
                    },
                    new UnkownCircle(false)
                    {
                        Size = new Vector2(0.1f)
                    }
                };
            }

            private class UnkownCircle : CircularContainer
            {
                private static Color4 g = new Color4(70, 70, 70, 255);

                public UnkownCircle(bool gray)
                {
                    Masking = true;
                    RelativeSizeAxes = Axes.Both;
                    Anchor = Anchor.Centre;
                    Origin = Anchor.Centre;
                    Children = new Drawable[]
                    {
                    new Box
                    {
                        Colour = gray ? g : Color4.Black,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = ((char)0xf128).ToString(),
                        Colour = gray ? Color4.Black : g,
                        TextSize = 210,
                    }
                    };
                }
            }
        }
    }
}