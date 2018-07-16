using Id3;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Platform;
using osu.Framework.Screens;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private ProgressContainer progress;
        private PlaylistContainer playlist;

        private readonly List<Shader> loadTargets = new List<Shader>();

        [BackgroundDependencyLoader]
        private void load(Storage storage, GameHost host, ShaderManager manager, FrameworkConfigManager config, TypeProyect game)
        {
            this.storage = storage;
            this.host = host;
            this.game = game;
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED));
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.BLUR));
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE));
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_3, FragmentShaderDescriptor.TEXTURE_ROUNDED));
            loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_3, FragmentShaderDescriptor.TEXTURE));
            config.GetBindable<WindowMode>(FrameworkSetting.WindowMode).Value = WindowMode.Fullscreen;
            config.GetBindable<FrameSync>(FrameworkSetting.FrameSync).Value = FrameSync.Unlimited;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            game.LoadComponentSingleFile(progress = new ProgressContainer
            {
                Depth = -1,
                Position = new Vector2(192, 614),
                Width = 1536,
            }, Add);

            game.LoadComponentSingleFile(triangles = new Triangles { Velocity = 2.5f }, Add);
            game.LoadComponentSingleFile(coverContainer = new CoverContainer()
            {
                Position = new Vector2(192, 140),
                Size = new Vector2(350),
            }, Add);
            game.LoadComponentSingleFile(new TitleContainer(Anchor.BottomLeft)
            {
                Position = new Vector2(550, 140),
                TextSize = 100,
                Font = "Exo2.0-Bold",
                PreferredText = MetadataTypes.TitleUnicode,
                Width = 1176,
            }, Add);
            game.LoadComponentSingleFile(new TitleContainer(Anchor.TopLeft)
            {
                Position = new Vector2(550, 345),
                TextSize = 70,
                Font = "Exo2.0-Medium",
                PreferredText = MetadataTypes.ArtistUnicode,
                Width = 1176,
                Colour = new Color4(200, 200, 200, 255),
                TransitionDelay = 75,
            }, Add);
            game.LoadComponentSingleFile(new Visualisation
            {
                Position = new Vector2(192, 614),
                Size = new Vector2(1536, 460),
            }, Add);
            game.LoadComponentSingleFile(playlist = new PlaylistContainer(), Add);
        }

        public void ImportSongs(object sender, FileDropEventArgs e)
        {
            ImportPaths(e.FileName);
        }

        public void ImportPaths(params string[] paths)
        {
            if (paths.Any(p => checkExtension(Path.GetExtension(p))))
                return;
            foreach (string p in paths)
            {
                AudioMetadata meta = new AudioMetadata();
                using (var mp3 = new Mp3(p))
                {
                    Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
                    if (tag.CustomTexts.Count > 0 && tag.CustomTexts.Any(t => t.Value.StartsWith("DISPLAY ARTIST\0")))
                        meta.Artist = meta.ArtistUnicode = tag.CustomTexts.First(t => t.Value.StartsWith("DISPLAY ARTIST\0")).Value.Split("DISPLAY ARTIST\0")[1];
                    else if (tag.Artists.Value.Count > 0)
                        meta.Artist = meta.ArtistUnicode = tag.Artists.Value[0];
                    else
                        meta.Artist = meta.ArtistUnicode = "Unkown Artist";
                    meta.Title = meta.TitleUnicode = tag.Title.Value ?? "Unkown Title";
                }
                playlist.AddSong(meta, p);
            }
        }

        public void PlayNext() => playlist.PlayNext();

        private bool checkExtension(string ext) => ext != ".mp3";
    }
}
