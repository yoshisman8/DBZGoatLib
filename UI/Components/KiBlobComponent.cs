using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBZGoatLib.UI.Components
{
    internal class KiBlobComponent : AnimatedImage
    {
        public KiBlobComponent(Asset<Texture2D> texture, int _frames) : base(texture, _frames)
        {
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Color = KiBar.GetBlobColor();
        }
    }
}
