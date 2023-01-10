using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace DBZGoatLib.UI
{
    internal class KiBarContainer : DragableUIPanel
    {
		public KiBarContainer(Asset<Texture2D> texture) : base(texture)
        {
            Vector2 screenPosition = new(DBZConfig.Instance.KiBarX, DBZConfig.Instance.KiBarY);

            Left.Set(screenPosition.X, 0f);
            Top.Set(screenPosition.Y, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (!dragging)
            {
                Vector2 screenRatioPosition = new(Left.Pixels, Top.Pixels);

                if (DBZConfig.Instance.KiBarX != screenRatioPosition.X || DBZConfig.Instance.KiBarY != screenRatioPosition.Y)
                {
                    DBZConfig.Instance.KiBarX = screenRatioPosition.X;
                    DBZConfig.Instance.KiBarY = screenRatioPosition.Y;
                    DBZGoatLib.SaveConfig(DBZConfig.Instance);
                }
            }

            if (DBZConfig.Instance.UseNewKiBar)
                base.DrawSelf(spriteBatch);
        }
    }
}
