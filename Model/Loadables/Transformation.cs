using System;
using System.Linq;
using System.Reflection;
using System.Text;

using DBZGoatLib.Handlers;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
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
            if (Type != 0)
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
        }

        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            tip = BuildTooltip();
        }
        
        public string BuildTooltip() {
            float dmg = damageMulti - 1f;
            float speed = speedMulti - 1f;

            float num1 = 60f * kiDrainRate;
            float num2 = 60f * kiDrainRateWithMastery;

            StringBuilder sb = new StringBuilder();

            if (dmg != 0f)
                sb.Append($"Damage {(dmg > 0 ? '+' : "")}{dmg:P2} ");
            if (speed != 0f)
                if(dmg != 0)
                    sb.AppendLine($" | Speed {(speed > 0 ? '+' : "")}{speed:P2}");
                else
                    sb.AppendLine($"Speed {(speed > 0 ? '+' : "")}{speed:P2}");
            if (baseDefenceBonus != 0)
                sb.Append($"Defense {(baseDefenceBonus > 0 ? '+' : "")}{baseDefenceBonus:N0}");
            if (attackDrainMulti != 0f)
                if(baseDefenceBonus != 0)
                    sb.AppendLine($" | Ki Costs {(attackDrainMulti > 0 ? '+' : '-')}{attackDrainMulti:P2}");
                else
                    sb.AppendLine($"Ki Costs {(attackDrainMulti > 0 ? '+' : '-')}{attackDrainMulti:P2}");
            if(kiDrainRate != 0)
                sb.Append($"Ki Drain {MathF.Round(num1):N0}/s, {MathF.Round(num2):N0}/s when mastered");

            return sb.ToString();
        }

        public override void Update(Player player, ref int buffIndex) {
            TypeInfo MyPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));

            dynamic modPlayer = MyPlayer.GetMethod("ModPlayer").Invoke(null, [player]);
            FieldInfo KiDamage = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetField("KiDamage");
            FieldInfo KiDrainRate = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetField("kiDrainMulti");

            if (modPlayer.IsKiDepleted() || (TransformationHandler.IsAnythingBut(player, Type) && !Stackable())) {
                TransformationHandler.ClearTransformations(player);
                return;
            }

            if (TransformationHandler.IsKaioken(player) && !Stackable())
            {
                TransformationHandler.EndTranformation(player, TransformationHandler.GetTransformation(Type).Value);
                return;
            }

            Lighting.AddLight(player.Center + player.velocity * 8f, 0.84f, 0.59f, 0.95f);

            float drain = GPlayer.ModPlayer(player).GetMastery(Type) < 1f ? kiDrainRate : kiDrainRateWithMastery;


            if(baseDefenceBonus != 0)
                player.statDefense += baseDefenceBonus;
            
            if(drain != 0)
                modPlayer.AddKi(drain * -1f, false, true);

            if(speedMulti != 0)
            {
                player.moveSpeed *= 1f + (speedMulti - 1f) * modPlayer.bonusSpeedMultiplier;
                player.maxRunSpeed *= 1f + (speedMulti - 1f) * modPlayer.bonusSpeedMultiplier;
                player.runAcceleration *= 1f + (speedMulti - 1f) * modPlayer.bonusSpeedMultiplier;
                if (player.jumpSpeedBoost < 1f)
                {
                    player.jumpSpeedBoost = 1f;
                }

                player.jumpSpeedBoost *= 1f + (speedMulti - 1f) * modPlayer.bonusSpeedMultiplier;
            }
            
            if(damageMulti != 0)
            {
                player.GetDamage(DamageClass.Generic) *= damageMulti;

                KiDamage.SetValue(modPlayer, modPlayer.KiDamage * damageMulti);
            }
            
            if(attackDrainMulti != 0)
                KiDrainRate.SetValue(modPlayer, attackDrainMulti);

            base.Update(player, ref buffIndex);
        }
    }
}