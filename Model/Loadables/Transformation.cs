using System;
using System.Linq;
using System.Text;

using DBZGoatLib.Handlers;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace DBZGoatLib.Model {

    public abstract class Transformation : ModBuff, IDBZLoadable {
        public float damageMulti;
        public float speedMulti;
        public float kiDrainRate;
        public float kiDrainRateWithMastery;
        public float attackDrainMulti;
        public int baseDefenceBonus;

        /// <summary>
        /// Full name of this form.
        /// </summary>
        public abstract string FormName();

        /// <summary>
        /// This form can be stacked with other forms. Only return true if you you know how to balance it!
        /// </summary>
        public abstract bool Stackable();

        /// <summary>
        /// Color for the transformation's name.
        /// </summary>
        public abstract Color TextColor();

        /// <summary>
        /// This form's sound data. Return an empty/new SoundData to have no startup or looping audio.
        /// </summary>
        public abstract SoundData SoundData();

        /// <summary>
        /// This form's aura data. Return an empty/new AuraData to not have a saiyan aura.
        /// </summary>
        public abstract AuraData AuraData();

        /// <summary>
        /// This form's Ki Bar color change. Return null if the form should not change the Ki Bar's color.
        /// </summary>
        public abstract Gradient KiBarGradient();

        /// <summary>
        /// The texture path for this form's saiyan hair. Return null if the form should not change the player's hair layer.
        /// </summary>
        public abstract string HairTexturePath();

        /// <summary>
        /// Whether this saiyan form should render electric sparks around the user. Defaults to false.
        /// </summary>
        public abstract bool SaiyanSparks();

        /// <summary>
        /// The condition which is checked to see whether or not the player can enter this form or not. 
        /// If not overwritten, this will simply if the user is in this form already and make no additional checks. 
        /// </summary>
        /// <param name="player">Player instance.</param>
        /// <returns>Whether the user can enter this form (True) or not (False).</returns>
        public abstract bool CanTransform(Player player);

        /// <summary>
        /// Code to be executed when the player successfully enters this form. 
        /// </summary>
        /// <param name="player">Player which just transformed.</param>
        public abstract void OnTransform(Player player);

        /// <summary>
        /// Code to be executed when the player succesfully exits this form.
        /// Not mandatory.
        /// </summary>
        /// <param name="player">Player who just left this form.</param>
        public abstract void PostTransform(Player player);

        public AnimationData AnimationData => new AnimationData(AuraData(), SaiyanSparks(), SoundData(), HairTexturePath());
        public TransformationInfo Info => new TransformationInfo(
            Type, Name, Stackable(), FormName(), TextColor(), CanTransform, OnTransform, PostTransform, AnimationData, KiBarGradient());

        public override void Load()
        {
            if(Type != 0)
                TransformationHandler.RegisterTransformation(Info);
        }
        public override void Unload()
        {
            TransformationHandler.UnregisterTransformation(Info);
        }

        public override void SetStaticDefaults() {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;

            DisplayName.SetDefault(FormName());
            Description.SetDefault(BuildTooltip());
        }

        public string BuildTooltip() {
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

            return sb.ToString();
        }

        public override void Update(Player player, ref int buffIndex) {
            var MyPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));

            dynamic modPlayer = MyPlayer.GetMethod("ModPlayer").Invoke(null, new object[] { player });
            var KiDamage = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetField("KiDamage");
            var KiDrainRate = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetField("kiDrainMulti");

            if (modPlayer.IsKiDepleted() || (TransformationHandler.IsAnythingBut(player, Type) && !Stackable())) {
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
            player.GetDamage(DamageClass.Generic) *= damageMulti;

            KiDrainRate.SetValue(modPlayer, attackDrainMulti);
            KiDamage.SetValue(modPlayer, modPlayer.KiDamage * damageMulti);

            base.Update(player, ref buffIndex);
        }
    }
}