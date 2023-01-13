using System;
using System.Linq;

using DBZGoatLib.Handlers;
using DBZGoatLib.Model;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;

namespace DBZGoatLib.Layers {

    public class AuraLayer : PlayerDrawLayer {

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => true;

        public override Position GetDefaultPosition() {
            return new AfterParent(PlayerDrawLayers.Torso);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo) {
            if (Main.netMode == NetmodeID.Server)
                return;

            GPlayer modPlayer = drawInfo.drawPlayer.GetModPlayer<GPlayer>();

            if (TransformationHandler.IsTransformed(drawInfo.drawPlayer)) {
                var forms = TransformationHandler.GetAllCurrentForms(drawInfo.drawPlayer);

                foreach (var data in forms.Where(x => x.stackable).Select(x => x.animationData)) {
                    if (data.Aura.Equals(new AuraData()))
                        continue;
                    DrawAura(modPlayer, data.Aura, TransformationHandler.IsTransformed(drawInfo.drawPlayer, true) ? 1.5f : 1f);
                    Lighting.AddLight(drawInfo.drawPlayer.Center + drawInfo.drawPlayer.velocity * 8f, data.Aura.Color.R / 100, data.Aura.Color.G / 100, data.Aura.Color.B / 100);
                }
                foreach (var data in forms.Where(x => !x.stackable).Select(x => x.animationData))
                {
                    if (data.Aura.Equals(new AuraData()))
                        continue;
                    DrawAura(modPlayer, data.Aura);
                    Lighting.AddLight(drawInfo.drawPlayer.Center + drawInfo.drawPlayer.velocity * 8f, data.Aura.Color.R / 100, data.Aura.Color.G / 100, data.Aura.Color.B / 100);
                }
            }
        }

        public static void DrawAura(GPlayer modPlayer, AuraData aura, float auraScale = 1f) {
            Texture2D texture = aura.GetTexture();

            Rectangle rectangle = new Rectangle(0, aura.GetHeight() * modPlayer.auraCurrentFrame, texture.Width, aura.GetHeight());
            var samplerState = Main.DefaultSamplerState;
            if (modPlayer.Player.mount.Active)
                samplerState = LegacyPlayerRenderer.MountedSamplerState;

            Tuple<float, Vector2> rotationAndPosition = aura.GetAuraRotationAndPosition(modPlayer);
            float num = rotationAndPosition.Item1;
            Vector2 vector2 = rotationAndPosition.Item2;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, aura.BlendState, samplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(texture, vector2 - Main.screenPosition, new Rectangle?(rectangle), aura.Color, num, new Vector2(aura.GetWidth(), aura.GetHeight()) * 0.5f, auraScale, SpriteEffects.None, 0.0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}