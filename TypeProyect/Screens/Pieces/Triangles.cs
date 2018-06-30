using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.Lists;
using osu.Framework.MathUtils;
using System;
using System.Collections.Generic;

namespace TypeProyect.Screens.Pieces
{
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


        public Color4 ColourLight = new Color4(30, 30, 30, 175);
        public Color4 ColourDark = new Color4(5, 5, 5, 175);

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
        protected virtual float SpawnRatio => 1.2f;

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

        private Bindable<AudioMetadata> meta = new Bindable<AudioMetadata>();

        public Triangles()
        {
            texture = Texture.WhitePixel;
            triangleScale = 5;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders, TypeProyect proyect)
        {
            meta.BindTo(proyect.Metadata);
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

            Velocity = 2.32222f + (float)Math.Pow((meta.Value?.Track?.CurrentAmplitudes.Average ?? 0) * 2 + .5, 2.5);

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
}
