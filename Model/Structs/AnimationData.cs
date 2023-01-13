using System;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria.ModLoader;

namespace DBZGoatLib.Model {

    public readonly struct AnimationData {
        public readonly AuraData Aura;
        public readonly bool Sparks;
        public readonly string HairPath;
        public readonly SoundData Sound;

        public AnimationData(
            AuraData _auraData,
            bool _sparks,
            SoundData _soundData,
            string _HairPath) {
            Sparks = _sparks;
            Sound = _soundData;
            Aura = _auraData;
            HairPath = _HairPath;
        }

        [Obsolete("Please use AnimationData(AuraData, bool, SoundData, string)")]
        public AnimationData(AuraData _auraData, bool _sparks, SoundData _soundData) {
            Sparks = _sparks;
            Sound = _soundData;
            Aura = _auraData;
            HairPath = string.Empty;
        }

        public static bool operator ==(AnimationData a1, AnimationData a2) {
            return a1.Sparks == a2.Sparks &&
                a1.HairPath == a2.HairPath &&
                a1.Sound == a2.Sound &&
                a1.Aura == a2.Aura;
        }

        public static bool operator !=(AnimationData a1, AnimationData a2) {
            return !(a1.Sparks == a2.Sparks &&
                a1.HairPath == a2.HairPath &&
                a1.Sound == a2.Sound &&
                a1.Aura == a2.Aura);
        }

        public override bool Equals(object obj) {
            if (obj is not AnimationData || obj == null) return false;

            var a1 = (AnimationData)obj;

            return a1.Sparks == Sparks &&
                a1.HairPath == HairPath &&
                a1.Sound == Sound &&
                a1.Aura == Aura;
        }

        public override int GetHashCode() {
            return (Aura.GetHashCode() + HairPath.GetHashCode() + Sound.GetHashCode() + Sparks.ToString()).GetHashCode();
        }
    }

    public readonly struct AuraData {
        public readonly string AuraPath;
        public readonly int Frames;
        public readonly BlendState BlendState;
        public readonly Color Color;

        /// <summary>
        ///
        /// </summary>
        /// <param name="_Path">Path to the aura visuals. Leave empty to use generic aura.</param>
        /// <param name="_Frames">The number of frames this aura has.</param>
        /// <param name="_blendState">If using generic aura, set to AlphaBlend.</param>
        /// <param name="_Color">If not using generic aura, set to White.</param>
        /// <param name="_HairPath">Leave blank if not you do not wish to replace the head texture.</param>
        public AuraData(
            string _Path,
            int _Frames,
            BlendState _blendState,
            Color _Color) {
            AuraPath = _Path;
            Frames = _Frames;
            BlendState = _blendState;
            Color = _Color;
        }

        [Obsolete("Please use AuraData(string, int, BlendState, Color)")]
        public AuraData(string _Path, int _Frames, int _FrameTimerLimit, BlendState _blendState, Color _Color, string _HairPath) {
            AuraPath = _Path;
            Frames = _Frames;
            BlendState = _blendState;
            Color = _Color;
        }

        public Texture2D GetTexture() => ModContent.Request<Texture2D>(string.IsNullOrEmpty(AuraPath) ? "DBZGoatLib/Assets/BaseAura" : AuraPath, AssetRequestMode.AsyncLoad).Value;

        public int GetHeight() => GetTexture().Height / Frames;

        public int GetWidth() => GetTexture().Width;

        public int GetAuraOffsetY(GPlayer modPlayer, float scale) => (int)(0.0 - ((double)(GetHeight() / 2) * scale - modPlayer.Player.height * 0.600000023841858));

        public Tuple<float, Vector2> GetAuraRotationAndPosition(GPlayer modPlayer, float auraScale) {
            bool flag = (double)Math.Abs(modPlayer.Player.velocity.X) <= 6.0 && (double)Math.Abs(modPlayer.Player.velocity.Y) <= 6.0;
            Vector2 zero = Vector2.Zero;
            int auraOffsetY = GetAuraOffsetY(modPlayer, auraScale);
            float num1;
            Vector2 vector2;

            dynamic DBZModPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetMethod("ModPlayer").Invoke(null, new object[] { modPlayer.Player });

            if (DBZModPlayer.isFlying) {
                int num2 = (int)Math.Floor((modPlayer.Player).height * 0.75);
                double num3 = modPlayer.Player.fullRotation <= 0.0 ? 3.14159274101257 : -3.14159274101257;
                num1 = modPlayer.Player.fullRotation + (float)num3;
                double num4 = modPlayer.Player.width / 4;
                double num5 = modPlayer.Player.height / 4;
                double num6 = GetWidth() / 4;
                double num7 = GetHeight() / 4;
                double num8 = num4 + auraOffsetY + num2;
                double num9 = num6 - num8;
                double num10 = num7 - (num5 + auraOffsetY + num2);
                double num11 = num9 * Math.Cos(modPlayer.Player.fullRotation);
                double num12 = Math.Sin(modPlayer.Player.fullRotation);
                double num13 = num10 * num12;
                vector2 = modPlayer.Player.Center + new Vector2((float)(0.0 - num13), (float)num11);
            } else {
                vector2 = modPlayer.Player.Center + new Vector2(0.0f, auraOffsetY);
                num1 = 0.0f;
            }
            return new Tuple<float, Vector2>(num1, vector2);
        }

        public static bool operator ==(AuraData a1, AuraData a2) {
            return a1.AuraPath == a2.AuraPath &&
                a1.Frames == a2.Frames &&
                a1.BlendState == a2.BlendState;
        }

        public static bool operator !=(AuraData a1, AuraData a2) {
            return !(a1.AuraPath == a2.AuraPath &&
                a1.Frames == a2.Frames &&
                a1.BlendState == a2.BlendState);
        }

        public override bool Equals(object obj) {
            if (obj is not AuraData || obj == null) return false;

            var a1 = (AuraData)obj;

            return a1.AuraPath == AuraPath &&
                a1.Frames == Frames &&
                a1.BlendState == BlendState;
        }

        public override int GetHashCode() {
            return (AuraPath + Frames.ToString() + BlendState.GetHashCode()).GetHashCode();
        }
    }

    public readonly struct SoundData {
        public readonly string StartAudioPath;
        public readonly string LoopAudioPath;
        public readonly int LoopSoundDuration;

        public SoundData(
            string _StartAudioPath,
            string _LoopAudioPath,
            int _LoopDuration) {
            StartAudioPath = _StartAudioPath;
            LoopAudioPath = _LoopAudioPath;
            LoopSoundDuration = _LoopDuration;
        }

        public static bool operator ==(SoundData a1, SoundData a2) {
            return a1.StartAudioPath == a2.StartAudioPath &&
                a1.LoopAudioPath == a2.LoopAudioPath &&
                a1.LoopSoundDuration == a2.LoopSoundDuration;
        }

        public static bool operator !=(SoundData a1, SoundData a2) {
            return !(a1.StartAudioPath == a2.StartAudioPath &&
                a1.LoopAudioPath == a2.LoopAudioPath &&
                a1.LoopSoundDuration == a2.LoopSoundDuration);
        }

        public override bool Equals(object obj) {
            if (obj is not SoundData || obj == null) return false;

            var a1 = (SoundData)obj;

            return a1.StartAudioPath == StartAudioPath &&
                a1.LoopAudioPath == LoopAudioPath &&
                a1.LoopSoundDuration == LoopSoundDuration;
        }

        public override int GetHashCode() {
            return (StartAudioPath + LoopAudioPath + LoopSoundDuration.ToString()).GetHashCode();
        }
    }
}