using System.Collections.Generic;

using Microsoft.Xna.Framework;

using ReLogic.Utilities;

using Terraria;
using Terraria.Audio;

namespace DBZGoatLib.Handlers {

    public static class SoundHandler {

        public static uint invalidSlot {
            get {
                return (uint)SlotId.Invalid.ToFloat();
            }
        }

        public static KeyValuePair<uint, ActiveSound> PlaySound(
            string soundId,
            Player player = null,
            float volume = 1f,
            float pitchVariance = 0.0f) {
            Vector2 location = player != null ? player.Center : Vector2.Zero;
            return PlaySound(soundId, location, volume, pitchVariance);
        }

        public static KeyValuePair<uint, ActiveSound> PlaySound(
            string soundId,
            Vector2 location,
            float volume = 1f,
            float pitchVariance = 0.0f) {
            if (Main.dedServ)
                return new KeyValuePair<uint, ActiveSound>(invalidSlot, null);
            SlotId invalid = SlotId.Invalid;
            SoundStyle customStyle = GetCustomStyle(soundId, volume, pitchVariance);
            ActiveSound activeSound = (ActiveSound)null;
            SlotId slotId = !location.Equals(Vector2.Zero) ? SoundEngine.PlaySound(customStyle, new Vector2?(location)) : SoundEngine.PlaySound(customStyle, new Vector2?());
            SoundEngine.TryGetActiveSound(slotId, out activeSound);
            return new KeyValuePair<uint, ActiveSound>(slotId.Value, activeSound);
        }

        public static void PlaySound(SlotId slotId) {
            ActiveSound activeSound;
            SoundEngine.TryGetActiveSound(slotId, out activeSound);
            if (activeSound.IsPlaying)
                return;
            activeSound.Resume();
        }

        public static SoundStyle GetCustomStyle(
            string soundId,
            float volume = 1f,
            float pitchVariance = 0.0f) {
            SoundStyle customStyle = new SoundStyle(soundId, (SoundType)0);
            customStyle.Volume = volume;
            customStyle.PitchVariance = pitchVariance;
            return customStyle;
        }

        public static KeyValuePair<uint, ActiveSound> KillTrackedSound(
            KeyValuePair<uint, ActiveSound> soundInfo) {
            ActiveSound activeSound = soundInfo.Value;
            if (activeSound != null)
                activeSound.Stop();
            else
                soundInfo.Value?.Stop();
            return new KeyValuePair<uint, ActiveSound>(invalidSlot, (ActiveSound)null);
        }

        public static void KillOtherPlayerAudio(Player myPlayer) {
            for (int index = 0; index < Main.player.Length; ++index) {
                Player player = Main.player[index];
                if (((Entity)player).whoAmI == index && ((Entity)player).whoAmI != Main.myPlayer) {
                    GPlayer modPlayer = player.GetModPlayer<GPlayer>();
                    modPlayer.auraSoundInfo = KillTrackedSound(modPlayer.auraSoundInfo);
                }
            }
            myPlayer.GetModPlayer<GPlayer>().playerIndexWithLocalAudio = -1;
        }

        public static bool CanPlayOtherPlayerAudio(GPlayer myPlayer, Player otherPlayer) => myPlayer.playerIndexWithLocalAudio == otherPlayer.whoAmI || myPlayer.playerIndexWithLocalAudio == -1;

        public static bool ShouldPlayPlayerAudio(Player player, bool isTransformation) {
            GPlayer modPlayer1 = player.GetModPlayer<GPlayer>();
            bool flag;
            if (player.whoAmI == Main.myPlayer) {
                flag = modPlayer1.auraSoundInfo.Value == null | isTransformation;
                if (modPlayer1.playerIndexWithLocalAudio != -1)
                    KillOtherPlayerAudio(player);
            } else {
                GPlayer modPlayer2 = Main.LocalPlayer.GetModPlayer<GPlayer>();
                flag = modPlayer2.auraSoundInfo.Value == null && CanPlayOtherPlayerAudio(modPlayer2, player);
                if (flag)
                    modPlayer2.playerIndexWithLocalAudio = ((Entity)player).whoAmI;
            }
            return flag;
        }

        public static void UpdateTrackedSound(
          KeyValuePair<uint, ActiveSound> soundInfo,
          Vector2 position) {
            ActiveSound activeSound = soundInfo.Value;
            if (activeSound == null)
                return;
            activeSound.Position = new Vector2?(position);
            activeSound.Update();
        }
    }
}