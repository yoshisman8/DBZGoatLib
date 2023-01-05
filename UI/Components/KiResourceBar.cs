using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace DBZGoatLib.UI
{
    public class KiResourceBar : UIElement
    {
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            float Quotient = KiBar.AverageKi / KiBar.MaxKi;

            float clamp = Utils.Clamp(Quotient, 0f, 1f);

            var hitbox = GetDimensions().ToRectangle();

            int left = hitbox.Left;
            int right = hitbox.Right;
            int steps = (int)((right - left) * clamp);
            for (int i = 0; i < steps; i++)
            {
                float percent = (float)i / (right - left);
                spriteBatch.Draw((Texture2D)TextureAssets.MagicPixel, new Rectangle(left + i, hitbox.Y, 1, hitbox.Height), KiBar.GetColor().GetColor(percent));
            }
        }
    }
}
