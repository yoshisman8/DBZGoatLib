using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace DBZGoatLib.UI
{
    public class TransformationPanelComponent : DragableUIPanel
    {
        public TransformationPanelComponent(Asset<Texture2D> texture) : base(texture)
        {
            Vector2 screenPosition = new(DBZConfig.Instance.TransMenuX, DBZConfig.Instance.TransMenuY);

            Left.Set(screenPosition.X, 0f);
            Top.Set(screenPosition.Y, 0f);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (!dragging)
            {
                Vector2 screenRatioPosition = new(Left.Pixels, Top.Pixels);

                if (DBZConfig.Instance.TransMenuX != screenRatioPosition.X || DBZConfig.Instance.TransMenuY != screenRatioPosition.Y)
                {
                    DBZConfig.Instance.TransMenuX = screenRatioPosition.X;
                    DBZConfig.Instance.TransMenuY = screenRatioPosition.Y;
                    DBZGoatLib.SaveConfig(DBZConfig.Instance);
                }
            }
        }
    }
}
