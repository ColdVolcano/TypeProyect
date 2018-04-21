﻿using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Framework.Screens;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using TypeProyect.Screens.Pieces;

namespace TypeProyect.Screens
{
    public class Loader : Screen
    {
        private Container progressContainer;
        private SpriteText currentTime;
        private Storage storage;
        private GameHost host;
        private Bindable<Track> trackBind = new Bindable<Track>();
        private CoverContainer coverContainer;
        private Container timeContainer;
        private Triangles triangles;

        private Bindable<int> trackN = new Bindable<int>();

        private readonly List<Shader> loadTargets = new List<Shader>();

        [BackgroundDependencyLoader]
        private void load(Storage storage, GameHost host, ShaderManager manager, FrameworkConfigManager config)
        {
            this.host = host;
            this.storage = storage;
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED));
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.BLUR));
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE));
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_3, FragmentShaderDescriptor.TEXTURE_ROUNDED));
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_3, FragmentShaderDescriptor.TEXTURE));
            config.GetBindable<WindowMode>(FrameworkSetting.WindowMode).Value = WindowMode.Fullscreen;
            config.GetBindable<FrameSync>(FrameworkSetting.FrameSync).Value = FrameSync.VSync;

            AddRange(new Drawable[]
            {
                triangles = new Triangles()
                {
                    Colour = new Color4(30, 30, 30, 255),
                    ColourLight = new Color4(255, 255, 255, 255),
                    ColourDark = new Color4(42, 42, 42, 255),
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1),
                    TriangleScale = 5,
                    Velocity = 2.5f,
                },
                coverContainer = new CoverContainer(trackBind)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.Both,
                    Position = new Vector2(-0.30234375f, 0.31f),
                    Size = new Vector2(375),
                },
                new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.CentreLeft,
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.X,
                    Position = new Vector2(-0.4f, 0.55f),
                    Size = new Vector2(0.8f, 40),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = new Color4(25, 25, 25, 255),
                        },
                        new Visualisation(trackBind)
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.TopLeft,
                            RelativeSizeAxes = Axes.X,
                            Height = 460
                        }
                    }
                },
                timeContainer = new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.CentreLeft,
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.X,
                    Position = new Vector2(-0.4f, 0.55f),
                    Size = new Vector2(0f, 40),
                    Children = new Drawable[]
                    {
                        currentTime = new SpriteText
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.BottomLeft,
                            Text = "0:00",
                            Font = "Exo2.0-Bold",
                            TextSize = 35,
                            Position = new Vector2(0, -15),
                        },
                        new EquilateralTriangle
                        {
                            RelativePositionAxes = Axes.X,
                            Size = new Vector2(13),
                            Position = new Vector2(1f, -3.5f),
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopLeft,
                            Rotation = 180,
                            EdgeSmoothness = new Vector2(1),
                        }
                    }
                },
                new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.CentreLeft,
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.X,
                    Position = new Vector2(-0.4f, 0.55f),
                    Size = new Vector2(0.8f, 40),
                    Masking = true,
                    Children = new Drawable[]
                    {
                        progressContainer = new Container
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
                },
            });

            coverContainer.TrackIndex.BindTo(trackN);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            loadTrack();
        }

        private bool screenie = false;
        private int nscreenies = 0;

        protected override void Update()
        {
            Track track = trackBind.Value;
            base.Update();
            /*else if (!screenie && contentLoaded && track.IsRunning)
            {
                TakeScreenshotAsync();
                screenie = true;
            }*/
            if (track?.IsLoaded ?? false)
            {
                if (track.HasCompleted)
                    loadTrack();
                else
                {
                    timeContainer.Width = (progressContainer.Width = (float)(track.CurrentTime / track.Length)) * 0.8f;
                    currentTime.Text = $"{(int)(track.CurrentTime / 60000)}:{((int)(track.CurrentTime / 1000) % 60):00}";
                    triangles.Velocity = 2.32222f + (float)Math.Pow(track.CurrentAmplitudes.Average * 2 + .5, 2.5);
                    float progress = progressContainer.Size.X;
                    float drawX = DrawSize.X;
                    float current = currentTime.Size.X;
                    currentTime.MoveToX(
                        progress * 0.8f * drawX > current / 2 ?
                        (drawX * (0.8f - 0.8f * progress) > current / 2 ?
                            progress * 0.8f * drawX - current / 2 :
                            drawX * 0.8f - current)
                        : 0);
                }
            }
        }

        private void loadTrack()
        {
            int tempIndex = trackN.Value + 1;
            string filename = $"{tempIndex:00}.mp3";
            Track tempTrack;
            if (storage.Exists(filename))
            {
                tempTrack = new TrackBass(storage.GetStream(filename));
            }
            else
            {
                tempIndex = 1;
                tempTrack = new TrackBass(storage.GetStream("01.mp3"));
            }
            progressContainer.ResizeWidthTo(0, 500, Easing.OutExpo);
            Game.Audio.Track.AddItemToList(trackBind.Value = tempTrack);
            trackN.Value = tempIndex;
            tempTrack.Start();
        }

        public async void TakeScreenshotAsync()
        {
            using (var bitmap = await host.TakeScreenshotAsync())
            {
                var stream = storage.GetStream(getAndSecureFileName(), FileAccess.Write);
                bitmap.Save(stream, ImageFormat.Png);
            }

            screenie = false;
        }

        private string getAndSecureFileName()
        {
            string filename = $"{nscreenies}.png";
            if (storage.Exists(filename))
                storage.Delete(filename);
            return filename;
        }
        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            trackBind.Value.Seek(trackBind.Value.Length - 3000);
            return base.OnMouseDown(state, args);
        }
    }
}
