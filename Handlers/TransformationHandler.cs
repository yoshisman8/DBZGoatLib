using System;
using System.Collections.Generic;
using System.Linq;

using DBZGoatLib.Model;
using DBZGoatLib.Network;
using DBZGoatLib.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DBZGoatLib.Handlers {

    public sealed class TransformationHandler {

        internal static readonly List<string> DBTForms = new() {
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

        private static TransformationInfo[] DBT_Transformation_Info { get
            {
                List<TransformationInfo> list = new();
                foreach (var buffName in DBTForms)
                    list.Add(new TransformationInfo(DBZGoatLib.DBZMOD.Value.mod.Find<ModBuff>(buffName).Type, buffName, false, Defaults.FormNames[buffName], Defaults.FormColors[buffName], null, null, null, new AnimationData(),Defaults.GetFormKiBar(buffName)));
                return list.ToArray();
            }
        }
        
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
        /// List of all registered form chains. Can be added to.
        /// </summary>
        public static List<TransformationChain> TransformationChains { get; private set; } = new List<TransformationChain>();

        /// <summary>
        /// Register a new transformation into the handler.
        /// </summary>
        public static void RegisterTransformation(TransformationInfo transformation) => Transformations.Add(transformation);

        /// <summary>
        /// Unregisters as transformation from the handler.
        /// </summary>
        /// <param name="transformation"></param>
        public static void UnregisterTransformation(TransformationInfo transformation) 
        {
            int index = Transformations.FindIndex(x => x.buffKeyName == transformation.buffKeyName);
            if (index > -1) 
                Transformations.RemoveAt(index);
        }
        
        /// <summary>
        /// Registers a new batch of transformation chains. Not required for basic functionality.
        /// </summary>
        /// <param name="chain"></param>
        public static void RegisterTransformationChains(List<TransformationChain> chain) => TransformationChains.AddRange(chain);

        /// <summary>
        /// Unregisters a transformation chain. Be careful when removing vanilla transformation chains!
        /// </summary>
        /// <param name="chain"></param>
        public static void UnregisterTransformationChain(TransformationChain chain)
        {
            int index = TransformationChains.FindIndex(x => x.TransformationBuffKeyName == chain.TransformationBuffKeyName);
            if(index > -1)
                TransformationChains.RemoveAt(index);
        }

        /// <summary>
        /// Gets a transformation by its Buff ID.
        /// Cannot return DBT Transformation info.
        /// </summary>
        /// <param name="buffid">Buff ID</param>
        /// <returns></returns>
        public static TransformationInfo GetTransformation(int buffid) => Transformations.First(x => x.buffID == buffid);

        /// <summary>
        /// Gets a transformation by its Buff class name. 
        /// Can fetch DBT transformations.
        /// </summary>
        /// <param name="buffName">Buff class name</param>
        /// <returns></returns>
        public static TransformationInfo GetTransformation(string buffName)
        {
            if (DBTForms.Contains(buffName))
            {
                return DBT_Transformation_Info.First(x => x.buffKeyName == buffName);
            }
            else return Transformations.First(x => x.buffKeyName == buffName);
        }

        /// <summary>
        /// Starts a transformation on the user
        /// </summary>
        /// <param name="player">Player to be transformed.</param>
        /// <param name="transformation">Transformation Info</param>
        public static void Transform(Player player, TransformationInfo transformation) {

            if (DBTForms.Contains(transformation.buffKeyName))
            {
                ExternalTransform(player, transformation);
                return;
            }

            if (player.HasBuff(transformation.buffID))
                return;
            if (!transformation.condition(player))
                return;

            if (transformation.KiBarGradient != null)
                KiBar.SetTransformationColor(transformation.KiBarGradient);
            ClearTransformations(player);
            StartTransformation(player, transformation);
        }

        private static void ExternalTransform(Player player, TransformationInfo form)
        {
            var tHandler = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("TransformationHelper"));
            dynamic buffInfo = tHandler.GetProperty(Defaults.FormPaths[form.buffKeyName]).GetValue(null);
            var canTransform = tHandler.GetMethod("CanTransform", new Type[] { typeof(Player), DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("BuffInfo")).AsType() });
            var doTransform = tHandler.GetMethod("DoTransform");

            if (!(bool)canTransform.Invoke(null, new object[] { player, buffInfo }))
                return;
            if (form.KiBarGradient != null)
                KiBar.SetTransformationColor(form.KiBarGradient);
            doTransform.Invoke(null, new object[] {player, buffInfo, DBZGoatLib.DBZMOD.Value.mod });
        }
        /// <summary>
        /// End all transformations, including DBT's
        /// </summary>
        public static void ClearTransformations(Player player) {
            var helper = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("TransformationHelper"));

            helper.GetMethod("EndTransformations").Invoke(null, new object[] { player });

            SoundHandler.PlaySound("DBZMODPORT/Sounds/PowerDown", player, 0.3f);

            if(player.whoAmI == Main.myPlayer)
                KiBar.ResetTransformationColor();

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
                KiBar.ResetTransformationColor();
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
            foreach (var ext in DBT_Transformation_Info)
            {
                if (player.HasBuff(ext.buffID))
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
            foreach (var transformation in DBT_Transformation_Info.Where(x => !x.stackable))
            {
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
            foreach (var external in DBT_Transformation_Info)
            {
                if (player.HasBuff(external.buffID))
                    list.Add(external);
            }
            return list.ToArray();
        }

        /// <summary>
        /// Checks whether the provided player has a transformation other than the one passed. Useful for checking for overlapps!
        /// Returns true if an overlap was found. 
        /// Does not check for Stackable forms.
        /// </summary>
        public static bool IsAnythingBut(Player player, int buffId, bool includeExternal = false) {
            foreach (var trans in Transformations) {
                if (player.HasBuff(trans.buffID) && trans.buffID != buffId && !trans.stackable)
                    return true;
            }

            if (ModLoader.HasMod("DBZMODPORT") && includeExternal) {
                foreach (var ext in DBT_Transformation_Info) {
                    if (player.HasBuff(ext.buffID) && ext.buffID != buffId && !ext.stackable)
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

        /// <summary>
        /// Attemts to find the first registered transformation chain for this transformation.
        /// </summary>
        /// <param name="transformation">Transformation Info to search for.</param>
        /// <param name="Charge">Whether to search for chains which require the charge keybind to be held.</param>
        /// <returns>Transformation Chain, or null if none is found.</returns>
        public static TransformationChain? GetChain(TransformationInfo transformation, bool Charge = false)
        {
            if (!TransformationChains.Any(x => x.TransformationBuffKeyName == transformation.buffKeyName && x.Charging == Charge))
                return null;
            else return TransformationChains.First(x => x.TransformationBuffKeyName == transformation.buffKeyName && x.Charging == Charge);
        }
    }
}