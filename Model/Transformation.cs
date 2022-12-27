using DBZGoatLib.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace DBZGoatLib.Model
{
    public abstract class Transformation : ModBuff
    {
        public float damageMulti;
        public float speedMulti;
        public float kiDrainRate;
        public float kiDrainRateWithMastery;
        public float attackDrainMulti;
        public int baseDefenceBonus;

        private int mastery = 0;
        private int tipMastery = 0;
        private string tipCache = "";

        /// <summary>
        /// This form can be stacked with other forms. Only do this if you you know how to balance it!
        /// </summary>
        public bool Stackable = false;
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;

            Description.SetDefault(BuildTooltip());
        }
        public override void ModifyBuffTip(ref string tip, ref int rare)
        {
            tip = BuildTooltip();
        }

        public string BuildTooltip()
        {
            if (tipMastery != mastery || tipMastery == 0)
            {
                int percent1 = (int)Math.Round(damageMulti * 100f, 0);
                int percent2 = (int)Math.Round((speedMulti - 1f) * 100f, 0);

                float num1 = 60f * kiDrainRate;
                float num2 = 60f * kiDrainRateWithMastery;

                int percent3 = (int)Math.Round(attackDrainMulti * 100.0, 0) - 100;

                StringBuilder sb = new StringBuilder();

                if (percent1 != 0)
                    sb.Append($"Damage {(percent1 > 0 ? '+' : '-')}{percent1}% ");
                if (percent2 != 0)
                    sb.AppendLine($"Speed {(percent2 > 0 ? '+' : '-')}{percent2}%");

                if (percent3 != 0)
                    sb.AppendLine($"Ki Costs {(percent3 > 0 ? '+' : '-')}{percent3}");

                sb.AppendLine($"Ki Drain {(int)num1}/s, {(int)num2}/s when mastered");

                sb.Append($"Mastery: {mastery}%");

                tipCache = sb.ToString();
                tipMastery = mastery;

                return sb.ToString();
            }
            else
            {
                return tipCache;
            }
        }
        public override void Update(Player player, ref int buffIndex)
        {
            mastery = (int)(GPlayer.ModPlayer(player).GetMastery(Type) * 100f);

            var MyPlayer = DBZGoatLib.DBZMOD.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));

            dynamic modPlayer = MyPlayer.GetMethod("ModPlayer").Invoke(null, new object[] { player });
            var KiDamage = DBZGoatLib.DBZMOD.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetField("KiDamage");
            var KiDrainRate = DBZGoatLib.DBZMOD.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetField("kiDrainMulti");

            if (modPlayer.IsKiDepleted() || (TransformationHandler.IsAnythingBut(player, Type, true) && !Stackable))
            {
                TransformationHandler.ClearTransformations(player);
                return;
            }

            Lighting.AddLight(player.Center + player.velocity * 8f, 0.84f, 0.59f, 0.95f);

            float drain = GPlayer.ModPlayer(player).GetMastery(Type) < 1f ? kiDrainRate : kiDrainRateWithMastery;

            player.statDefense += baseDefenceBonus;

            modPlayer.AddKi(drain * -1f, false, true);

            player.moveSpeed *= 1f + (speedMulti -1f) * modPlayer.bonusSpeedMultiplier;
            player.maxRunSpeed *= 1f + (speedMulti - 1f) * modPlayer.bonusSpeedMultiplier;
            player.runAcceleration *= 1f + (speedMulti - 1f) * modPlayer.bonusSpeedMultiplier;
            if (player.jumpSpeedBoost < 1f)
            {
                player.jumpSpeedBoost = 1f;
            }

            player.jumpSpeedBoost *= 1f + (speedMulti - 1f) * modPlayer.bonusSpeedMultiplier;
            player.GetDamage(DamageClass.Generic) += damageMulti;


            KiDrainRate.SetValue(modPlayer, attackDrainMulti);
            KiDamage.SetValue(modPlayer, modPlayer.KiDamage * (1f + damageMulti));

            base.Update(player, ref buffIndex);
        }
    }
}
