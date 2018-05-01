using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;

namespace TypeProyect.Screens.Pieces
{
    public class CoverContainer : Container
    {
        private Storage storage;
        private readonly Sprite cover;
        private readonly Sprite exchangeCover;
        private readonly Container mainContainer;
        private readonly Container subContainer;
        private int coverIndex = 0;

        private Bindable<AudioMetadata> metadata = new Bindable<AudioMetadata>();

        AudioMetadata lastMetadata;

        public CoverContainer()
        {
            Masking = true;

            Children = new Drawable[]
            {
                subContainer = new Container
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

            metadata.ValueChanged += restoreChanges;
        }

        private void restoreChanges(AudioMetadata newMeta)
        {
            Scheduler.Add(() => mainContainer.ScaleTo(1).Then().Delay(30000 - newMeta.Track.CurrentTime + Time.Elapsed).Then().ScaleTo(1).OnComplete(_ => UpdateCover()));

            exchangeCover.Texture = cover.Texture;
            cover.Texture = newMeta.Covers[coverIndex = 0];

            if (lastMetadata != null)
            {
                subContainer.Show();
                mainContainer.MoveToX(1).Then().MoveToX(0, 500, Easing.OutQuart).OnComplete(_ => subContainer.Hide());
            }
            else
                subContainer.Hide();
            lastMetadata = newMeta;
        }

        private void UpdateCover()
        {
            base.Update();

            var textures = metadata.Value?.Covers;
            var track = metadata.Value?.Track;
            if (textures?.Count > 1 && track.Length - track.CurrentTime > 2000)
            {
                if (coverIndex >= textures.Count - 1)
                    coverIndex = -1;
                mainContainer.ScaleTo(new Vector2(0, 1), 750 / 2f, Easing.InExpo)
                    .OnComplete(_ => cover.Texture = textures[++coverIndex]);
                mainContainer.Delay(750 / 2f)
                    .Then()
                    .ScaleTo(1, 750 / 2f, Easing.OutExpo)
                    .Then()
                    .Delay(30000 - 750)
                    .Then()
                    .ScaleTo(1)
                    .OnComplete(_ => UpdateCover());
            }
        }

        [BackgroundDependencyLoader]
        private void load(Storage storage, TypeProyect proyect)
        {
            this.storage = storage;
            metadata.BindTo(proyect.Metadata);
        }

        private class UnknownAlbumArt : BufferedContainer
        {
            public UnknownAlbumArt()
            {
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