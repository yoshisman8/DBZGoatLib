using DBZGoatLib.Handlers;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace DBZGoatLib.UI.Components
{
    internal class UnderSparks : AnimatedImage
    {
        public UnderSparks(Asset<Texture2D> texture, int _frames) : base(texture, _frames)
        {
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (TransformationHandler.IsTransformed(Main.CurrentPlayer, true))
                base.DrawSelf(spriteBatch);
        }
    }
}
