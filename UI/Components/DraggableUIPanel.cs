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

namespace DBZGoatLib.UI.Components
{
	public class DragableUIPanel : UIImage
	{
		// Stores the offset from the top left of the UIPanel while dragging.
		private Vector2 offset;
		public bool dragging;

        public DragableUIPanel(Asset<Texture2D> texture) : base(texture)
        {
        }
        public override void OnInitialize()
        {
            base.OnInitialize();
		}
		
        public override void MouseDown(UIMouseEvent evt)
		{
			base.MouseDown(evt);
			DragStart(evt);
		}

		public override void MouseUp(UIMouseEvent evt)
		{
			base.MouseUp(evt);
			DragEnd(evt);
		}

		public void DragStart(UIMouseEvent evt)
		{
			offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
			//offset = new Vector2(0, 0);
			dragging = true;
		}

		public void DragEnd(UIMouseEvent evt)
		{
			Vector2 end = evt.MousePosition;
			dragging = false;

			Left.Set(end.X - offset.X, 0f);
			Top.Set(end.Y - offset.Y, 0f);

			Recalculate();

		}
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

			Vector2 mousePos = new(Main.mouseX, Main.mouseY);

			if (ContainsPoint(mousePos))
			{
				Main.LocalPlayer.mouseInterface = true;
			}
			if (dragging)
			{
				Left.Set(mousePos.X - offset.X, 0f);
				Top.Set(mousePos.Y - offset.Y, 0f);
				Recalculate();
			}
		}
        public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (ContainsPoint(Main.MouseScreen))
			{
				Main.LocalPlayer.mouseInterface = true;
			}
        }
	}
}
