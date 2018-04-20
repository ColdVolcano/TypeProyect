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

namespace TypeProyect.Screens.Pieces
{
    public class CoverContainer : Container
    {
        private Storage storage;
        private Sprite cover;
        private Sprite exchangeCover;
        private Container mainContainer;

        public Bindable<int> TrackIndex = new Bindable<int>(0);

        public CoverContainer()
        {
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
        }

        private void TrackChanged(int n)
        {
            Texture tempCover = new TextureStore(new RawTextureLoaderStore(new StorageBackedResourceStore(storage)), false).Get($"{n:00}.jpg");
            exchangeCover.Texture = cover.Texture;
            cover.Texture = tempCover;

            mainContainer.MoveToX(1).Then().MoveToX(0, 500, Easing.OutQuart);
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