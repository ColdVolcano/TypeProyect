using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using System;
using System.Threading.Tasks;

namespace TypeProyect.Screens.Pieces
{
    public class ProgressContainer : Container
    {
        private TypeProyect proyect;
        private Bindable<AudioMetadata> metadata = new Bindable<AudioMetadata>();
        private const float bar_height = 40;
        private EquilateralTriangle triangle;
        private Container barContainer;
        private Container progress;
        private SpriteText timeText;
        private SpriteText totalTimeText;
        private SpriteText hoverTimeText;

        public ProgressContainer()
        {
            Height = bar_height;
            Origin = Anchor.BottomLeft;
            Children = new Drawable[]
            {
                timeText = new SpriteText
                {
                    AlwaysPresent = true,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Text = "0:00",
                    Font = "Exo2.0-SemiBold",
                    TextSize = 45,
                    Position = new Vector2(0, -45),
                },
                totalTimeText = new SpriteText
                {
                    AlwaysPresent = true,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Text = "x:xx",
                    Font = "Exo2.0-SemiBold",
                    TextSize = 45,
                    Position = new Vector2(0, -45),
                },
                hoverTimeText = new SpriteText
                {
                    Alpha = 0,
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
        [BackgroundDependencyLoader]
        private void load(TypeProyect proj)
        {
            proyect = proj;
            metadata.BindTo(proj.Metadata);
            metadata.ValueChanged += selfText;
        }

        private void selfText(AudioMetadata obj)
        {
            Task.Run(() =>
            {
                while (!(obj.Track?.IsLoaded ?? false)) ;
                hoverTimeText.Text = formatTime(TimeSpan.FromMilliseconds(obj.Track?.Length ?? 0) * hoverTimeText.Position.X);
                totalTimeText.Text = formatTime(TimeSpan.FromMilliseconds(obj.Track?.Length ?? 0));
            });
        }

        private void trianglePopUp(bool value)
        {
            if (metadata.Value?.Track?.IsLoaded ?? false)
            {
                triangle.MoveToY(value || dragged ? -1 : 13, 200, Easing.OutCubic);
                timeText.ResizeTextHeightTo(value || dragged ? 45 - 14f : 45, 200, Easing.OutCubic);
                if (!value || dragged)
                {
                    timeText.FadeIn(200, Easing.OutCubic);
                    xyz = false;
                }
                totalTimeText.ResizeTextHeightTo(value || dragged ? 45 - 14f : 45, 200, Easing.OutCubic);
                if (!value || dragged)
                {
                    totalTimeText.FadeIn(200, Easing.OutCubic);
                    zyx = false;
                }
                hoverTimeText.FadeTo(value || dragged ? 1 : 0, 200, Easing.OutCubic);
            }
        }

        private string formatTime(TimeSpan t) => $"{Math.Floor(t.Duration().TotalMinutes)}:{t.Duration().Seconds:D2}";

        private double currentTime;

        protected override void Update()
        {
            base.Update();

            var track = metadata.Value?.Track;

            if (track?.IsLoaded ?? false)
            {
                if (!dragged)
                    currentTime = track.CurrentTime;

                float pp = (float)(currentTime / track.Length);
                progress.ResizeWidthTo(pp);
                timeText.Text = formatTime(TimeSpan.FromMilliseconds(track.CurrentTime));

                if (track.HasCompleted && !track.Looping)
                    proyect.PlayNext();
            }
        }

        private bool xyz = false;
        private bool zyx = false;

        protected override bool OnHover(InputState state)
        {
            trianglePopUp(true);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state) => trianglePopUp(false);

        protected override bool OnMouseMove(InputState state)
        {
            if (IsHovered || dragged)
            {
                float f = MathHelper.Clamp(ToLocalSpace(state.Mouse.Position).X / barContainer.DrawSize.X, 0, 1);
                triangle.MoveToX(f);
                hoverTimeText.MoveToX(f);
                if (dragged)
                    currentTime = (metadata.Value?.Track?.Length ?? 0) * f;
                hoverTimeText.Text = formatTime(TimeSpan.FromMilliseconds((metadata.Value?.Track?.Length ?? 0) * f));
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

        private bool dragged = false;

        protected override bool OnDragStart(InputState state) => true;

        protected override bool OnDrag(InputState state)
        {
            if (!IsHovered)
                TriggerOnMouseMove(state);
            return base.OnDrag(state);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            var t = metadata.Value?.Track;
            if ((t?.IsLoaded ?? false) && IsHovered && state.Mouse.IsPressed(MouseButton.Left))
            {
                dragged = true;
                t.Stop();
                TriggerOnMouseMove(state);
            }
            return true;
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            var t = metadata.Value?.Track;

            if ((t?.IsLoaded ?? false) && dragged)
            {
                if (triangle.Position.X != 1)
                {
                    t.Seek(MathHelper.Clamp(t.Length * triangle.Position.X, 0, t.Length));
                    t.Start();
                }
                else
                    proyect.PlayNext();
                dragged = false;
            }
            return base.OnMouseUp(state, args);
        }
    }

    public static class SpriteTextExtensions
    {
        public static TransformSequence<T> ResizeTextHeightTo<T>(this T drawable, float newHeight, double duration = 0, Easing easing = Easing.None) where T : SpriteText =>
           drawable.TransformTo(nameof(drawable.TextSize), newHeight, duration, easing);
    }
}
