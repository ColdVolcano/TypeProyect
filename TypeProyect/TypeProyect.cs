﻿using OpenTK.Input;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using System;
using System.Threading.Tasks;
using TypeProyect.Screens;

namespace TypeProyect
{
    internal class TypeProyect : Game
    {
        private DependencyContainer dependencies;

        private Loader loader;

        public Bindable<AudioMetadata> Metadata;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            Resources.AddStore(new DllResourceStore(@"TypeProyect.dll"));
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

            Metadata = new Bindable<AudioMetadata>();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            //var m = new AudioMetadata();
            //File.WriteAllText("C:\\Users\\LavainstranterCV\\AppData\\Roaming\\sample-game\\json.txt", Newtonsoft.Json.JsonConvert.SerializeObject(m, Newtonsoft.Json.Formatting.Indented));
            dependencies.Cache(this);

            LoadComponentSingleFile(loader = new Loader
            {
                RelativeSizeAxes = Axes.Both
            }, Add);
        }

        public void Cache<T>(T instance) where T : class
        {
            dependencies.Cache(instance);
        }

        private Task asyncLoadStream;

        public void LoadComponentSingleFile<T>(T d, Action<T> add)
            where T : Drawable
        {
            Schedule(() => { asyncLoadStream = asyncLoadStream?.ContinueWith(t => LoadComponentAsync(d, add).Wait()) ?? LoadComponentAsync(d, add); });
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);

            var a = host.Window as DesktopGameWindow;

            a.Title = "TypeProyect";

            a.FileDrop += importSongs;
        }

        private void importSongs(object sender, FileDropEventArgs args) => loader.ImportSongs(sender, args);

        public void PlayNext() => loader.PlayNext();
    }
}
