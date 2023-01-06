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
using Terraria.ID;

namespace DBZGoatLib.UI
{
    public class TransNode : UIImageButton
    {
        public Node Node;
        private Asset<Texture2D> TextureCache;
        private Asset<Texture2D> Hidden;

        private UIImage Selector;
        private UIImage Lock;
        public TransNode(Asset<Texture2D> texture, Node node) : base(texture)
        {
            Node = node;
            TextureCache = texture;
            Hidden = ModContent.Request<Texture2D>("DBZGoatLib/Assets/UI/Undiscovered");

            Width.Set(32, 0f);
            Height.Set(32, 0f);

            MaxHeight.Set(32, 0f);
            MaxWidth.Set(32, 0f);

            Left.Set(40 * node.PosX, 0f);
            Top.Set(40 * node.PosY, 0f);

            Selector = new UIImage(ModContent.Request<Texture2D>("DBZGoatLib/Assets/UI/Selected"));

            Selector.Width.Set(32, 0f);
            Selector.Height.Set(32, 0f);
            Selector.Left.Set(-2, 0f);
            Selector.Top.Set(-2, 0f);
            Selector.IgnoresMouseInteraction = true;

            Lock = new UIImage(ModContent.Request<Texture2D>("DBZGoatLib/Assets/UI/Locked"));

            Lock.Width.Set(32, 0f);
            Lock.Height.Set(32, 0f);
            Lock.IgnoresMouseInteraction = true;
        }
        public override void Click(UIMouseEvent evt)
        {
            

            if (!Node.UnlockCondition(Main.CurrentPlayer) || !Node.DiscoverCondition(Main.CurrentPlayer))
            {
                SoundHandler.PlayVanillaSound(SoundID.MenuTick, Main.CurrentPlayer.position);
                Main.NewText(Node.UnlockHint);
                return;
            }

            SoundHandler.PlayVanillaSound(SoundID.MenuTick, Main.CurrentPlayer.position);

            if (!Node.ViewOnly)
            {
                Node.OnSelect?.Invoke(Main.CurrentPlayer);
                return;
            }
            TransformationMenu.ActiveForm = Node.BuffKeyName;

            Node.OnSelect?.Invoke(Main.CurrentPlayer);
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            TransformationMenu.Tooltip = null;
        }
        public override void MouseOver(UIMouseEvent evt)
        {
            if (!Node.DiscoverCondition(Main.CurrentPlayer))
                TransformationMenu.Tooltip = "???";
            else
                TransformationMenu.Tooltip = TransformationHandler.GetTransformation(Node.BuffKeyName).transformationText;
            TransformationMenu.HoveredForm = Node.BuffKeyName;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (!Node.DiscoverCondition(Main.CurrentPlayer))
                spriteBatch.Draw(Hidden.Value, GetDimensions().ToRectangle(), Color.White);
            else
                spriteBatch.Draw(TextureCache.Value, GetDimensions().ToRectangle(), Color.White);
        }
        protected override void DrawChildren(SpriteBatch spriteBatch)
        {
            if (TransformationMenu.ActiveForm == Node.BuffKeyName)
            {
                Append(Selector);
            }
            else
            {
                if (Children.Contains(Selector))
                    RemoveChild(Selector);
            }

            if (!Node.UnlockCondition(Main.CurrentPlayer))
            {
                Append(Lock);
            }
            else
            {
                if (Children.Contains(Lock))
                    RemoveChild(Lock);
            }
            
            base.DrawChildren(spriteBatch);
        }
    }
}
