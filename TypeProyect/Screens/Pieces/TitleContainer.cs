using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Allocation;

namespace TypeProyect.Screens.Pieces
{
    public class TitleContainer : Container
    {
        private Bindable<AudioMetadata> meta = new Bindable<AudioMetadata>();

        private TextFlowContainer mainText;
        private Container mainContainer;

        private Anchor textAnchor;

        public MetadataTypes PreferredText = MetadataTypes.TitleUnicode;

        public float TextSize
        {
            get => textSize;
            set
            {
                Height = value * 2;
                textSize = value;
            }
        }

        private float textSize = 75;

        public string Font = "Exo2.0";

        /// <summary>
        /// The ammount of delay used when the value of <see cref="Text"/> changes.
        /// </summary>
        public double TransitionDelay = 0;

        public TitleContainer(Anchor textAnchor)
        {
            this.textAnchor = textAnchor;
            Masking = true;
            Children = new Drawable[]
            {
                mainContainer = new Container
                {
                    Masking = true,
                    RelativePositionAxes = Axes.Y,
                    RelativeSizeAxes = Axes.Both,
                    Child = mainText = new TextFlowContainer
                    {
                        Anchor = textAnchor,
                        Origin = textAnchor,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(TypeProyect type)
        {
            meta.BindTo(type.Metadata);
            meta.ValueChanged += transition;
        }

        private void transition(AudioMetadata newMeta)
        {
            mainContainer.Delay(TransitionDelay)
                .MoveToY((int)(textAnchor & Anchor.y0) == 1 ? -1 : 1, 212.5, Easing.InQuart)
                .OnComplete(_ =>
                {
                    mainText.Clear();
                    mainText.AddText((string)(newMeta.GetType().GetField(PreferredText.ToString()).GetValue(newMeta)) ?? string.Empty, s =>
                    {
                        s.TextSize = textSize;
                        s.Font = Font;
                    });
                    mainContainer.MoveToY(0, 212.5, Easing.OutQuad);
                });
        }
    }

    public enum MetadataTypes
    {
        TitleUnicode,
        Title,
        Artist,
        ArtistUnicode
    }
}
