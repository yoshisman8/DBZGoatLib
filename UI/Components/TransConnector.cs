using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;
using DBZGoatLib.Model;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using DBZGoatLib.Handlers;
using Terraria.GameContent;

namespace DBZGoatLib.UI.Components
{
    public class TransConnector : UIElement
    {
        public Connection Connection;

        public TransConnector(Connection connection)
        {
            Connection = connection;

            Left.Set(16 + (40 * Connection.StartPosX), 0);
            Top.Set(16 + (40 * Connection.StartPosY), 0);

            if(Connection.Veritcal)
            {
                Width.Set(4, 0);
                Height.Set(42 * Connection.Length, 0);
            }
            else
            {
                Width.Set(42 * Connection.Length, 0);
                Height.Set(4, 0);
            }
            IgnoresMouseInteraction = true;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            Rectangle hitbox = GetDimensions().ToRectangle();

            int start = Connection.Veritcal? hitbox.Top : hitbox.Left;
            int end = Connection.Veritcal ? hitbox.Bottom : hitbox.Right;
            int steps = end - start;

            for(int i = 0; i < steps; i++)
            {
                float percent = (float)i / (end - start);
                if (Connection.Veritcal)
                    spriteBatch.Draw((Texture2D)TextureAssets.MagicPixel, new Rectangle(hitbox.X, start + i, hitbox.Width, 1), Connection.Color.GetColor(percent));
                else
                    spriteBatch.Draw((Texture2D)TextureAssets.MagicPixel, new Rectangle(start + i, hitbox.Y, 1, hitbox.Height), Connection.Color.GetColor(percent));
            }

        }
    }
}
