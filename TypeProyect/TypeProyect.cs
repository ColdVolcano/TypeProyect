using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using System;
using System.Threading.Tasks;
using TypeProyect.Screens;
using osu.Framework.Configuration;
using osu.Framework.Logging;
using System.IO;

namespace TypeProyect
{
    internal class TypeProyect : Game
    {
        protected override string MainResourceFile => "TypeProyect.dll";

        private DependencyContainer dependencies;

        public Bindable<AudioMetadata> Metadata;

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

            Metadata = new Bindable<AudioMetadata>();

            Metadata.ValueChanged += newMeta =>
            {
                newMeta.InitializeComponents(Host.Storage);
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var m = new AudioMetadata();
            File.WriteAllText("C:\\Users\\LavainstranterCV\\AppData\\Roaming\\sample-game\\json.txt", Newtonsoft.Json.JsonConvert.SerializeObject(m, Newtonsoft.Json.Formatting.Indented));
            dependencies.Cache(this);

            LoadComponentSingleFile(new Loader
            {
                RelativeSizeAxes = Axes.Both
            }, Add);
        }

        private Task asyncLoadStream;

        public void LoadComponentSingleFile<T>(T d, Action<T> add)
            where T : Drawable
        {
            Schedule(() => { asyncLoadStream = asyncLoadStream?.ContinueWith(t => LoadComponentAsync(d, add).Wait()) ?? LoadComponentAsync(d, add); });
        }
    }
}
