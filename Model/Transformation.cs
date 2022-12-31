using System;
using System.Linq;
using System.Text;

using DBZGoatLib.Handlers;

using Terraria;
using Terraria.ModLoader;

namespace DBZGoatLib.Model {

    public abstract class Transformation : ModBuff {
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

        public override void SetStaticDefaults() {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;

            Description.SetDefault(BuildTooltip());
        }

        public override void ModifyBuffTip(ref string tip, ref int rare) {
            tip = BuildTooltip();
        }

        public string BuildTooltip() {
            if (tipMastery != mastery || tipMastery == 0) {
                var speed = speedMulti - 1f;

                float num1 = 60f * kiDrainRate;
                float num2 = 60f * kiDrainRateWithMastery;

                StringBuilder sb = new StringBuilder();

                if (damageMulti != 0f)
                    sb.Append($"Damage {(damageMulti > 0 ? '+' : '-')}{damageMulti:P2} ");
                if (speed != 0f)
                    sb.AppendLine($"Speed {(speed > 0 ? '+' : '-')}{speed:P2}");
                if (attackDrainMulti != 0f)
                    sb.AppendLine($"Ki Costs {(attackDrainMulti > 0 ? '+' : '-')}{attackDrainMulti:P2}");

                sb.AppendLine($"Ki Drain {MathF.Round(num1):N0}/s, {MathF.Round(num2):N0}/s when mastered");

                sb.Append($"Mastery: {mastery}%");

                tipCache = sb.ToString();
                tipMastery = mastery;

                return sb.ToString();
            } else {
                return tipCache;
            }
        }

        public override void Update(Player player, ref int buffIndex) {
            mastery = (int)(GPlayer.ModPlayer(player).GetMastery(Type) * 100f);

            var MyPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));

            dynamic modPlayer = MyPlayer.GetMethod("ModPlayer").Invoke(null, new object[] { player });
            var KiDamage = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetField("KiDamage");
            var KiDrainRate = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetField("kiDrainMulti");

            if (modPlayer.IsKiDepleted() || (TransformationHandler.IsAnythingBut(player, Type, true) && !Stackable)) {
                TransformationHandler.ClearTransformations(player);
                return;
            }

            Lighting.AddLight(player.Center + player.velocity * 8f, 0.84f, 0.59f, 0.95f);

            float drain = GPlayer.ModPlayer(player).GetMastery(Type) < 1f ? kiDrainRate : kiDrainRateWithMastery;

            player.statDefense += baseDefenceBonus;

            modPlayer.AddKi(drain * -1f, false, true);

            player.moveSpeed *= 1f + (speedMulti - 1f) * modPlayer.bonusSpeedMultiplier;
            player.maxRunSpeed *= 1f + (speedMulti - 1f) * modPlayer.bonusSpeedMultiplier;
            player.runAcceleration *= 1f + (speedMulti - 1f) * modPlayer.bonusSpeedMultiplier;
            if (player.jumpSpeedBoost < 1f) {
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