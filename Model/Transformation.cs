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

        public override void ModifyBuffTip(ref string tip, ref int rare)
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

            sb.Append($"Mastery: {(int)(GPlayer.ModPlayer(Main.CurrentPlayer).GetMastery(Type) * 100f)}%");

            tip = sb.ToString();
        }

        public override void Update(Player player, ref int buffIndex)
        {
            var MyPlayer = DBZGoatLib.DBZMOD.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));

            dynamic modPlayer = MyPlayer.GetMethod("ModPlayer").Invoke(null, new object[] { player });
            var KiDamage = DBZGoatLib.DBZMOD.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetField("KiDamage");
            var KiDrainRate = DBZGoatLib.DBZMOD.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetField("kiDrainMulti");

            if (modPlayer.IsKiDepleted() || TransformationHandler.IsAnythingBut(player, Type))
            {
                TransformationHandler.ClearTransformations(player);
                return;
            }

            Lighting.AddLight(player.Center + player.velocity * 8f, 0.84f, 0.59f, 0.95f);

            float drain = GPlayer.ModPlayer(player).GetMastery(Type) < 1f ? kiDrainRate : kiDrainRateWithMastery;

            player.statDefense += baseDefenceBonus;

            modPlayer.AddKi(drain * -1f, false, true);

            player.moveSpeed *= speedMulti * modPlayer.bonusSpeedMultiplier;
            player.maxRunSpeed *= speedMulti * modPlayer.bonusSpeedMultiplier;
            player.runAcceleration *= speedMulti * modPlayer.bonusSpeedMultiplier;
            if (player.jumpSpeedBoost < 1f)
            {
                player.jumpSpeedBoost = 1f;
            }

            player.jumpSpeedBoost *= speedMulti * modPlayer.bonusSpeedMultiplier;
            player.GetDamage(DamageClass.Generic) += damageMulti;


            KiDrainRate.SetValue(modPlayer, attackDrainMulti);
            KiDamage.SetValue(modPlayer, modPlayer.KiDamage * (1f + damageMulti));

            base.Update(player, ref buffIndex);
        }
    }
}
