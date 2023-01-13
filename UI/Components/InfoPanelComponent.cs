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

namespace DBZGoatLib.UI.Components
{
    public class InfoPanelComponent : UIImage
    {

        private UIText KiDamage;
        private UIText KiCrit;
        private UIText KiUsage;
        private UIText KiRegen;
        private UIText KiChargeRate;
        private UIText KiCastSpeed;
        private UIText KiBeamCharges;

        private int rowHeight = 40;
        private int leftMargin = 10;
        private int GetHeight(int row) => 9 + (rowHeight * (row-1));
        public InfoPanelComponent(Asset<Texture2D> texture) : base(texture)
        {
            KiDamage = new("Ki Damage: +0%");
            KiDamage.Height.Set(rowHeight, 0f);
            KiDamage.Left.Set(leftMargin, 0f);
            KiDamage.Top.Set(GetHeight(1),0f);
            Append(KiDamage);

            KiCrit = new("Ki Crit Chance: +0%");
            KiCrit.Height.Set(rowHeight, 0f);
            KiCrit.Left.Set(leftMargin, 0f);
            KiCrit.Top.Set(GetHeight(2), 0f);
            Append(KiCrit);

            KiUsage = new("Ki Usage: +0%");
            KiUsage.Height.Set(rowHeight, 0f);
            KiUsage.Left.Set(leftMargin, 0f);
            KiUsage.Top.Set(GetHeight(3), 0f);
            Append(KiUsage);

            KiCastSpeed = new("Ki Cast Speed: +0%");
            KiCastSpeed.Height.Set(rowHeight, 0f);
            KiCastSpeed.Left.Set(leftMargin, 0f);
            KiCastSpeed.Top.Set(GetHeight(4), 0f);
            Append(KiCastSpeed);

            KiBeamCharges = new("Additional Beam Charges: 0");
            KiBeamCharges.Height.Set(rowHeight, 0f);
            KiBeamCharges.Left.Set(leftMargin, 0f);
            KiBeamCharges.Top.Set(GetHeight(5), 0f);
            Append(KiBeamCharges);

            KiRegen = new("Ki Regen: 0 ki/sec");
            KiRegen.Height.Set(rowHeight, 0f);
            KiRegen.Left.Set(leftMargin, 0f);
            KiRegen.Top.Set(GetHeight(6), 0f);
            Append(KiRegen);

            KiChargeRate = new("Ki Charge Rate: 0 ki/sec");
            KiChargeRate.Height.Set(rowHeight, 0f);
            KiChargeRate.Left.Set(leftMargin, 0f);
            KiChargeRate.Top.Set(GetHeight(7), 0f);
            Append(KiChargeRate);

        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var MyPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));
            dynamic instance = MyPlayer.GetMethod("ModPlayer").Invoke(null, new object[] { Main.CurrentPlayer });

            float KiDamage = (float)instance.KiDamage;
            float KiCrit = (int)instance.kiCrit;
            float kiRegen = (int)instance.kiRegen;
            float usageMulti = (float)instance.kiDrainMulti;
            float chargeRate = (int)instance.kiChargeRate;
            float chargeLimit = (int)instance.chargeLimitAdd;
            float castSpeed = (float)instance.kiSpeedAddition;

            this.KiDamage.SetText($"Ki Damage: {(KiDamage - 1 >= 0 ? "+" : "")}{KiDamage-1:P2}");
            this.KiCrit.SetText($"Ki Crit Chance: {KiCrit / 100f:P2}");
            this.KiUsage.SetText($"Ki Usage: {(usageMulti - 1 >= 0 ? "+" : "")}{usageMulti - 1:P2}");
            this.KiCastSpeed.SetText($"Ki Cast Speed: {(castSpeed - 1 >= 0 ? "+" : "")}{castSpeed - 1:P2}");
            this.KiBeamCharges.SetText($"Additional Beam Charges: {chargeLimit:N0}.");
            this.KiRegen.SetText($"Ki Regen: {Math.Floor(kiRegen * 20)} ki/sec.");
            this.KiChargeRate.SetText($"Ki Charge Rate: {Math.Floor(chargeRate * 60)} ki/sec.");
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (TransformationMenu.InfoVisible)
                base.DrawSelf(spriteBatch);
        }

        protected override void DrawChildren(SpriteBatch spriteBatch)
        {
            if (TransformationMenu.InfoVisible)
                base.DrawChildren(spriteBatch);
        }
    }
}
