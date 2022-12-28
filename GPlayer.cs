using DBZGoatLib.Handlers;
using DBZGoatLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace DBZGoatLib
{
    public class GPlayer : ModPlayer
    {
        public int lightningFrameTimer;
        public int lightning3FrameCount = 9;
        public int lightning3FrameIndex;
        public int lightning3FrameTime;
        public int lightning3FrameTimer = 6;

        public Dictionary<int, float> Masteries = new Dictionary<int, float>();
        public Dictionary<int, bool> MasteryMaxed = new Dictionary<int, bool>();

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


        public override void SaveData(TagCompound tag)
        {
            foreach (var trans in TransformationHandler.Transformations)
            {
                if(Masteries.TryGetValue(trans.buffID, out float mastery) && !tag.ContainsKey($"DBZGoatLib_{trans.buffKeyName}"))
                {
                    tag.Add($"DBZGoatLib_{trans.buffKeyName}", mastery);
                }
                if (MasteryMaxed.TryGetValue(trans.buffID, out bool maxed) && !tag.ContainsKey($"DBZGoatLib_{trans.buffKeyName}_Maxed"))
                {
                    tag.Add($"DBZGoatLib_{trans.buffKeyName}_Maxed", maxed);
                }
            }
        }
        public override void LoadData(TagCompound tag)
        {
            foreach (var trans in TransformationHandler.Transformations)
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
        }
        public override void OnEnterWorld(Player player)
        {
            if (player.whoAmI != Player.whoAmI)
                return;

            foreach (var trans in TransformationHandler.Transformations)
            {
                if (!Masteries.ContainsKey(trans.buffID))
                    Masteries.Add(trans.buffID, 0f);
                if (!MasteryMaxed.ContainsKey(trans.buffID))
                    MasteryMaxed.Add(trans.buffID, false);
            }
        }
        public override void PlayerDisconnect(Player player)
        {
            TransformationHandler.ClearTransformations(player);
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (TransformationHandler.IsTransformed(drawInfo.drawPlayer))
            {
                var pClass = DBZGoatLib.DBZMOD.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));
                var modPlayer = pClass.GetMethod("ModPlayer").Invoke(null, new object[] { drawInfo.drawPlayer });

                pClass.GetField("isCharging").SetValue(modPlayer, false);
            }
        }

        public override void PreUpdateMovement()
        {

            if (TransformationHandler.IsTransformed(Player))
            {
                currentAnimation = TransformationHandler.GetCurrentTransformation(Player).Value.animationData;

                if (!currentAnimation.Equals(previousAnimation))
                {
                    auraSoundInfo = SoundHandler.KillTrackedSound(auraSoundInfo);
                    HandleAuraStartupSound(currentAnimation, false);
                    auraSoundtimer = 0;
                    auraFrameTimer = 0;
                }
            }
            else
            {
                SoundHandler.KillTrackedSound(auraSoundInfo);
                currentAnimation = new AnimationData();
            }
            previousAnimation = currentAnimation;
            HandleAuraLoopSound(currentAnimation);
            IncrementAuraFrameTimers(currentAnimation.Aura);
        }
        public static GPlayer ModPlayer(Player player) => player.GetModPlayer<GPlayer>();

        public float GetMastery(int BuffId)
        {
            if (Masteries.ContainsKey(BuffId))
                return Masteries[BuffId];
            else return 0f;
        }

        public void HandleAuraLoopSound(AnimationData data)
        {
            if (data.Sound.Equals(new SoundData()))
                return;
            if (data.Sound.LoopSoundDuration <= 0 || string.IsNullOrEmpty(data.Sound.LoopAudioPath))
                return;
            if(SoundHandler.ShouldPlayPlayerAudio(Player, true))
            {
                if (auraSoundtimer == 0)
                    auraSoundInfo = SoundHandler.PlaySound(data.Sound.LoopAudioPath, Player, 0.7f);
                auraSoundtimer++;
                if (auraSoundtimer >= data.Sound.LoopSoundDuration)
                    auraSoundtimer = 0;
            }
            SoundHandler.UpdateTrackedSound(auraSoundInfo, Player.position);
        }
        public void HandleAuraStartupSound(AnimationData data, bool isCharging)
        {
            if (data.Equals(new AnimationData()))
                return;
            if (data.Sound.Equals(new SoundData()))
                return;
            if (string.IsNullOrEmpty(data.Sound.StartAudioPath))
                return;
            SoundHandler.PlaySound(data.Sound.StartAudioPath, Player, 0.7f, 0.1f);
        }
        public void IncrementAuraFrameTimers(AuraData aura)
        {
            if (aura.Equals(new AuraData()))
                return;
            dynamic modPlayer = DBZGoatLib.DBZMOD.Code.DefinedTypes.First(x=>x.Name.Equals("MyPlayer")).GetMethod("ModPlayer").Invoke(null, new object[] { Player });
            if (modPlayer.isCharging)
                ++this.auraFrameTimer;
            ++this.auraFrameTimer;
            if (this.auraFrameTimer >= 3)
            {
                this.auraFrameTimer = 0;
                ++this.auraCurrentFrame;
            }
            if (this.auraCurrentFrame < aura.Frames)
                return;
            this.auraCurrentFrame = 0;
        }
        public override void PostUpdate()
        {

            if (TransformationHandler.IsTransformed(Player))
            {
                if (TransformationHandler.GetCurrentTransformation(Player).Value.animationData.Sparks)
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
                if((DateTime.Now - LastMasteryTick.Value).TotalSeconds >= 1)
                {
                    LastMasteryTick = DateTime.Now;
                    HandleMasteryGain(TransformationHandler.GetCurrentTransformation(Player).Value);
                }

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
                else
                {
                    var myPlayer = DBZGoatLib.DBZMOD.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));
                    var instance = myPlayer.GetMethod("ModPlayer").Invoke(null, new object[] { Player });
                    bool prodigy = (bool)myPlayer.GetMethod("IsProdigy").Invoke(instance, null);
                    Masteries[transformation.buffID] = Math.Min(1f, value + (0.00232f * (prodigy ? 2f : 1f)));
                }
            }
        }
        public override void OnHitAnything(float x, float y, Entity victim)
        {
            if (victim is NPC)
                if ((victim as NPC).type == NPCID.TargetDummy)
                    return;
            if (!TransformationHandler.IsTransformed(Player))
                return;
            if(!LastHitEnemy.HasValue)
                LastHitEnemy = DateTime.Now;

            if ((DateTime.Now - LastHitEnemy.Value).TotalMilliseconds < 500)
                return;
            LastHitEnemy = DateTime.Now;

            var transformation = TransformationHandler.GetCurrentTransformation(Player).Value;
            HandleMasteryGain(transformation);
        }

        public override void OnHitByNPC(NPC npc, int damage, bool crit)
        {
            if (!TransformationHandler.IsTransformed(Player))
                return;

            if (!LastHit.HasValue)
                LastHit = DateTime.Now;

            if ((DateTime.Now - LastHit.Value).TotalMilliseconds < 500)
                return;
            LastHit = DateTime.Now;

            var transformation = TransformationHandler.GetCurrentTransformation(Player).Value;

            HandleMasteryGain(transformation);
        }
        public override void OnHitByProjectile(Projectile proj, int damage, bool crit)
        {
            if (!TransformationHandler.IsTransformed(Player))
                return;

            if (!LastHit.HasValue)
                LastHit = DateTime.Now;

            if ((DateTime.Now - LastHit.Value).TotalMilliseconds < 500)
                return;
            LastHit = DateTime.Now;

            var transformation = TransformationHandler.GetCurrentTransformation(Player).Value;

            HandleMasteryGain(transformation);
        }
    }
}
