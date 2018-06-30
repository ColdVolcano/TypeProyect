using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Allocation;
using OpenTK.Graphics;
using osu.Framework.Platform;
using OpenTK;
using Newtonsoft.Json;
using System.IO;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Transforms;
using System;
using osu.Framework.Input;

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
        private Container barContainer;
        private Container progress;
        private SpriteText timeText;
        private SpriteText totalTimeText;
        private SpriteText hoverTimeText;
        private Bindable<bool> barsHovered = new Bindable<bool>();

        public ProgressContainer()
        {
            Height = bar_height;
            Origin = Anchor.BottomLeft;
            Children = new Drawable[]
            {
                timeText = new SpriteText
                {
                    AlwaysPresent = true,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Text = "0:00",
                    Font = "Exo2.0-SemiBold",
                    TextSize = 45,
                    Position = new Vector2(0, -45),
                },
                totalTimeText = new SpriteText
                {
                    AlwaysPresent = true,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Text = "x:xx",
                    Font = "Exo2.0-SemiBold",
                    TextSize = 45,
                    Position = new Vector2(0, -45),
                },
                hoverTimeText = new SpriteText
                {
                    RelativePositionAxes = Axes.X,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.BottomCentre,
                    Text = "0:00",
                    Font = "Exo2.0-SemiBold",
                    TextSize = 35,
                    Shadow = true,
                    Position = new Vector2(0, -15)
                },
                new Container
                {
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 100,
                    Masking = true,
                    Child = triangle = new EquilateralTriangle
                    {
                        Rotation = 180,
                        EdgeSmoothness = new Vector2(1),
                        RelativePositionAxes = Axes.X,
                        Size = new Vector2(13),
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.TopCentre,
                        Position = new Vector2(0, 13),
                    },
                },
                barContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = bar_height,
                    Masking = true,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = new Color4(25, 25, 25, 255),
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                        },
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

        private void trianglePopUp(bool value)
        {
            if (proyect.Metadata.Value?.Track?.IsLoaded ?? false)
            {
                triangle.MoveToY(value ? -1 : 13, 200, Easing.OutCubic);
                timeText.ResizeTextHeightTo(value ? 45 - 14f : 45, 200, Easing.OutCubic);
                if (!value)
                    timeText.FadeIn(200, Easing.OutCubic);
                xyz = false;
                totalTimeText.ResizeTextHeightTo(value ? 45 - 14f : 45, 200, Easing.OutCubic);
                if (!value)
                    totalTimeText.FadeIn(200, Easing.OutCubic);
                zyx = false;
                hoverTimeText.FadeTo(value ? 1 : 0, 200, Easing.OutCubic);
            }
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
            proyect.Cache(this);
            barsHovered.ValueChanged += trianglePopUp;
            barsHovered.TriggerChange();
        }

        private void playNext()
        {
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
            using (StreamReader r = new StreamReader(storage.GetStream(metadataList[next ? ++index : --index])))
            {
                AudioMetadata meta = JsonConvert.DeserializeObject<AudioMetadata>(r.ReadToEnd());
                meta.InitializeComponents(storage);
                proyect.Metadata.Value = meta;
                proyect.Audio.Track.AddItemToList(meta.Track);
                meta.Track.Restart();
            }
            totalTimeText.Text = formatTime(TimeSpan.FromMilliseconds(proyect.Metadata.Value?.Track?.Length ?? 0));
        }

        private string formatTime(TimeSpan t) => $"{Math.Floor(t.Duration().TotalMinutes)}:{t.Duration().Seconds:D2}";

        protected override void Update()
        {
            base.Update();
            barsHovered.Value = barContainer.IsHovered;

            var track = proyect.Metadata.Value?.Track;

            if (track?.IsLoaded ?? false)
            {
                float pp = (float)(track.CurrentTime / track.Length);
                progress.ResizeWidthTo(pp);
                //triangle.MoveToX(pp);
                timeText.Text = formatTime(TimeSpan.FromMilliseconds(track.CurrentTime));

                if (track.HasCompleted && !track.Looping)
                    playNext();
            }
        }

        private bool xyz = false;
        private bool zyx = false;

        protected override bool OnMouseMove(InputState state)
        {
            if (barsHovered)
            {
                float f = ToLocalSpace(state.Mouse.Position).X / barContainer.DrawSize.X;
                triangle.MoveToX(f);
                hoverTimeText.MoveToX(f);
                hoverTimeText.Text = formatTime(TimeSpan.FromMilliseconds((proyect.Metadata.Value?.Track?.Length ?? 0) * f));
                if (hoverTimeText.BoundingBox.IntersectsWith(timeText.BoundingBox))
                {
                    if (!xyz)
                    {
                        timeText.FadeOut(200, Easing.OutCubic);
                        xyz = true;
                    }
                }
                else if (xyz)
                {
                    timeText.FadeIn(200, Easing.OutCubic);
                    xyz = false;
                }
                if (hoverTimeText.BoundingBox.IntersectsWith(totalTimeText.BoundingBox))
                {
                    if (!zyx)
                    {
                        totalTimeText.FadeOut(200, Easing.OutCubic);
                        zyx = true;
                    }
                }
                else if (zyx)
                {
                    totalTimeText.FadeIn(200, Easing.OutCubic);
                    zyx = false;
                }
            }
            return base.OnMouseMove(state);
        }

        protected override bool OnDragStart(InputState state)
        {
            return true;
        }

        protected override bool OnClick(InputState state)
        {
            var t = proyect.Metadata.Value?.Track;
            if (barsHovered && (t?.IsLoaded ?? false))
            {
                float f = ToLocalSpace(state.Mouse.Position).X / barContainer.DrawSize.X;
                progress.ResizeWidthTo(f, 500, Easing.OutExpo);
                progress.Delay(500);
                t.Seek(f * t.Length);
            }
            return base.OnClick(state);
        }
    }

    public static class SpriteTextExtensions
    {
        public static TransformSequence<T> ResizeTextHeightTo<T>(this T drawable, float newHeight, double duration = 0, Easing easing = Easing.None) where T : SpriteText =>
           drawable.TransformTo(nameof(drawable.TextSize), newHeight, duration, easing);
    }
}
