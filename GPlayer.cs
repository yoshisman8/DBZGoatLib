using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
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

namespace DBZGoatLib
{
    public class GPlayer : ModPlayer
    {
        #region Mastery Dictionaries

        public readonly Dictionary<int, float> Masteries = new();
        public readonly Dictionary<int, bool> MasteryMaxed = new();

        #endregion

        #region Animation Variables

        internal KeyValuePair<uint, ActiveSound> auraSoundInfo;
        internal KeyValuePair<uint, ActiveSound> techniqueSoundInfo;
        internal int playerIndexWithLocalAudio;
        internal int auraSoundtimer = 0;
        internal int techniqueSoundTimer = 0;
        internal int formAuraFrameTimer = 0;
        internal int techniqueAuraFrameTimer = 0;
        internal int auraCurrentFrame = 0;
        internal int techniqueCurrentFrame = 0;
        internal AnimationData? currentForm;
        internal AnimationData? previousFrom;
        internal AnimationData? currentTechnique;
        internal AnimationData? previousTechnique;
        public int lightningFrameTimer;
        public int lightning3FrameCount = 9;
        public int lightning3FrameIndex;
        public int lightning3FrameTime;
        public int lightning3FrameTimer = 6;

        #endregion

        #region Timers

        internal DateTime? LastMasteryTick;
        internal DateTime? LastHitEnemy;
        internal DateTime? LastHit;

        #endregion

        #region Trait Variables

        public bool Traited;
        public string Trait;
        public float MasteryMultiplier = 1f;

        #endregion

        #region Internal Variables

        internal string SavedTree;
        internal string SavedSelection;
        internal bool isCharging;

        #endregion

        #region Data Handling

        public override void SaveData(TagCompound tag)
        {
            foreach (TransformationInfo trans in TransformationHandler.Transformations)
            {
                if (Masteries.TryGetValue(trans.buffID, out float mastery) &&
                    !tag.ContainsKey($"DBZGoatLib_{trans.buffKeyName}"))
                {
                    tag.Add($"DBZGoatLib_{trans.buffKeyName}", mastery);
                }

                if (MasteryMaxed.TryGetValue(trans.buffID, out bool maxed) &&
                    !tag.ContainsKey($"DBZGoatLib_{trans.buffKeyName}_Maxed"))
                {
                    tag.Add($"DBZGoatLib_{trans.buffKeyName}_Maxed", maxed);
                }
            }

            tag.Add("DBZGoatLib_SelectedPanel", UIHandler.TruePanels[UIHandler.ActivePanel].Name);
            tag.Add("DBZGoatLib_SelectedForm", TransformationMenu.ActiveForm);
            tag.Add("DBZGoatLib_Trait", Trait);
            tag.Add("DBZGoatLib_Traited", Traited);
        }

        public override void LoadData(TagCompound tag)
        {
            foreach (TransformationInfo trans in TransformationHandler.Transformations)
            {
                if (tag.ContainsKey($"DBZGoatLib_{trans.buffKeyName}"))
                {
                    if (!Masteries.ContainsKey(trans.buffID))
                        Masteries.Add(trans.buffID, tag.GetFloat($"DBZGoatLib_{trans.buffKeyName}"));
                    else Masteries[trans.buffID] = tag.GetFloat($"DBZGoatLib_{trans.buffKeyName}");
                }

                if (tag.ContainsKey($"DBZGoatLib_{trans.buffKeyName}_Maxed"))
                {
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

        public override void OnEnterWorld()
        {
            if (Player.whoAmI != Main.LocalPlayer.whoAmI)
                return;
            
            foreach (TransformationInfo trans in TransformationHandler.Transformations)
            {
                Masteries.TryAdd(trans.buffID, 0f);
                MasteryMaxed.TryAdd(trans.buffID, false);
            }

            if (!string.IsNullOrEmpty(SavedTree))
                UIHandler.TryChangePanel(SavedTree);
            if (!string.IsNullOrEmpty(SavedSelection))
                TransformationMenu.ActiveForm = SavedSelection;
            UIHandler.Dirty = true;
            UIHandler.Loaded = true;

            if (!Traited)
                RollTraits();
            else
            {
                TraitInfo? traitInfo = TraitHandler.GetTraitByName(Trait);
                if (traitInfo.HasValue)
                    traitInfo.Value.IfTrait(Player);
            }
        }

        #endregion

        public static GPlayer ModPlayer(Player player) => player.GetModPlayer<GPlayer>();

        public bool IsCharging()
        {
            if (ModLoader.HasMod("DBZMODPORT"))
            {
                dynamic modPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"))
                    .GetMethod("ModPlayer").Invoke(null,
                        [Player]);
                return (bool)modPlayer.isCharging;
            }

            return isCharging;
        }

        public override void PlayerDisconnect() => TransformationHandler.ClearTransformations(Player);
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
            => TransformationHandler.ClearTransformations(Player);

        public override void OnRespawn()
            => TransformationHandler.ClearTransformations(Player);

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (DBZGoatLib.OpenMenu.JustPressed)
                TransformationMenu.Visible ^= true;
            DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("TransMenu")).GetField("menuvisible")
                .SetValue(null, false);
            ProcessTransformationTriggers();
            dynamic myPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"))
                .GetMethod("ModPlayer", DBZGoatLib.flagsAll).Invoke(null,
                    [Player]);
            dynamic dbtConfig = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"))
                .GetField("activeConfig", DBZGoatLib.flagsAll).GetValue(myPlayer);
            if (TransformationHandler.EnergyChargeKey.JustPressed && (bool)dbtConfig.IsChargeToggled)
            {
                isCharging ^= true;
            }
        }

        #region Trait Handling

        internal void ClearDBTTrait()
        {
            TypeInfo MyPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));

            FieldInfo playerTrait = MyPlayer.GetField("playerTrait");

            object modPlayer = MyPlayer.GetMethod("ModPlayer").Invoke(null, [Player]);

            playerTrait.SetValue(modPlayer, "");
        }

        public void RollTraits()
        {
            ClearDBTTrait();

            TraitInfo rolled = TraitHandler.RollTrait();

            SetTrait(rolled);

            Traited = true;
        }

        public void RerollTraits()
        {
            ClearDBTTrait();

            TraitInfo rolled = TraitHandler.RollTrait(false, Trait);

            SetTrait(rolled);
        }

        private void SyncTraits()
        {
            TypeInfo myPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));
            object instance = myPlayer.GetMethod("ModPlayer").Invoke(null, [Player]);
            FieldInfo traitField = myPlayer.GetField("playerTrait");

            string trait = (string)traitField.GetValue(instance);

            if ((trait == "Prodigy" || trait == "Legendary") && trait != Trait)
            {
                TraitInfo? T = TraitHandler.GetTraitByName(trait);
                if (T.HasValue)
                {
                    TraitInfo? current = TraitHandler.GetTraitByName(Trait);

                    if (current.HasValue)
                        current.Value.IfUntrait(Player);

                    Trait = T.Value.Name;

                    T.Value.IfTrait(Player);

                    UIHandler.Dirty = true;
                }
            }
        }

        public void SetTrait(TraitInfo trait)
        {
            TraitInfo? current = TraitHandler.GetTraitByName(Trait);

            current?.IfUntrait?.Invoke(Player);

            Trait = trait.Name;

            trait.IfTrait?.Invoke(Player);

            UIHandler.Dirty = true;
        }

        #endregion

        #region Animation Handling

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (TransformationHandler.IsTransformed(drawInfo.drawPlayer))
            {
                TypeInfo pClass = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));
                object modPlayer = pClass.GetMethod("ModPlayer").Invoke(null, [drawInfo.drawPlayer]);

                pClass.GetField("isCharging").SetValue(modPlayer, false);
            }
        }

        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            if (TransformationHandler.IsTransformed(drawInfo.drawPlayer))
            {
                TransformationInfo? form = TransformationHandler.GetCurrentTransformation(drawInfo.drawPlayer);
                TransformationInfo? stackable =
                    TransformationHandler.GetCurrentStackedTransformation(drawInfo.drawPlayer);

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

        public override void PreUpdateMovement()
        {
            if (TransformationHandler.IsTransformed(Player))
            {
                TransformationInfo? form = TransformationHandler.GetCurrentTransformation(Player);
                TransformationInfo? technique = TransformationHandler.GetCurrentStackedTransformation(Player);

                currentForm = form?.animationData;
                currentTechnique = technique?.animationData;

                if (currentForm.HasValue)
                {
                    if (currentForm != previousFrom)
                    {
                        auraSoundInfo = SoundHandler.KillTrackedSound(auraSoundInfo);
                        HandleAuraStartupSound(currentForm.Value);
                        auraSoundtimer = 0;
                        formAuraFrameTimer = 0;
                    }
                }
                else
                {
                    SoundHandler.KillTrackedSound(auraSoundInfo);
                    currentForm = null;
                }

                if (currentTechnique.HasValue)
                {
                    if (currentTechnique != previousTechnique)
                    {
                        techniqueSoundInfo = SoundHandler.KillTrackedSound(techniqueSoundInfo);
                        HandleAuraStartupSound(currentTechnique.Value);
                        techniqueSoundTimer = 0;
                        techniqueAuraFrameTimer = 0;
                    }
                }
                else
                {
                    SoundHandler.KillTrackedSound(techniqueSoundInfo);
                    currentTechnique = null;
                }
            }
            else
            {
                SoundHandler.KillTrackedSound(auraSoundInfo);
                SoundHandler.KillTrackedSound(techniqueSoundInfo);
                currentForm = null;
                currentTechnique = null;
            }

            previousFrom = currentForm;
            previousTechnique = currentTechnique;

            HandleAuraLoopSound(currentForm, currentTechnique);
            IncrementAuraFrameTimers(currentForm, currentTechnique);
        }

        public void HandleAuraLoopSound(AnimationData? form, AnimationData? technique)
        {
            if (!form.HasValue && !technique.HasValue)
                return;
            if (form.HasValue)
            {
                if (!form.Value.Sound.Equals(new SoundData()))
                {
                    if (form.Value.Sound.LoopSoundDuration <= 0 || string.IsNullOrEmpty(form.Value.Sound.LoopAudioPath))
                        return;
                    if (SoundHandler.ShouldPlayPlayerAudio(Player, true))
                    {
                        if (auraSoundtimer == 0)
                            auraSoundInfo = SoundHandler.PlaySound(form.Value.Sound.LoopAudioPath, Player, 0.7f);
                        auraSoundtimer++;
                        if (auraSoundtimer >= form.Value.Sound.LoopSoundDuration)
                            auraSoundtimer = 0;
                    }

                    SoundHandler.UpdateTrackedSound(auraSoundInfo, Player.position);
                }
            }

            if (technique.HasValue)
            {
                if (!technique.Value.Sound.Equals(new SoundData()))
                {
                    if (technique.Value.Sound.LoopSoundDuration <= 0 ||
                        string.IsNullOrEmpty(technique.Value.Sound.LoopAudioPath))
                        return;
                    if (SoundHandler.ShouldPlayPlayerAudio(Player, true))
                    {
                        if (techniqueSoundTimer == 0)
                            techniqueSoundInfo =
                                SoundHandler.PlaySound(technique.Value.Sound.LoopAudioPath, Player, 0.7f);
                        techniqueSoundTimer++;
                        if (techniqueSoundTimer >= technique.Value.Sound.LoopSoundDuration)
                            techniqueSoundTimer = 0;
                    }

                    SoundHandler.UpdateTrackedSound(techniqueSoundInfo, Player.position);
                }
            }
        }

        public void HandleAuraStartupSound(AnimationData data)
        {
            if (data.Equals(new AnimationData()))
                return;
            if (data.Sound.Equals(new SoundData()))
                return;
            if (string.IsNullOrEmpty(data.Sound.StartAudioPath))
                return;
            SoundHandler.PlaySound(data.Sound.StartAudioPath, Player, 0.7f, 0.1f);
        }

        public void IncrementAuraFrameTimers(AnimationData? form, AnimationData? technique)
        {
            if (form.HasValue)
            {
                if (form.Value.Aura.Frames > 0)
                {
                    if (IsCharging())
                        ++formAuraFrameTimer;
                    ++formAuraFrameTimer;
                    if (formAuraFrameTimer >= 3)
                    {
                        formAuraFrameTimer = 0;
                        ++auraCurrentFrame;
                    }

                    if (auraCurrentFrame >= form.Value.Aura.Frames)
                        auraCurrentFrame = 0;
                }
            }

            if (technique.HasValue)
            {
                if (technique.Value.Aura.Frames > 0)
                {
                    if (IsCharging())
                        ++techniqueAuraFrameTimer;
                    ++techniqueAuraFrameTimer;
                    if (techniqueAuraFrameTimer >= 3)
                    {
                        techniqueAuraFrameTimer = 0;
                        ++techniqueCurrentFrame;
                    }

                    if (techniqueCurrentFrame >= technique.Value.Aura.Frames)
                        techniqueCurrentFrame = 0;
                }
            }
        }

        public override void PreUpdate()
        {
            TypeInfo pClass = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));
            object modPlayer = pClass.GetMethod("ModPlayer").Invoke(null, [Player]);

            bool value = (bool)pClass.GetField("isCharging").GetValue(modPlayer);

            if (isCharging != value)
                pClass.GetField("isCharging").SetValue(modPlayer, isCharging);
        }

        public override void PostUpdate()
        {
            if (TransformationHandler.IsTransformed(Player))
            {
                if (TransformationHandler.GetAllCurrentForms(Player).Any(x => x.animationData.Sparks))
                {
                    lightning3FrameTime++;
                }
            }

            if (lightning3FrameTime >= lightning3FrameTimer)
            {
                lightning3FrameTime = 0;
                lightning3FrameIndex++;
                if (lightning3FrameIndex >= lightning3FrameCount)
                {
                    lightning3FrameIndex = 0;
                }
            }

            if (lightningFrameTimer >= 15)
            {
                lightningFrameTimer = 0;
            }

            if (!TransformationHandler.IsTransformed(Player))
                LastMasteryTick = null;
            if (!LastMasteryTick.HasValue && TransformationHandler.IsTransformed(Player))
                LastMasteryTick = DateTime.Now;
            if (LastMasteryTick.HasValue && TransformationHandler.IsTransformed(Player))
                if ((DateTime.Now - LastMasteryTick.Value).TotalSeconds >= 1)
                {
                    LastMasteryTick = DateTime.Now;
                    TransformationInfo[] transformation = TransformationHandler.GetAllCurrentForms(Player);

                    foreach (TransformationInfo form in transformation)
                        HandleMasteryGain(form);
                }

            SyncTraits();
        }

        #endregion

        #region Mastery Handling

        /// <summary>
        /// Gets the player's mastery. Only works with GoatLib transformations.
        /// </summary>
        /// <param name="BuffId">Int Buff ID of the transformation.</param>
        /// <returns>Mastery value.</returns>
        public float GetMastery(int BuffId)
        {
            if (Masteries.TryGetValue(BuffId, out float mastery))
                return mastery;

            return 0f;
        }

        /// <summary>
        /// Gets the player's mastery. Works with both GoatLib and DBT transformations.
        /// </summary>
        /// <param name="buffKeyName">Class Name of the buff.</param>
        /// <returns>Mastery value.</returns>
        public float GetMastery(string buffKeyName)
        {
            if (Defaults.MasteryPaths.TryGetValue(buffKeyName, out string masteryPath))
            {
                TypeInfo ModPlayer =
                    DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));
                dynamic myPlayer = ModPlayer.GetMethod("ModPlayer").Invoke(null, [Player]);

                FieldInfo path = ModPlayer.GetField(masteryPath);

                return (float)path.GetValue(myPlayer);
            }

            return 0f;
        }

        public void HandleMasteryGain(TransformationInfo transformation)
        {
            if (Masteries.TryGetValue(transformation.buffID, out float value))
            {
                if (value >= 1f)
                {
                    Masteries[transformation.buffID] = 1f;
                    if (MasteryMaxed.TryGetValue(transformation.buffID, out bool mastered))
                    {
                        if (!mastered)
                        {
                            MasteryMaxed[transformation.buffID] = true;
                            Main.NewText($"You've mastered {transformation.transformationText}.");
                        }
                    }

                    return;
                }

                Masteries[transformation.buffID] = Math.Min(1f, value + (0.00232f * MasteryMultiplier));
            }
            else if (Defaults.MasteryPaths.TryGetValue(transformation.buffKeyName, out string path))
            {
                TypeInfo myPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));
                object instance = myPlayer.GetMethod("ModPlayer").Invoke(null, [Player]);
                FieldInfo masteryField = myPlayer.GetField(path, DBZGoatLib.flagsAll);
                float mastery = (float)masteryField.GetValue(instance);
                masteryField.SetValue(instance, Math.Min(1f, mastery + (0.00232f * MasteryMultiplier)));
            }
        }

        public override void OnHitAnything(float x, float y, Entity victim)
        {
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

            TransformationInfo[] transformation = TransformationHandler.GetAllCurrentForms(Player);

            foreach (TransformationInfo form in transformation)
                HandleMasteryGain(form);
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if (!TransformationHandler.IsTransformed(Player))
                return;

            if (!LastHit.HasValue)
                LastHit = DateTime.Now;

            if ((DateTime.Now - LastHit.Value).TotalMilliseconds < 500)
                return;
            LastHit = DateTime.Now;

            TransformationInfo[] transformation = TransformationHandler.GetAllCurrentForms(Player);

            foreach (TransformationInfo form in transformation)
                HandleMasteryGain(form);
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (!TransformationHandler.IsTransformed(Player))
                return;

            if (!LastHit.HasValue)
                LastHit = DateTime.Now;

            if ((DateTime.Now - LastHit.Value).TotalMilliseconds < 500)
                return;
            LastHit = DateTime.Now;

            TransformationInfo[] transformation = TransformationHandler.GetAllCurrentForms(Player);

            foreach (TransformationInfo form in transformation)
                HandleMasteryGain(form);
        }

        #endregion

        #region Tansformation Handling

        public void ProcessTransformationTriggers()
        {
            TransformationInfo? transformation = FetchTransformation();

            if (!transformation.HasValue && TransformationHandler.PowerDownKey.JustPressed)
            {
                TransformationHandler.ClearTransformations(Player);
            }
            else if (transformation.HasValue)
            {
                TransformationHandler.Transform(Player, transformation.Value);
            }
        }

        public TransformationInfo? FetchTransformation()
        {
            if (TransformationHandler.TransformKey.JustPressed)
            {
                if (TransformationHandler.IsTransformed(Player, true))
                {
                    TransformationInfo? current = TransformationHandler.GetCurrentTransformation(Player);
                    
                    if (!current.HasValue) return null;
                    
                    TransformationChain[] chain = TransformationHandler.GetChain(current.Value,
                        TransformationHandler.EnergyChargeKey.Current);
                    foreach (TransformationChain C in chain)
                    {
                        if (string.IsNullOrEmpty(C.NextTransformationBuffKeyName)) continue;
                        TransformationInfo? next =
                            TransformationHandler.GetTransformation(C.NextTransformationBuffKeyName);
                        if (!next.HasValue) continue;
                        if (next.Value.condition.Invoke(Player))
                        {
                            return next;
                        }
                    }

                    return null;
                }
                
                return string.IsNullOrEmpty(TransformationMenu.ActiveForm)
                    ? TransformationHandler.GetTransformation("SSJ1Buff")
                    : TransformationHandler.GetTransformation(TransformationMenu.ActiveForm);
            }

            if (TransformationHandler.PowerDownKey.JustPressed)
            {
                if (TransformationHandler.IsTransformed(Player, true) && TransformationHandler.EnergyChargeKey.Current)
                {
                    TransformationInfo? current = TransformationHandler.GetCurrentTransformation(Player);
                    if (!current.HasValue) return null;
                    TransformationChain[] chain = TransformationHandler.GetChain(current.Value,
                        TransformationHandler.EnergyChargeKey.Current);
                    foreach (TransformationChain C in chain)
                    {
                        if (string.IsNullOrEmpty(C.PreviousTransformationBuffKeyName)) continue;
                        TransformationInfo? prev =
                            TransformationHandler.GetTransformation(C.PreviousTransformationBuffKeyName);
                        if (!prev.HasValue) continue;
                        if (prev.Value.condition(Player))
                        {
                            return prev;
                        }
                    }

                    TransformationChain[] chain2 = TransformationHandler.GetChain(current.Value);
                    foreach (TransformationChain C in chain2)
                    {
                        if (string.IsNullOrEmpty(C.PreviousTransformationBuffKeyName)) continue;
                        TransformationInfo? prev =
                            TransformationHandler.GetTransformation(C.PreviousTransformationBuffKeyName);
                        if (!prev.HasValue) continue;
                        if (prev.Value.condition(Player))
                        {
                            return prev;
                        }
                    }
                }
            }
            
            return null;
        }

        #endregion
    }
}