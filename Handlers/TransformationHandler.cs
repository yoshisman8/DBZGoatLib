using System.Collections.Generic;
using System.Linq;

using DBZGoatLib.Model;
using DBZGoatLib.Network;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DBZGoatLib.Handlers {

    public sealed class TransformationHandler {

        private static readonly List<string> DBTForms = new() {
            "SSJ1Buff",
            "ASSJBuff",
            "USSJBuff",
            "SuperKaiokenBuff",
            "SSJ2Buff",
            "SSJ3Buff",
            "SSJGBuff",
            "SSJBBuff",
            "SSJRBuff",
            "LSSJBuff",
            "LSSJ2Buff",
            "LSSJ3Buff"
        };

        private static readonly List<string> DBCAForms = new() {
            "UIBuff",
            "UISignBuff",
            "UEBuff"
        };

        /// <summary>
        /// The Transform keybind registered by DBT.
        /// </summary>
        public static ModKeybind TransformKey = null!;

        /// <summary>
        /// The Power down keybind registered by DBT.
        /// </summary>
        public static ModKeybind PowerDownKey = null!;

        /// <summary>
        /// The Energy Charge keybind registered by DBT.
        /// </summary>
        public static ModKeybind EnergyChargeKey = null!;

        /// <summary>
        /// List of all Transformations. Can be added to.
        /// </summary>
        public static List<TransformationInfo> Transformations { get; private set; } = new List<TransformationInfo>();

        /// <summary>
        /// Register a new transformation into the handler.
        /// </summary>
        public static void RegisterTransformation(TransformationInfo transformation) => Transformations.Add(transformation);

        /// <summary>
        /// Unregisters as transformation from the handler.
        /// </summary>
        /// <param name="transformation"></param>
        public static void UnregisterTransformation(TransformationInfo transformation) => Transformations.Remove(transformation);

        /// <summary>
        /// Gets a transformation by its Buff ID.
        /// </summary>
        /// <param name="buffid">Buff ID</param>
        /// <returns></returns>
        public static TransformationInfo GetTransformation(int buffid) => Transformations.First(x => x.buffID == buffid);

        /// <summary>
        /// Gets a transformation by its Buff class name.
        /// </summary>
        /// <param name="buffName">Buff class name</param>
        /// <returns></returns>
        public static TransformationInfo GetTransformation(string buffName) => Transformations.First(x => x.buffKeyName == buffName);

        /// <summary>
        /// Starts a transformation on the user
        /// </summary>
        /// <param name="player">Player to be transformed.</param>
        /// <param name="transformation">Transformation Info</param>
        public static void Transform(Player player, TransformationInfo transformation) {
            if (player.HasBuff(transformation.buffID))
                return;
            if (!transformation.condition(player))
                return;
            ClearTransformations(player);
            StartTransformation(player, transformation);
        }

        /// <summary>
        /// End all transformations, including DBT's
        /// </summary>
        public static void ClearTransformations(Player player) {
            var helper = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("TransformationHelper"));

            helper.GetMethod("EndTransformations").Invoke(null, new object[] { player });

            SoundHandler.PlaySound("DBZMODPORT/Sounds/PowerDown", player, 0.3f);

            foreach (var transformation in Transformations) {
                EndTranformation(player, transformation);
            }
        }

        /// <summary>
        /// End a transformation from this mod.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="buffId"></param>
        public static void EndTranformation(Player player, TransformationInfo transformation) {
            if (player.HasBuff(transformation.buffID)) {
                player.DelBuff(player.FindBuffIndex(transformation.buffID));

                transformation.postTransform(player);

                if (Main.dedServ || Main.netMode != NetmodeID.MultiplayerClient || player.whoAmI != Main.myPlayer)
                    return;
                NetworkHelper.transSync.SendFormChanges(256, player.whoAmI, player.whoAmI, transformation.buffID, 0);
            }
        }

        private static void StartTransformation(Player player, TransformationInfo transformation) {
            player.AddBuff(transformation.buffID, 666666, false);
            if (!string.IsNullOrEmpty(transformation.transformationText))
                CombatText.NewText(player.Hitbox, transformation.tranformtionColor, transformation.transformationText, false, false);
            if (!Main.dedServ && Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer)
                NetworkHelper.transSync.SendFormChanges(256, player.whoAmI, player.whoAmI, transformation.buffID, 666666);
            transformation.onTransform(player);
        }

        /// <summary>
        /// Checks whether the user is transformed or not.
        /// </summary>
        public static bool IsTransformed(Player player, bool IgnoreDBTtransformations = true, bool ignoreDBCATransformations = true) {
            foreach (var trans in Transformations) {
                if (player.HasBuff(trans.buffID))
                    return true;
            }
            if (!IgnoreDBTtransformations) {
                if (ModLoader.HasMod("DBZMODPORT")) {
                    foreach (var ext in DBTForms) {
                        if (player.HasBuff(DBZGoatLib.DBZMOD.Value.mod.Find<ModBuff>(ext).Type))
                            return true;
                    }
                }

                if (ModLoader.HasMod("dbzcalamity")) {
                    foreach (var ext in DBCAForms) {
                        if (player.HasBuff(DBZGoatLib.DBCAMOD.Value.mod.Find<ModBuff>(ext).Type))
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the current transformation the player has. Returns null if none is found.
        /// DOES NOT CHECK FOR STACKABLE FORMS.
        /// </summary>
        public static TransformationInfo? GetCurrentTransformation(Player player) {
            foreach (var transformation in Transformations.Where(x => !x.stackable)) {
                if (player.HasBuff(transformation.buffID))
                    return transformation;
            }
            return null;
        }

        /// <summary>
        /// Gets the current STACKABLE transformation the player has. Returns null if none is found
        /// </summary>
        public static TransformationInfo? GetCurrentStackedTransformation(Player player) {
            foreach (var transformation in Transformations.Where(x => x.stackable)) {
                if (player.HasBuff(transformation.buffID))
                    return transformation;
            }
            return null;
        }

        /// <summary>
        /// Gets ALL current transformations the player has, including stacked ones. Returns an empty array if the user is not transformed.
        /// </summary>
        public static TransformationInfo[] GetAllCurrentForms(Player player) {
            List<TransformationInfo> list = new();

            foreach (var transformation in Transformations) {
                if (player.HasBuff(transformation.buffID))
                    list.Add(transformation);
            }
            return list.ToArray();
        }

        /// <summary>
        /// Checks whether the provided player has a transformation other than the one passed. Useful for checking for overlapps!
        /// Returns true if an overlap was found.
        /// </summary>
        public static bool IsAnythingBut(Player player, int buffId, bool includeExternal = false) {
            foreach (var trans in Transformations) {
                if (player.HasBuff(trans.buffID) && trans.buffID != buffId)
                    return true;
            }

            if (ModLoader.HasMod("DBZMODPORT") && includeExternal) {
                foreach (var ext in DBTForms) {
                    int extType = DBZGoatLib.DBZMOD.Value.mod.Find<ModBuff>(ext).Type;
                    if (player.HasBuff(extType) && extType != buffId)
                        return true;
                }
            }

            if (ModLoader.HasMod("dbzcalamity") && includeExternal) {
                foreach (var ext in DBCAForms) {
                    int extType = DBZGoatLib.DBCAMOD.Value.mod.Find<ModBuff>(ext).Type;
                    if (player.HasBuff(extType) && extType != buffId)
                        return true;
                }
            }

            return false;
        }
    }
}