using System;
using System.IO;
using System.Linq;

using DBZGoatLib.Handlers;
using DBZGoatLib.Network;

using Terraria.ModLoader;

namespace DBZGoatLib {

    public class DBZGoatLib : Mod {
        public static readonly Lazy<Mod> Instance = new(() => ModLoader.GetMod("DBZGoatLib"));
        public static readonly Lazy<(bool loaded, Mod mod)> DBZMOD = new(() => (ModLoader.TryGetMod("DBZMODPORT", out Mod dbz), dbz));
        public static readonly Lazy<(bool loaded, Mod mod)> DBCAMOD = new(() => (ModLoader.TryGetMod("dbzcalamity", out Mod dbca), dbca));

        public override void Load() {
            if (DBZMOD.Value.loaded) {
                var MyPlayer = DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));

                TransformationHandler.TransformKey = (ModKeybind)MyPlayer.GetField("transform").GetValue(null);
                TransformationHandler.PowerDownKey = (ModKeybind)MyPlayer.GetField("powerDown").GetValue(null);
                TransformationHandler.EnergyChargeKey = (ModKeybind)MyPlayer.GetField("energyCharge").GetValue(null);
            }
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI) => NetworkHelper.HandlePacket(reader, whoAmI);
    }
}