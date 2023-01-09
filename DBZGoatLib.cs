using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DBZGoatLib.Handlers;
using DBZGoatLib.Model;
using DBZGoatLib.Network;
using DBZGoatLib.UI;
using MonoMod.RuntimeDetour;
using Terraria.ModLoader;
using Terraria.UI;

namespace DBZGoatLib
{

    public class DBZGoatLib : Mod
    {
        public const BindingFlags flagsAll = BindingFlags.Public | BindingFlags.NonPublic
        | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetField
        | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;

        internal List<Detour> Detours = new List<Detour>();
        internal List<Hook> Hooks = new List<Hook>();

        public static readonly Lazy<Mod> Instance = new(() => ModLoader.GetMod("DBZGoatLib"));
        public static readonly Lazy<(bool loaded, Mod mod)> DBZMOD = new(() => (ModLoader.TryGetMod("DBZMODPORT", out Mod dbz), dbz));
        // public static readonly Lazy<(bool loaded, Mod mod)> DBCAMOD = new(() => (ModLoader.TryGetMod("dbzcalamity", out Mod dbca), dbca));

        public static ModKeybind OpenMenu;
        public override void Load()
        {
            if (DBZMOD.Value.loaded)
            {
                MonoModHooks.RequestNativeAccess();

                var MyPlayer = DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));

                TransformationHandler.TransformKey = (ModKeybind)MyPlayer.GetField("transform").GetValue(null);
                TransformationHandler.PowerDownKey = (ModKeybind)MyPlayer.GetField("powerDown").GetValue(null);
                TransformationHandler.EnergyChargeKey = (ModKeybind)MyPlayer.GetField("energyCharge").GetValue(null);
                OpenMenu = (ModKeybind)MyPlayer.GetField("transMenu").GetValue(null);

                UIHandler.RegisterPanel(Defaults.DefaultPanel);
                TransformationHandler.RegisterTransformationChains(Defaults.Chains);

                foreach (var trait in Defaults.DBT_Traits)
                    TraitHandler.RegisterTrait(trait);

                AddDetour(MyPlayer.AsType(), "HandleTransformations");
                AddDetour(MyPlayer.AsType(), "HandleKiDrainMasteryContribution");
                AddDetour(MyPlayer.AsType(), "HandleDamageReceivedMastery");

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

            var detour = new Detour(method, to.GetMethod(toName, flagsAll));

            detour.Apply();

            Detours.Add(detour);
        }
        public void AddDetour(Type type, string name, Type[] args, Type to, string toName)
        {
            Logger.Info($"type {type.FullName}   name {name}   args {{{string.Join(",", args.Select(a => a.FullName))}}}   to {to.FullName}   toName {toName}");

            Detours.Add(new Detour(type.GetMethod(name, args), to.GetMethod(toName, flagsAll)));
        }
        public void AddDetour(Type type, string name) =>
            Detours.Add(new Detour(type.GetMethod(name, flagsAll), GetType().GetMethod("Nothing", flagsAll)));
        public void Nothing() { }
        #endregion
    }
}