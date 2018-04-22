using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;
using System;

namespace TypeProyect.Screens.Pieces
{
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

        private readonly Bindable<Track> track = new Bindable<Track>();

        public Visualisation(Bindable<Track> track)
        {
            this.track.BindTo(track);
            this.track = track;
            texture = Texture.WhitePixel;
            Blending = BlendingMode.Additive;
        }

        protected override void Update()
        {
            base.Update();

            float[] temporalAmplitudes = track.Value?.CurrentAmplitudes.FrequencyAmplitudes ?? new float[256];
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
            public Texture Texture;
            public VisualiserSharedData Shared;
            public Vector2 Size;
            public float[] AudioData;

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
