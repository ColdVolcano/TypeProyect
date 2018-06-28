using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace TypeProyect.Screens.Pieces
{
    public class TitleContainer : Container
    {
        protected UnicodeBindableString Text = new UnicodeBindableString("ダタがない", "No data");

        private SpriteText mainText;
        private SpriteText subText;

        public float TextSize
        { 
            get => textSize;
            set
            {
                mainText.TextSize = value;
                subText.TextSize = value;
                textSize = value;
            }
        }

        private float textSize;

        /// <summary>
        /// The ammount of delay used when the value of <see cref="Text"/> changes.
        /// </summary>
        public double TransitionDelay = 0;

        public TitleContainer(float textSize)
        {
        }
    }
}
