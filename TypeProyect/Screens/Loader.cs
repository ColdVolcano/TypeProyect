using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Platform;
using osu.Framework.Screens;
using System.Collections.Generic;
using TypeProyect.Screens.Pieces;

namespace TypeProyect.Screens
{
    public class Loader : Screen
    {
        private Storage storage;
        private TypeProyect game;
        private GameHost host;
        private CoverContainer coverContainer;
        private Triangles triangles;

        private readonly List<Shader> loadTargets = new List<Shader>();

        [BackgroundDependencyLoader]
        private void load(Storage storage, GameHost host, ShaderManager manager, FrameworkConfigManager config, TypeProyect game)
        {
            this.host = host;
            this.storage = storage;
            this.game = game;
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED));
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.BLUR));
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE));
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_3, FragmentShaderDescriptor.TEXTURE_ROUNDED));
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_3, FragmentShaderDescriptor.TEXTURE));
            config.GetBindable<WindowMode>(FrameworkSetting.WindowMode).Value = WindowMode.Fullscreen;
            config.GetBindable<FrameSync>(FrameworkSetting.FrameSync).Value = FrameSync.VSync;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            game.LoadComponentSingleFile(new ProgressContainer { Depth = -1 }, Add);
            game.LoadComponentSingleFile(triangles = new Triangles() { Velocity = 2.5f }, Add);
            game.LoadComponentSingleFile(coverContainer = new CoverContainer()
            {
                Position = new Vector2(192, 147f),
                Size = new Vector2(375),
            }, Add);
            game.LoadComponentSingleFile(
            new Visualisation()
            {
                Position = new Vector2(192, 614),
                Size = new Vector2(1536, 460),
            }, Add);
        }
    }
}
