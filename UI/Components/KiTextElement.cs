using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;

namespace DBZGoatLib.UI.Components
{
    public class KiTextElement : UIText
    {
        public KiTextElement(string text, float textScale = 1, bool large = false) : base(text, textScale, large)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            SetText($"{(int)KiBar.AverageKi}/{KiBar.MaxKi}");
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if(DBZConfig.Instance.ShowKi)
                base.DrawSelf(spriteBatch);
        }
    }
}
