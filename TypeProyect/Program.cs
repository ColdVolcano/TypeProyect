using osu.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Framework.Audio.Track;
using osu.Framework.Audio;
using System.Drawing.Imaging;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using Triangle = osu.Framework.Graphics.Primitives.Triangle;
using osu.Framework.Lists;

namespace TypeProyect
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (Game game = new SampleGame())
            using (GameHost host = Host.GetSuitableHost(@"sample-game"))
                host.Run(game);
        }
    }

    public class Loader : Screen
    {
        private Container progressContainer;
        private SpriteText currentTime;
        private SpriteText currentWhiteTime;
        private Storage storage;
        private GameHost host;
        private Track track;
        private Texture cover;
        private Container coverContainer;
        private Triangles triangles;

        private readonly List<Shader> loadTargets = new List<Shader>();

        [BackgroundDependencyLoader]
        private void load(Storage storage, GameHost host, ShaderManager manager, FrameworkConfigManager config, AudioManager audio, TextureStore textures)
        {
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED));
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.BLUR));
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE));
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_3, FragmentShaderDescriptor.TEXTURE_ROUNDED));
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_3, FragmentShaderDescriptor.TEXTURE));
            config.GetBindable<WindowMode>(FrameworkSetting.WindowMode).Value = WindowMode.Fullscreen;
            config.GetBindable<FrameSync>(FrameworkSetting.FrameSync).Value = FrameSync.Unlimited;

            Game.Audio.Track.AddItemToList(track = new TrackBass(storage.GetStream("01.めいあいへるぷゆー？.mp3")));
            cover = new TextureStore(new RawTextureLoaderStore(new StorageBackedResourceStore(storage))).Get("cover.jpg");

            this.host = host;
            this.storage = storage;
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
                coverContainer = new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(400),
                    Masking = true,
                    RelativePositionAxes = Axes.Both,
                    Position = new Vector2(-0.4f, 0.33f),
                    Child = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fill,
                        Texture = cover,
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
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = new Color4(25, 25, 25, 255),
                        },
                        new Visualisation(track)
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.TopLeft,
                            RelativeSizeAxes = Axes.X,
                            Height = 400
                        }
                    }
                },
                progressContainer = new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.CentreLeft,
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.X,
                    Position = new Vector2(-0.4f, 0.55f),
                    Size = new Vector2(0.0f, 40),
                    Children = new Drawable[]
                    {
                        currentWhiteTime = new SpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = "0:00",
                            Font = "Exo2.0-Bold",
                            TextSize = 35,
                            Position = new Vector2(2, 0),
                        },
                        new Container
                        {
                            Masking = true,
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.White,
                                    EdgeSmoothness = new Vector2(1),
                                },
                                currentTime = new SpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = "0:00",
                                    Font = "Exo2.0-Bold",
                                    TextSize = 35,
                                    Position = new Vector2(2, 0),
                                    Colour = Color4.Black,
                                }
                            }
                        },
                    }
                },
            });
        }

        private bool screenie = false;
        private int nscreenies = 0;

        protected override void Update()
        {
            base.Update();
            progressContainer.Width = (float)(track.CurrentTime / track.Length) * 0.8f;
            currentWhiteTime.Text = currentTime.Text = $"{(int)(track.CurrentTime / 60000)}:{((int)(track.CurrentTime / 1000) % 60):00}";
            triangles.Velocity = 2.32222f + (float)Math.Pow(track.CurrentAmplitudes.Average * 2 + .5, 2.5);
            if (currentTime.Anchor == Anchor.CentreLeft && progressContainer.Size.X * DrawSize.X > currentTime.Size.X + 4)
            {
                currentTime.Anchor = currentTime.Origin = Anchor.CentreRight; //? progressContainer.Size.X - 0.4f : currentTime.Size.X / 2 / DrawSize.X - 0.4f);
                currentTime.Position = new Vector2(-2, 0);
            }
            else if (progressContainer.Size.X * DrawSize.X < currentTime.Size.X + 4)
            {
                currentTime.Anchor = currentTime.Origin = Anchor.CentreLeft; //? progressContainer.Size.X - 0.4f : currentTime.Size.X / 2 / DrawSize.X - 0.4f);
                currentTime.Position = new Vector2(2, 0);
            }
            /*else if (!screenie && contentLoaded && track.IsRunning)
            {
                TakeScreenshotAsync();
                screenie = true;
            }*/
            if (track.IsLoaded && !track.IsRunning)
            {
                track.Start();
            }
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
            track.Seek(150000);
            return base.OnMouseDown(state, args);
        }
    }

    internal class SampleGame : Game
    {
        protected override string MainResourceFile => "TypeProyect.dll";

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            dependencies.Cache(Fonts = new FontStore { ScaleAdjust = 100 });

            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/FontAwesome"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/osuFont"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Medium"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-MediumItalic"));

            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Noto-Basic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Noto-Hangul"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Noto-CJK-Basic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Noto-CJK-Compatibility"));

            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Regular"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-RegularItalic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-SemiBold"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-SemiBoldItalic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Bold"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-BoldItalic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Light"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-LightItalic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-Black"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Exo2.0-BlackItalic"));

            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Venera"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Venera-Light"));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(new Loader
            {
                RelativeSizeAxes = Axes.Both
            });
        }
    }
    public class Triangles : Drawable
    {
        private const float triangle_size = 100;
        private const float base_velocity = 50;

        /// <summary>
        /// How many screen-space pixels are smoothed over.
        /// Same behavior as Sprite's EdgeSmoothness.
        /// </summary>
        private const float edge_smoothness = 1;

        public override bool HandleKeyboardInput => false;
        public override bool HandleMouseInput => false;


        public Color4 ColourLight = Color4.White;
        public Color4 ColourDark = Color4.Black;

        /// <summary>
        /// Whether we want to expire triangles as they exit our draw area completely.
        /// </summary>
        protected virtual bool ExpireOffScreenTriangles => true;

        /// <summary>
        /// Whether we should create new triangles as others expire.
        /// </summary>
        protected virtual bool CreateNewTriangles => true;

        /// <summary>
        /// The amount of triangles we want compared to the default distribution.
        /// </summary>
        protected virtual float SpawnRatio => 1;

        private float triangleScale = 1;

        /// <summary>
        /// Whether we should drop-off alpha values of triangles more quickly to improve
        /// the visual appearance of fading. This defaults to on as it is generally more
        /// aesthetically pleasing, but should be turned off in buffered containers.
        /// </summary>
        public bool HideAlphaDiscrepancies = true;

        /// <summary>
        /// The relative velocity of the triangles. Default is 1.
        /// </summary>
        public float Velocity = 1;

        private readonly SortedList<TriangleParticle> parts = new SortedList<TriangleParticle>(Comparer<TriangleParticle>.Default);

        private Shader shader;
        private readonly Texture texture;

        public Triangles()
        {
            texture = Texture.WhitePixel;
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            shader = shaders?.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            addTriangles(true);
        }

        public float TriangleScale
        {
            get { return triangleScale; }
            set
            {
                float change = value / triangleScale;
                triangleScale = value;

                for (int i = 0; i < parts.Count; i++)
                {
                    TriangleParticle newParticle = parts[i];
                    newParticle.Scale *= change;
                    parts[i] = newParticle;
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            Invalidate(Invalidation.DrawNode, shallPropagate: false);

            if (CreateNewTriangles)
                addTriangles(false);

            float adjustedAlpha = HideAlphaDiscrepancies ?
                // Cubically scale alpha to make it drop off more sharply.
                (float)Math.Pow(DrawInfo.Colour.AverageColour.Linear.A, 3) :
                1;

            float elapsedSeconds = (float)Time.Elapsed / 1000;
            // Since position is relative, the velocity needs to scale inversely with DrawHeight.
            // Since we will later multiply by the scale of individual triangles we normalize by
            // dividing by triangleScale.
            float movedDistance = -elapsedSeconds * Velocity * base_velocity / (DrawHeight * triangleScale);

            for (int i = 0; i < parts.Count; i++)
            {
                TriangleParticle newParticle = parts[i];

                // Scale moved distance by the size of the triangle. Smaller triangles should move more slowly.
                newParticle.Position.Y += parts[i].Scale * movedDistance;

                parts[i] = newParticle;

                float bottomPos = parts[i].Position.Y + triangle_size * parts[i].Scale * 0.866f / DrawHeight;
                if (bottomPos < 0)
                    parts.RemoveAt(i);
            }
        }

        private void addTriangles(bool randomY)
        {
            int aimTriangleCount = (int)(DrawWidth * DrawHeight * 0.002f / (triangleScale * triangleScale) * SpawnRatio);

            for (int i = 0; i < aimTriangleCount - parts.Count; i++)
                parts.Add(createTriangle(randomY));
        }

        private TriangleParticle createTriangle(bool randomY)
        {
            TriangleParticle particle = CreateTriangle();

            particle.Position = new Vector2(RNG.NextSingle(), randomY ? RNG.NextSingle() : 1);
            particle.InterpolationPoint = CreateTriangleShade();

            return particle;
        }

        /// <summary>
        /// Creates a triangle particle with a random scale.
        /// </summary>
        /// <returns>The triangle particle.</returns>
        protected virtual TriangleParticle CreateTriangle()
        {
            const float std_dev = 0.16f;
            const float mean = 0.5f;

            float u1 = 1 - RNG.NextSingle(); //uniform(0,1] random floats
            float u2 = 1 - RNG.NextSingle();
            float randStdNormal = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)); //random normal(0,1)
            var scale = Math.Max(triangleScale * (mean + std_dev * randStdNormal), 0.1f); //random normal(mean,stdDev^2)

            return new TriangleParticle { Scale = scale };
        }
        protected virtual double CreateTriangleShade() => Interpolation.ValueAt(RNG.NextSingle(), (double)0, 1, 0, 1);

        protected override DrawNode CreateDrawNode() => new TrianglesDrawNode();

        private readonly TrianglesDrawNodeSharedData sharedData = new TrianglesDrawNodeSharedData();
        protected override void ApplyDrawNode(DrawNode node)
        {
            base.ApplyDrawNode(node);

            var trianglesNode = (TrianglesDrawNode)node;

            trianglesNode.Shader = shader;
            trianglesNode.Texture = texture;
            trianglesNode.Size = DrawSize;
            trianglesNode.Shared = sharedData;
            trianglesNode.ColourDark = ColourDark;
            trianglesNode.ColourLight = ColourLight;

            trianglesNode.Parts.Clear();
            trianglesNode.Parts.AddRange(parts);
        }

        private class TrianglesDrawNodeSharedData
        {
            public readonly LinearBatch<TexturedVertex2D> VertexBatch = new LinearBatch<TexturedVertex2D>(100 * 3, 10, PrimitiveType.Triangles);
        }

        private class TrianglesDrawNode : DrawNode
        {
            public Shader Shader;
            public Texture Texture;

            public Color4 ColourLight;
            public Color4 ColourDark;

            public TrianglesDrawNodeSharedData Shared;

            public readonly List<TriangleParticle> Parts = new List<TriangleParticle>();
            public Vector2 Size;

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                base.Draw(vertexAction);

                Shader.Bind();
                Texture.TextureGL.Bind();

                Vector2 localInflationAmount = edge_smoothness * DrawInfo.MatrixInverse.ExtractScale().Xy;

                foreach (TriangleParticle particle in Parts)
                {
                    var offset = triangle_size * new Vector2(particle.Scale * 0.5f, particle.Scale * 0.866f);
                    var size = new Vector2(2 * offset.X, offset.Y);

                    var triangle = new Triangle(
                        Vector2Extensions.Transform(particle.Position * Size, DrawInfo.Matrix),
                        Vector2Extensions.Transform(particle.Position * Size + offset, DrawInfo.Matrix),
                        Vector2Extensions.Transform(particle.Position * Size + new Vector2(-offset.X, offset.Y), DrawInfo.Matrix)
                    );

                    ColourInfo colourInfo = DrawInfo.Colour;
                    colourInfo.ApplyChild(Interpolation.ValueAt(particle.InterpolationPoint, ColourLight, ColourDark, 0, 1));

                    Texture.DrawTriangle(
                        triangle,
                        colourInfo,
                        null,
                        Shared.VertexBatch.Add,
                        Vector2.Divide(localInflationAmount, size));
                }

                Shader.Unbind();
            }
        }

        protected struct TriangleParticle : IComparable<TriangleParticle>
        {
            /// <summary>
            /// The position of the top vertex of the triangle.
            /// </summary>
            public Vector2 Position;

            /// <summary>
            /// The colour of the triangle.
            /// </summary>
            public double InterpolationPoint;

            /// <summary>
            /// The scale of the triangle.
            /// </summary>
            public float Scale;

            /// <summary>
            /// Compares two <see cref="TriangleParticle"/>s. This is a reverse comparer because when the
            /// triangles are added to the particles list, they should be drawn from largest to smallest
            /// such that the smaller triangles appear on top.
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public int CompareTo(TriangleParticle other) => other.Scale.CompareTo(Scale);
        }
    }

    public class Visualisation : Drawable
    {
        /// <summary>
        /// How much should each bar go down each milisecond (based on a full bar).
        /// </summary>
        private const float decay_time = 125f;

        /// <summary>
        /// Number of milliseconds between each amplitude update.
        /// </summary>
        private const float time_between_updates = 50;

        private readonly float[] frequencyAmplitudes = new float[256];
        private readonly float[] higherAmplitudes = new float[256];

        public override bool HandleKeyboardInput => false;
        public override bool HandleMouseInput => false;

        private readonly Texture texture;

        private readonly Track track;

        public Visualisation(Track track)
        {
            this.track = track;
            texture = Texture.WhitePixel;
            Blending = BlendingMode.Additive;
        }

        protected override void Update()
        {
            base.Update();

            float[] temporalAmplitudes = track?.CurrentAmplitudes.FrequencyAmplitudes ?? new float[256];
            for (int i = 0; i < 256; i++)
            {
                frequencyAmplitudes[i] = Math.Max(frequencyAmplitudes[i] - (float)Time.Elapsed * higherAmplitudes[i] / decay_time, 0);
                float targetAmplitude = (float)Math.Log(Math.Pow(temporalAmplitudes[i], 0.9) + 1, 2);
                if (targetAmplitude > frequencyAmplitudes[i])
                    higherAmplitudes[i] = frequencyAmplitudes[i] = targetAmplitude;
            }
            Invalidate(Invalidation.DrawNode, shallPropagate: false);
        }

        protected override DrawNode CreateDrawNode() => new VisualisationDrawNode();

        private readonly VisualiserSharedData sharedData = new VisualiserSharedData();

        protected override void ApplyDrawNode(DrawNode node)
        {
            base.ApplyDrawNode(node);

            var visNode = (VisualisationDrawNode)node;

            visNode.Texture = texture;
            visNode.Size = DrawSize;
            visNode.Shared = sharedData;
            visNode.AudioData = frequencyAmplitudes;
        }

        private class VisualiserSharedData
        {
            public readonly LinearBatch<TexturedVertex2D> VertexBatch = new LinearBatch<TexturedVertex2D>(100 * 4, 10, PrimitiveType.Quads);
        }

        private class VisualisationDrawNode : DrawNode
        {
            public Shader Shader;
            public Texture Texture;
            public VisualiserSharedData Shared;
            public Vector2 Size;
            public float[] AudioData;

            private const float bars_per_line = 85;

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                base.Draw(vertexAction);

                Texture.TextureGL.Bind();

                Vector2 inflation = DrawInfo.MatrixInverse.ExtractScale().Xy;

                ColourInfo colourInfo = DrawInfo.Colour;
                colourInfo.ApplyChild(new Color4(1, 1, 1, 0.25f));

                if (AudioData != null)
                {
                    float limit = 64 * 3 + 63 * 1;

                    for (int i = 0; i < 255; i++)
                    {
                        if (AudioData[i] == 0)
                            continue;

                        var barPosition = new Vector2(((i % 64) * 4 / limit) * Size.X, 0);

                        var barSize = new Vector2(Size.X * 3 / limit, Size.Y * AudioData[i + 1]);

                        var rectangle = new Quad(
                            Vector2Extensions.Transform(barPosition, DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(0, barSize.Y), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, 0), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + barSize, DrawInfo.Matrix)
                        );

                        Texture.DrawQuad(
                            rectangle,
                            colourInfo,
                            null,
                            Shared.VertexBatch.Add,
                            //barSize by itself will make it smooth more in the X axis than in the Y axis, this reverts that.
                            Vector2.Divide(inflation, barSize.Yx));
                    }
                }
            }
        }
    }
}