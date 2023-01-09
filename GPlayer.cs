using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using DBZGoatLib.Handlers;
using DBZGoatLib.Model;
using DBZGoatLib.UI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace DBZGoatLib {

    public class GPlayer : ModPlayer {
        public int lightningFrameTimer;
        public int lightning3FrameCount = 9;
        public int lightning3FrameIndex;
        public int lightning3FrameTime;
        public int lightning3FrameTimer = 6;

        public readonly Dictionary<int, float> Masteries = new();
        public readonly Dictionary<int, bool> MasteryMaxed = new();

        internal KeyValuePair<uint, ActiveSound> auraSoundInfo;
        internal int playerIndexWithLocalAudio;
        internal int auraSoundtimer = 0;
        internal int auraFrameTimer = 0;
        internal int auraCurrentFrame = 0;
        internal AnimationData currentAnimation;
        internal AnimationData previousAnimation;

        internal DateTime? LastMasteryTick;
        internal DateTime? LastHitEnemy;
        internal DateTime? LastHit;

        public bool Traited;
        public string Trait;
        public float MasteryMultiplier = 1f;

        internal string SavedTree;
        internal string SavedSelection;
        public override void SaveData(TagCompound tag) {
            foreach (var trans in TransformationHandler.Transformations) {
                if (Masteries.TryGetValue(trans.buffID, out float mastery) && !tag.ContainsKey($"DBZGoatLib_{trans.buffKeyName}")) {
                    tag.Add($"DBZGoatLib_{trans.buffKeyName}", mastery);
                }
                if (MasteryMaxed.TryGetValue(trans.buffID, out bool maxed) && !tag.ContainsKey($"DBZGoatLib_{trans.buffKeyName}_Maxed")) {
                    tag.Add($"DBZGoatLib_{trans.buffKeyName}_Maxed", maxed);
                }
            }
            tag.Add("DBZGoatLib_SelectedPanel", UIHandler.TruePanels[UIHandler.ActivePanel].Name);
            tag.Add("DBZGoatLib_SelectedForm", TransformationMenu.ActiveForm);
            tag.Add("DBZGoatLib_Trait", Trait);
            tag.Add("DBZGoatLib_Traited", Traited);
        }

        public override void LoadData(TagCompound tag) {
            foreach (var trans in TransformationHandler.Transformations) {
                if (tag.ContainsKey($"DBZGoatLib_{trans.buffKeyName}")) {
                    if (!Masteries.ContainsKey(trans.buffID))
                        Masteries.Add(trans.buffID, tag.GetFloat($"DBZGoatLib_{trans.buffKeyName}"));
                    else Masteries[trans.buffID] = tag.GetFloat($"DBZGoatLib_{trans.buffKeyName}");
                }
                if (tag.ContainsKey($"DBZGoatLib_{trans.buffKeyName}_Maxed")) {
                    if (!MasteryMaxed.ContainsKey(trans.buffID))
                        MasteryMaxed.Add(trans.buffID, tag.GetBool($"DBZGoatLib_{trans.buffKeyName}_Maxed"));
                    else MasteryMaxed[trans.buffID] = tag.GetBool($"DBZGoatLib_{trans.buffKeyName}_Maxed");
                }
            }
            if (tag.ContainsKey("DBZGoatLib_SelectedForm"))
                SavedSelection = tag.GetString("DBZGoatLib_SelectedForm");
            if (tag.ContainsKey("DBZGoatLib_SelectedPanel"))
                SavedTree = tag.GetString("DBZGoatLib_SelectedPanel");

            if (tag.ContainsKey("DBZGoatLib_Trait"))
                Trait = tag.GetString("DBZGoatLib_Trait");
            if (tag.ContainsKey("DBZGoatLib_Traited"))
                Traited = tag.GetBool("DBZGoatLib_Traited");
        }
        public override void OnEnterWorld(Player player) {
            if (player.whoAmI != Player.whoAmI)
                return;

            foreach (var trans in TransformationHandler.Transformations) {
                if (!Masteries.ContainsKey(trans.buffID))
                    Masteries.Add(trans.buffID, 0f);
                if (!MasteryMaxed.ContainsKey(trans.buffID))
                    MasteryMaxed.Add(trans.buffID, false);
            }

            if(!string.IsNullOrEmpty(SavedTree))
                UIHandler.TryChangePanel(SavedTree);
            if (!string.IsNullOrEmpty(SavedSelection))
                TransformationMenu.ActiveForm = SavedSelection;
            UIHandler.Dirty = true;

            if (!Traited)
                RollTraits();
            else
            {
                var traitInfo = TraitHandler.GetTraitByName(Trait);
                if(traitInfo.HasValue)
                    traitInfo.Value.IfTrait(Player);
            }
        }
        internal void ClearDBTTrait()
        {
            var MyPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));

            var playerTrait = MyPlayer.GetField("playerTrait");

            var modPlayer = MyPlayer.GetMethod("ModPlayer").Invoke(null, new object[] { Player });

            playerTrait.SetValue(modPlayer, "");
        }
        public void RollTraits()
        {
            ClearDBTTrait();

            var rolled = TraitHandler.RollTrait();

            Trait = rolled.Name;

            rolled.IfTrait(Player);

            Traited = true;

            TransformationMenu.Dirty = true;
        }
        public void RerollTraits()
        {
            ClearDBTTrait();

            var current = TraitHandler.GetTraitByName(Trait);

            if (current.HasValue)
                current.Value.IfUntrait(Player);

            var rolled = TraitHandler.RollTrait(false, Trait);

            Trait = rolled.Name;

            rolled.IfTrait(Player);

            TransformationMenu.Dirty = true;
        }
        public override void PlayerDisconnect(Player player) => TransformationHandler.ClearTransformations(player);

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
            if (TransformationHandler.IsTransformed(drawInfo.drawPlayer)) {
                var pClass = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));
                var modPlayer = pClass.GetMethod("ModPlayer").Invoke(null, new object[] { drawInfo.drawPlayer });

                pClass.GetField("isCharging").SetValue(modPlayer, false);
            }
        }
        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            if (TransformationHandler.IsTransformed(drawInfo.drawPlayer))
            {
                var form = TransformationHandler.GetCurrentTransformation(drawInfo.drawPlayer);
                var stackable = TransformationHandler.GetCurrentStackedTransformation(drawInfo.drawPlayer);

                if (stackable.HasValue && !form.HasValue)
                {
                    if (!string.IsNullOrEmpty(stackable.Value.animationData.HairPath))
                    {
                        drawInfo.drawPlayer.head = 0;
                    }
                }
                else if (form.HasValue)
                {
                    if (!string.IsNullOrEmpty(form.Value.animationData.HairPath))
                    {
                        drawInfo.drawPlayer.head = 0;
                    }
                }  
            }
        }
        public override void PreUpdateMovement() {
            if (TransformationHandler.IsTransformed(Player)) {
                var forms = TransformationHandler.GetCurrentTransformation(Player) ?? TransformationHandler.GetCurrentStackedTransformation(Player);

                currentAnimation = forms.Value.animationData;

                if (!currentAnimation.Equals(previousAnimation)) {
                    auraSoundInfo = SoundHandler.KillTrackedSound(auraSoundInfo);
                    HandleAuraStartupSound(currentAnimation);
                    auraSoundtimer = 0;
                    auraFrameTimer = 0;
                }
            } else {
                SoundHandler.KillTrackedSound(auraSoundInfo);
                currentAnimation = new AnimationData();
            }
            previousAnimation = currentAnimation;
            HandleAuraLoopSound(currentAnimation);
            IncrementAuraFrameTimers(currentAnimation.Aura);
        }

        public static GPlayer ModPlayer(Player player) => player.GetModPlayer<GPlayer>();

        /// <summary>
        /// Gets the player's mastery. Only works with GoatLib transformations.
        /// </summary>
        /// <param name="BuffId">Int Buff ID of the transformation.</param>
        /// <returns>Mastery value.</returns>
        public float GetMastery(int BuffId) {
            if (Masteries.TryGetValue(BuffId, out var mastery))
                return mastery;

            else return 0f;
        }

        /// <summary>
        /// Gets the player's mastery. Works with both GoatLib and DBT transformations.
        /// </summary>
        /// <param name="buffKeyName">Class Name of the buff.</param>
        /// <returns>Mastery value.</returns>
        public float GetMastery(string buffKeyName)
        {
            if (Defaults.MasteryPaths.TryGetValue(buffKeyName, out var masteryPath))
            {
                var ModPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));
                dynamic myPlayer = ModPlayer.GetMethod("ModPlayer").Invoke(null, new object[] { Player });

                var path = ModPlayer.GetField(masteryPath);

                return (float)path.GetValue(myPlayer);
            }
            return 0f;
        }
        
        public void HandleAuraLoopSound(AnimationData data) {
            if (data.Sound.Equals(new SoundData()))
                return;
            if (data.Sound.LoopSoundDuration <= 0 || string.IsNullOrEmpty(data.Sound.LoopAudioPath))
                return;
            if (SoundHandler.ShouldPlayPlayerAudio(Player, true)) {
                if (auraSoundtimer == 0)
                    auraSoundInfo = SoundHandler.PlaySound(data.Sound.LoopAudioPath, Player, 0.7f);
                auraSoundtimer++;
                if (auraSoundtimer >= data.Sound.LoopSoundDuration)
                    auraSoundtimer = 0;
            }
            SoundHandler.UpdateTrackedSound(auraSoundInfo, Player.position);
        }

        public void HandleAuraStartupSound(AnimationData data) {
            if (data.Equals(new AnimationData()))
                return;
            if (data.Sound.Equals(new SoundData()))
                return;
            if (string.IsNullOrEmpty(data.Sound.StartAudioPath))
                return;
            SoundHandler.PlaySound(data.Sound.StartAudioPath, Player, 0.7f, 0.1f);
        }

        public void IncrementAuraFrameTimers(AuraData aura) {
            if (aura.Equals(new AuraData()))
                return;
            dynamic modPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetMethod("ModPlayer").Invoke(null, new object[] { Player });
            if (modPlayer.isCharging)
                ++auraFrameTimer;
            ++auraFrameTimer;
            if (auraFrameTimer >= 3) {
                auraFrameTimer = 0;
                ++auraCurrentFrame;
            }
            if (auraCurrentFrame < aura.Frames)
                return;
            auraCurrentFrame = 0;
        }

        public override void PostUpdate() {
            if (TransformationHandler.IsTransformed(Player)) {
                if (TransformationHandler.GetAllCurrentForms(Player).Any(x=>x.animationData.Sparks)) {
                    lightning3FrameTime++;
                }
            }
            if (lightning3FrameTime >= lightning3FrameTimer) {
                lightning3FrameTime = 0;
                lightning3FrameIndex++;
                if (lightning3FrameIndex >= lightning3FrameCount) {
                    lightning3FrameIndex = 0;
                }
            }
            if (lightningFrameTimer >= 15) {
                lightningFrameTimer = 0;
            }

            if (!TransformationHandler.IsTransformed(Player))
                LastMasteryTick = null;
            if (!LastMasteryTick.HasValue && TransformationHandler.IsTransformed(Player))
                LastMasteryTick = DateTime.Now;
            if (LastMasteryTick.HasValue && TransformationHandler.IsTransformed(Player))
                if ((DateTime.Now - LastMasteryTick.Value).TotalSeconds >= 1) {
                    LastMasteryTick = DateTime.Now;
                    var transformation = TransformationHandler.GetAllCurrentForms(Player);

                    foreach (var form in transformation)
                        HandleMasteryGain(form);
                }
        }

        public void HandleMasteryGain(TransformationInfo transformation) {
            if (Masteries.TryGetValue(transformation.buffID, out float value)) {
                if (value >= 1f) {
                    Masteries[transformation.buffID] = 1f;
                    if (MasteryMaxed.TryGetValue(transformation.buffID, out bool mastered)) {
                        if (!mastered) {
                            MasteryMaxed[transformation.buffID] = true;
                            Main.NewText($"You've mastered {transformation.transformationText}.");
                        }
                    }
                    return;
                } else {
                    Masteries[transformation.buffID] = Math.Min(1f, value + (0.00232f * MasteryMultiplier));
                }
            }
            else if (Defaults.MasteryPaths.TryGetValue(transformation.buffKeyName,out string path))
            {
                var myPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));
                var instance = myPlayer.GetMethod("ModPlayer").Invoke(null, new object[] { Player });
                var masteryField = myPlayer.GetField(path, DBZGoatLib.flagsAll);
                float mastery = (float)masteryField.GetValue(instance);
                masteryField.SetValue(instance, Math.Min(1f, mastery + (0.00232f * MasteryMultiplier)));

            }
        }

        public override void OnHitAnything(float x, float y, Entity victim) {
            if (victim is NPC)
                if ((victim as NPC).type == NPCID.TargetDummy)
                    return;
            if (!TransformationHandler.IsTransformed(Player))
                return;
            if (!LastHitEnemy.HasValue)
                LastHitEnemy = DateTime.Now;

            if ((DateTime.Now - LastHitEnemy.Value).TotalMilliseconds < 500)
                return;
            LastHitEnemy = DateTime.Now;

            var transformation = TransformationHandler.GetAllCurrentForms(Player);

            foreach (var form in transformation)
                HandleMasteryGain(form);
        }

        public override void OnHitByNPC(NPC npc, int damage, bool crit) {
            if (!TransformationHandler.IsTransformed(Player))
                return;

            if (!LastHit.HasValue)
                LastHit = DateTime.Now;

            if ((DateTime.Now - LastHit.Value).TotalMilliseconds < 500)
                return;
            LastHit = DateTime.Now;

            var transformation = TransformationHandler.GetAllCurrentForms(Player);

            foreach (var form in transformation)
                HandleMasteryGain(form);
        }

        public override void OnHitByProjectile(Projectile proj, int damage, bool crit) {
            if (!TransformationHandler.IsTransformed(Player))
                return;

            if (!LastHit.HasValue)
                LastHit = DateTime.Now;

            if ((DateTime.Now - LastHit.Value).TotalMilliseconds < 500)
                return;
            LastHit = DateTime.Now;

            var transformation = TransformationHandler.GetAllCurrentForms(Player);

            foreach (var form in transformation)
                HandleMasteryGain(form);
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (DBZGoatLib.OpenMenu.JustPressed)
                TransformationMenu.Visible ^= true;
            DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("TransMenu")).GetField("menuvisible").SetValue(null, false);
            ProcessTransformationTriggers();
        }

        public override void OnRespawn(Player player)
        {
            TransformationHandler.ClearTransformations(player);
            base.OnRespawn(player);
        }

        public void ProcessTransformationTriggers()
        {
            var transformation = FetchTransformation();

            if (!transformation.HasValue && TransformationHandler.PowerDownKey.JustPressed)
                TransformationHandler.ClearTransformations(Player);

            else if (transformation.HasValue)
                TransformationHandler.Transform(Player, transformation.Value);
        }
        public TransformationInfo? FetchTransformation()
        {
            if (TransformationHandler.TransformKey.JustPressed)
            {
                if (TransformationHandler.IsTransformed(Player, true))
                {
                    var current = TransformationHandler.GetCurrentTransformation(Player);
                    if (!current.HasValue)
                        return null;

                    var chain = TransformationHandler.GetChain(current.Value, TransformationHandler.EnergyChargeKey.Current);
                    if (!chain.HasValue)
                        return null;
                    if (string.IsNullOrEmpty(chain.Value.NextTransformationBuffKeyName))
                        return null;
                    return TransformationHandler.GetTransformation(chain.Value.NextTransformationBuffKeyName);
                }
                else
                {
                    return string.IsNullOrEmpty(TransformationMenu.ActiveForm) ? TransformationHandler.GetTransformation("SSJ1Buff") : TransformationHandler.GetTransformation(TransformationMenu.ActiveForm);
                }
            }
            else if (TransformationHandler.PowerDownKey.JustPressed)
            {
                if (TransformationHandler.IsTransformed(Player, true))
                {
                    var current = TransformationHandler.GetCurrentTransformation(Player);
                    if (!current.HasValue)
                        return null;

                    if (current.Value.buffKeyName == TransformationMenu.ActiveForm)
                        return null;

                    var chain = TransformationHandler.GetChain(current.Value, TransformationHandler.EnergyChargeKey.Current);
                    if (!chain.HasValue)
                        return null;
                    if (string.IsNullOrEmpty(chain.Value.PreviousTransformationBuffKeyName))
                        return null;
                    return TransformationHandler.GetTransformation(chain.Value.PreviousTransformationBuffKeyName);
                }
                else
                    return null;
            }
            else
                return null;
        }
    }
}