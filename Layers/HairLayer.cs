using DBZGoatLib.Handlers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DBZGoatLib.Layers
{
    public class HairLayer : PlayerDrawLayer
    {
        public override bool IsHeadLayer => true;
        public override Position GetDefaultPosition()
        {
            return new AfterParent(PlayerDrawLayers.Head);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            GPlayer modPlayer = drawInfo.drawPlayer.GetModPlayer<GPlayer>();

            if (TransformationHandler.IsTransformed(drawInfo.drawPlayer))
            {

                var data = TransformationHandler.GetCurrentTransformation(drawInfo.drawPlayer).Value.animationData;

                if (string.IsNullOrEmpty(data.HairPath))
                    return;

                Player drawPlayer = drawInfo.drawPlayer;

                var x = drawInfo.Position.X - Main.screenPosition.X - (drawPlayer.bodyFrame.Width / 2) + drawPlayer.width/2;
                var y = drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f;
                Vector2 vector = new Vector2((int)x ,(int)y) + drawPlayer.headPosition + drawInfo.hairOffset;

                vector -= CalcRotation(drawPlayer.headRotation);

                DrawData drawData = new DrawData(
                    ModContent.Request<Texture2D>(data.HairPath, AssetRequestMode.AsyncLoad).Value,
                    vector,
                    new Rectangle?(drawPlayer.bodyFrame),
                    Color.White,
                    drawPlayer.headRotation,
                    drawInfo.hairOffset,
                    1f,
                    drawInfo.playerEffect,0);


                drawInfo.hideHair = true;
                drawInfo.DrawDataCache.Add(drawData);
            }
        }
        private Vector2 CalcRotation(float rot)
        {
            Vector2 vector = new Vector2(-13f - rot * 4.25f, -13f + rot * 4.25f);
            float num = (float)Math.Sqrt(Math.Pow((double)vector.X, 2.0) + Math.Pow((double)vector.Y, 2.0));
            float num2 = (float)Math.Sqrt(Math.Pow((double)vector.Y, 2.0) + Math.Pow((double)(vector.X + num), 2.0));
            float num3 = (float)Math.Acos((Math.Pow((double)num, 2.0) + Math.Pow((double)num, 2.0) - Math.Pow((double)num2, 2.0)) / (2.0 * Math.Pow((double)num, 2.0)));
            float num4 = vector.X + num * (float)Math.Cos((double)(MathHelper.ToRadians(rot * 90f) + num3));
            float num5 = vector.Y + num * (float)Math.Sin((double)(MathHelper.ToRadians(rot * 90f) + num3));
            return new Vector2(num4, num5);
        }

    }
}
