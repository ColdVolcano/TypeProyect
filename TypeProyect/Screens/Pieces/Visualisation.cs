﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;
using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shaders;

namespace TypeProyect.Screens.Pieces
{
    public class Visualisation : Drawable
    {
        /// <summary>
        /// How much should each bar go down each milisecond (based on a full bar).
        /// </summary>
        private const float decay_time = 150f;

        private readonly float[] frequencyAmplitudes = new float[1024];
        private readonly float[] higherAmplitudes = new float[1024];

        public override bool HandleKeyboardInput => false;
        public override bool HandleMouseInput => false;

        private readonly Texture texture;

        private readonly Bindable<AudioMetadata> meta = new Bindable<AudioMetadata>();

        public Visualisation()
        {
            texture = Texture.WhitePixel;
            Blending = BlendingMode.Additive;
        }

        protected override void Update()
        {
            base.Update();
            float[] temporalAmplitudes = meta.Value?.Track?.CurrentAmplitudes.FrequencyAmplitudes ?? new float[1024];
            for (int i = 0; i < 1024; i++)
            {
                frequencyAmplitudes[i] = Math.Max(frequencyAmplitudes[i] - (float)Time.Elapsed * higherAmplitudes[i] / decay_time, 0);
                float targetAmplitude = (float)Math.Log(Math.Pow(temporalAmplitudes[i], 0.9) + 1, 2);
                if (targetAmplitude > frequencyAmplitudes[i])
                    higherAmplitudes[i] = frequencyAmplitudes[i] = targetAmplitude;
            }
            Invalidate(Invalidation.DrawNode, shallPropagate: false);
        }

        private Shader shader;

        [BackgroundDependencyLoader]
        private void load(TypeProyect proyect, ShaderManager shaders)
        {
            shader = shaders?.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
            meta.BindTo(proyect.Metadata);
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
            visNode.Shader = shader;
            visNode.AudioData = frequencyAmplitudes;
        }

        private class VisualiserSharedData
        {
            public readonly LinearBatch<TexturedVertex2D> VertexBatch = new LinearBatch<TexturedVertex2D>(255 * 4, 10, PrimitiveType.Quads);
        }

        private class VisualisationDrawNode : DrawNode
        {
            public Texture Texture;
            public VisualiserSharedData Shared;
            public Vector2 Size;
            public Shader Shader;
            public float[] AudioData;

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                base.Draw(vertexAction);

                Texture.TextureGL.Bind();

                Shader.Bind();

                Vector2 inflation = DrawInfo.MatrixInverse.ExtractScale().Xy;

                ColourInfo colourInfo = DrawInfo.Colour;
                colourInfo.ApplyChild(new Color4(1, 1, 1, 0.15f));

                if (AudioData != null)
                {
                    int maxColumns = 128;
                    float inverseRatio = 12;
                    float limit = maxColumns * (inverseRatio - 1) + (maxColumns - 1);

                    for (int i = 0; i < 1023; i++)
                    {
                        var barPosition = new Vector2(((i % maxColumns) * inverseRatio / limit) * Size.X, 0);

                        var barSize = new Vector2(Size.X * (inverseRatio - 1) / limit, Size.Y * AudioData[i + 1]);

                        var rectangle = new Quad(
                            Vector2Extensions.Transform(barPosition, DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, 0), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(0, barSize.Y), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + barSize, DrawInfo.Matrix)
                        );

                        Texture.DrawQuad(
                            rectangle,
                            colourInfo,
                            null,
                            Shared.VertexBatch.Add);
                    }
                }

                Shader.Unbind();
            }
        }
    }
}
