using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DBZGoatLib.Model;
using DBZGoatLib.Network;
using DBZGoatLib.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DBZGoatLib.Handlers
{
    public sealed class TransformationHandler
    {
        internal static readonly List<string> DBTForms =
        [
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
        ];

        private static TransformationInfo[] DBT_Transformation_Info
        {
            get
            {
                if (dbtTransformInfo == null)
                {
                    dbtTransformInfo = new TransformationInfo[DBTForms.Count];

                    for (int i = 0; i < DBTForms.Count; i++)
                    {
                        string buffName = DBTForms[i];
                        ModBuff buff = DBZGoatLib.DBZMOD.Value.mod.Find<ModBuff>(buffName);

                        TransformationInfo ti = new TransformationInfo(
                            buff.Type,
                            buffName,
                            false,
                            Defaults.FormNames[buffName],
                            Defaults.FormColors[buffName],
                            p => DBTFormCheck(buffName, p),
                            null,
                            null,
                            new AnimationData(),
                            Defaults.GetFormKiBar(buffName));

                        dbtTransformInfo[i] = ti;
                    }
                }

                return dbtTransformInfo;
            }
        }

        private static TransformationInfo[] dbtTransformInfo;

        private static bool DBTFormCheck(string BuffName, Player player)
        {
            dynamic modPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"))
                .GetMethod("ModPlayer").Invoke(null,
                    [player]);

            if (BuffName == "SSJ2Buff")
                return !(bool)modPlayer.IsPlayerLegendary();
            if (BuffName == "LSSJBuff")
                return (bool)modPlayer.lssjAchieved && (bool)modPlayer.IsPlayerLegendary();
            return true;
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
        public static List<TransformationInfo> Transformations { get; private set; } = [];

        /// <summary>
        /// List of all registered form chains. Can be added to.
        /// </summary>
        public static List<TransformationChain> TransformationChains { get; private set; } = [];

        /// <summary>
        /// Register a new transformation into the handler.
        /// </summary>
        public static void RegisterTransformation(TransformationInfo transformation) =>
            Transformations.Add(transformation);

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
        public static void RegisterTransformationChains(List<TransformationChain> chain) =>
            TransformationChains.AddRange(chain);

        /// <summary>
        /// Unregisters a transformation chain. Be careful when removing vanilla transformation chains!
        /// </summary>
        /// <param name="chain"></param>
        public static void UnregisterTransformationChain(TransformationChain chain)
        {
            int index = TransformationChains.FindIndex(x =>
                x.TransformationBuffKeyName == chain.TransformationBuffKeyName);
            if (index > -1)
                TransformationChains.RemoveAt(index);
        }

        /// <summary>
        /// Gets a transformation by its Buff ID.
        /// Cannot return DBT Transformation info.
        /// Returns null when the transformation cannot be found.
        /// </summary>
        /// <param name="buffid">Buff ID</param>
        /// <returns></returns>
        public static TransformationInfo? GetTransformation(int buffid)
        {
            if (Transformations.All(x => x.buffID != buffid))
                return null;
            return Transformations.First(x => x.buffID == buffid);
        }

        /// <summary>
        /// Gets a transformation by its Buff class name. 
        /// Can fetch DBT transformations.
        /// Returns null when the transformation cannot be found.
        /// </summary>
        /// <param name="buffName">Buff class name</param>
        /// <returns></returns>
        public static TransformationInfo? GetTransformation(string buffName)
        {
            if (DBTForms.Contains(buffName))
            {
                return DBT_Transformation_Info.First(x => x.buffKeyName == buffName);
            }

            if (Transformations.Any(x => x.buffKeyName == buffName))
                return Transformations.First(x => x.buffKeyName == buffName);
            return null;
        }

        /// <summary>
        /// Starts a transformation on the user
        /// </summary>
        /// <param name="player">Player to be transformed.</param>
        /// <param name="transformation">Transformation Info</param>
        public static void Transform(Player player, TransformationInfo transformation)
        {
            if (DBTForms.Contains(transformation.buffKeyName))
            {
                ExternalTransform(player, transformation);
                return;
            }

            if (player.HasBuff(transformation.buffID))
            {
                return;
            }

            if (!transformation.condition(player))
            {
                return;
            }

            if (IsTransformed(player, true) && !transformation.stackable)
            {
                ClearNonstackables(player);
            }

            StartTransformation(player, transformation);
        }

        private static void ExternalTransform(Player player, TransformationInfo form)
        {
            TypeInfo tHandler =
                DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("TransformationHelper"));
            dynamic buffInfo = tHandler.GetProperty(Defaults.FormPaths[form.buffKeyName]).GetValue(null);
            MethodInfo canTransform = tHandler.GetMethod("CanTransform", [ 
                typeof(Player),
                DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("BuffInfo")).AsType()
            ]);
            MethodInfo doTransform = tHandler.GetMethod("DoTransform");

            if (!(bool)canTransform.Invoke(null, [player, buffInfo]))
                return;
            doTransform.Invoke(null, [player, buffInfo, DBZGoatLib.DBZMOD.Value.mod]);
        }

        /// <summary>
        /// End all transformations, including DBT's
        /// </summary>
        public static void ClearTransformations(Player player)
        {
            TypeInfo helper =
                DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("TransformationHelper"));

            helper.GetMethod("EndTransformations").Invoke(null, [player]);

            SoundHandler.PlaySound("DBZMODPORT/Sounds/PowerDown", player, 0.3f);

            foreach (TransformationInfo transformation in Transformations)
            {
                EndTranformation(player, transformation);
            }
        }

        public static void ClearNonstackables(Player player)
        {
            TypeInfo helper =
                DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("TransformationHelper"));

            helper.GetMethod("EndTransformations").Invoke(null, [player]);

            SoundHandler.PlaySound("DBZMODPORT/Sounds/PowerDown", player, 0.3f);

            foreach (TransformationInfo transformation in Transformations.Where(x => !x.stackable))
            {
                EndTranformation(player, transformation);
            }
        }

        /// <summary>
        /// End a transformation from this mod.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="buffId"></param>
        public static void EndTranformation(Player player, TransformationInfo transformation)
        {
            if (player.HasBuff(transformation.buffID))
            {
                player.ClearBuff(transformation.buffID);
                
                transformation.postTransform(player);

                if (Main.dedServ || Main.netMode != NetmodeID.MultiplayerClient || player.whoAmI != Main.myPlayer)
                    return;
                NetworkHelper.transSync.SendFormChanges(256, player.whoAmI, player.whoAmI, transformation.buffID, 0);
            }
        }

        internal static bool IsKaioken(Player player)
        {
            TypeInfo tHandler =
                DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("TransformationHelper"));
            MethodInfo isKaioken = tHandler.GetMethod("IsKaioken", [typeof(Player)]);

            return (bool)isKaioken.Invoke(null, [player]);
        }

        private static void StartTransformation(Player player, TransformationInfo transformation)
        {
            player.AddBuff(transformation.buffID, 666666, false);
            if (!string.IsNullOrEmpty(transformation.transformationText))
                CombatText.NewText(player.Hitbox, transformation.tranformtionColor, transformation.transformationText,
                    false, false);
            if (!Main.dedServ && Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer)
                NetworkHelper.transSync.SendFormChanges(256, player.whoAmI, player.whoAmI, transformation.buffID,
                    666666);
            transformation.onTransform(player);
        }

        /// <summary>
        /// Checks whether the user is transformed or not.
        /// </summary>
        public static bool IsTransformed(Player player, bool IgnoreStackable = false)
        {
            foreach (TransformationInfo trans in Transformations)
            {
                if (player.HasBuff(trans.buffID))
                {
                    if (IgnoreStackable && trans.stackable)
                    {
                        continue;
                    }

                    return true;
                }
            }
            
            foreach (TransformationInfo ext in DBT_Transformation_Info)
            {
                if (player.HasBuff(ext.buffID))
                {
                    return true;
                }
            }
            
            return false;
        }

        private static bool doOnce = true;
        /// <summary>
        /// Gets the current transformation the player has. Returns null if none is found.
        /// DOES NOT CHECK FOR STACKABLE FORMS.
        /// </summary>
        public static TransformationInfo? GetCurrentTransformation(Player player)
        {
            foreach (TransformationInfo transformation in Transformations.Where(x => !x.stackable))
            {
                if (player.HasBuff(transformation.buffID))
                {
                    return transformation;
                }
            }

            foreach (TransformationInfo transformation in DBT_Transformation_Info.Where(x => !x.stackable))
            {   
                if (player.HasBuff(transformation.buffID))
                {
                    return transformation;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the current STACKABLE transformation the player has. Returns null if none is found
        /// </summary>
        public static TransformationInfo? GetCurrentStackedTransformation(Player player)
        {
            foreach (TransformationInfo transformation in Transformations.Where(x => x.stackable))
            {
                if (player.HasBuff(transformation.buffID))
                    return transformation;
            }

            return null;
        }

        /// <summary>
        /// Gets ALL current transformations the player has, including stacked ones. Returns an empty array if the user is not transformed.
        /// </summary>
        public static TransformationInfo[] GetAllCurrentForms(Player player)
        {
            List<TransformationInfo> list = [];

            foreach (TransformationInfo transformation in Transformations)
            {
                if (player.HasBuff(transformation.buffID))
                    list.Add(transformation);
            }

            foreach (TransformationInfo external in DBT_Transformation_Info)
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
        public static bool IsAnythingBut(Player player, int buffId, bool includeExternal = false)
        {
            foreach (TransformationInfo trans in Transformations)
            {
                if (trans.stackable)
                    continue;
                if (player.HasBuff(trans.buffID) && trans.buffID != buffId)
                    return true;
            }

            if (ModLoader.HasMod("DBZMODPORT") && includeExternal)
            {
                foreach (TransformationInfo ext in DBT_Transformation_Info)
                {
                    if (player.HasBuff(ext.buffID) && ext.buffID != buffId && !ext.stackable)
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
        /// <returns>Array of matching transformation chains. Array is empty if non are found.</returns>
        public static TransformationChain[] GetChain(TransformationInfo transformation, bool Charge = false)
        {
            List<TransformationChain> chains = [];
            foreach (TransformationChain c in TransformationChains)
                if (c.TransformationBuffKeyName == transformation.buffKeyName && c.Charging == Charge)
                    chains.Add(c);
            return chains.ToArray();
        }
    }
}