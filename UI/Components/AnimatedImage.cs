using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;

namespace DBZGoatLib.UI
{
    public class AnimatedImage : UIImage
    {
        private int frames;
        private int frameCounter;

        private Asset<Texture2D> textureCache;

        public AnimatedImage(Asset<Texture2D> texture, int _frames) : base(texture)
        {
            frames = _frames;
            textureCache = texture;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            frameCounter++;
            if (frameCounter >= 20)
                frameCounter = 0;
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var dimensions = GetDimensions();
            Rectangle rectangle = new Rectangle(0, (int)(Height.Pixels / frames) * (int)(frameCounter / 5), (int)Width.Pixels, (int)(Height.Pixels / frames));
            Vector2 position = new Vector2(dimensions.X, dimensions.Y);

            spriteBatch.Draw(textureCache.Value, position, rectangle, Color.White);
        }
    }
}
