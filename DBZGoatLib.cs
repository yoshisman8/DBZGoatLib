using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using DBZGoatLib.Handlers;
using DBZGoatLib.Model;
using DBZGoatLib.Network;
using DBZGoatLib.UI;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace DBZGoatLib
{

    public class DBZGoatLib : Mod
    {
        public const BindingFlags flagsAll = BindingFlags.Public | BindingFlags.NonPublic
        | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetField
        | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;

        internal List<Hook> Detours = new List<Hook>();
        internal List<Hook> Hooks = new List<Hook>();
        public delegate void orig_HandleTransformations(ModPlayer self);
        public delegate void orig_HandleDamageReceivedMastery(ModPlayer self);
        public delegate void orig_HandleKiDrainMasteryContribution(ModPlayer self, float kiAmount, bool isWeaponDrain, bool isFormDrain);
        public delegate void hook_HandleTransformations(orig_HandleTransformations orig, ModPlayer self);
        public delegate void hook_HandleDamageReceivedMastery(orig_HandleDamageReceivedMastery orig, ModPlayer self);
        public delegate void hook_HandleKiDrainMasteryContribution(orig_HandleKiDrainMasteryContribution orig, ModPlayer self, float kiAmount, bool isWeaponDrain, bool isFormDrain);


        public static readonly Lazy<Mod> Instance = new(() => ModLoader.GetMod("DBZGoatLib"));
        public static readonly Lazy<(bool loaded, Mod mod)> DBZMOD = new(() => (ModLoader.TryGetMod("DBZMODPORT", out Mod dbz), dbz));
        // public static readonly Lazy<(bool loaded, Mod mod)> DBCAMOD = new(() => (ModLoader.TryGetMod("dbzcalamity", out Mod dbca), dbca));

        public static ModKeybind OpenMenu;
        public override void Load()
        {
            if (DBZMOD.Value.loaded)
            {

                var MyPlayer = DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));
                

                TransformationHandler.TransformKey = (ModKeybind)MyPlayer.GetField("transform").GetValue(null);
                TransformationHandler.PowerDownKey = (ModKeybind)MyPlayer.GetField("powerDown").GetValue(null);
                TransformationHandler.EnergyChargeKey = (ModKeybind)MyPlayer.GetField("energyCharge").GetValue(null);
                OpenMenu = (ModKeybind)MyPlayer.GetField("transMenu").GetValue(null);

                UIHandler.RegisterPanel(Defaults.DefaultPanel);
                TransformationHandler.RegisterTransformationChains(Defaults.Chains);

                foreach (var trait in Defaults.DBT_Traits)
                    TraitHandler.RegisterTrait(trait);

                MonoModHooks.Add(MyPlayer.AsType().GetMethod("HandleTransformations", flagsAll), (hook_HandleTransformations)MyDetour);
                MonoModHooks.Add(MyPlayer.AsType().GetMethod("HandleDamageReceivedMastery", flagsAll), (hook_HandleDamageReceivedMastery)MyDetour);
                MonoModHooks.Add(MyPlayer.AsType().GetMethod("HandleKiDrainMasteryContribution", flagsAll), (hook_HandleKiDrainMasteryContribution)MyDetour);

                var WishMenu = DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("WishMenu"));

                AddDetour(WishMenu.AsType(), "DoGeneticWish", true, typeof(Defaults), "DoGeneticWish_Detour");
            }
        }
        public override void Unload()
        {
            UIHandler.UnregisterPanel(Defaults.DefaultPanel);

            foreach (var c in Defaults.Chains)
                TransformationHandler.UnregisterTransformationChain(c);

            foreach (var trait in Defaults.DBT_Traits)
                TraitHandler.UnregisterTrait(trait);
        }

        public override void PostSetupContent()
        {
            base.PostSetupContent();
            foreach (var form in ModContent.GetContent<Transformation>())
                form.Load();
        }
        internal static void SaveConfig(DBZConfig cfg)
        {
            MethodInfo saveMethodInfo = typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic);
            if (saveMethodInfo != null)
            {
                saveMethodInfo.Invoke(null, new object[] { cfg });
                return;
            }
            Instance.Value.Logger.Warn("In-game SaveConfig failed, code update required.");
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI) => NetworkHelper.HandlePacket(reader, whoAmI);

        #region Hook/Detours
        public void AddHook(Type type, string name, Type to, string toName)
        {
            Logger.Info($"type {type.FullName}   name {name}   methodType Method   to {to.FullName}   toName {toName}");

            MethodInfo method;
            method = type.GetMethod(name, flagsAll);

            var hook = new Hook(method, to.GetMethod(toName, flagsAll));

            hook.Apply();

            Hooks.Add(hook);
        }
        public void AddHook(Type type, string name, Type[] args, Type to, string toName)
        {
            Logger.Info($"type {type.FullName}   name {name}   methodType Method   to {to.FullName}   toName {toName}");

            MethodInfo method;
            method = type.GetMethod(name, flagsAll, args);

            var hook = new Hook(method, to.GetMethod(toName, flagsAll));

            hook.Apply();

            Hooks.Add(hook);
        }
        public void AddDetour(Type type, string name, bool methodType, Type to, string toName)
        {
            Logger.Info($"type {type.FullName}   name {name}   methodType {(methodType ? "Method" : "Property")}   to {to.FullName}   toName {toName}");

            MethodInfo method;
            if (methodType)
            {
                method = type.GetMethod(name, flagsAll);
            }
            else
            {
                method = type.GetProperty(name, flagsAll).GetMethod;
            }

            var detour = new Hook(method, to.GetMethod(toName, flagsAll));

            detour.Apply();

            Detours.Add(detour);
        }
        public void AddDetour(Type type, string name, Type[] args, Type to, string toName)
        {
            Logger.Info($"type {type.FullName}   name {name}   args {{{string.Join(",", args.Select(a => a.FullName))}}}   to {to.FullName}   toName {toName}");

            Detours.Add(new Hook(type.GetMethod(name, args), to.GetMethod(toName, flagsAll)));
        }

        static void MyDetour(orig_HandleTransformations orig, ModPlayer self)
        {
        }
        static void MyDetour(orig_HandleDamageReceivedMastery orig, ModPlayer self)
        {
        }
        static void MyDetour(orig_HandleKiDrainMasteryContribution orig, ModPlayer self, float kiAmount, bool isWeaponDrain, bool isFormDrain)
        {
        }
        #endregion
    }
}