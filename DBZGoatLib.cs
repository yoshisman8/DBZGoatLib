using System.IO;
using Terraria.ModLoader;
using DBZGoatLib.Network;
using System.Linq;
using DBZGoatLib.Handlers;

namespace DBZGoatLib
{
	public class DBZGoatLib : Mod
	{
		public static DBZGoatLib Instance;
		public static Mod DBZMOD;
        public static Mod DBCAMOD;
        

        public override void Load()
        {
            Instance = this;

            if(ModLoader.TryGetMod("DBZMODPORT", out Mod dbz))
            {
                DBZMOD = dbz;

                var MyPlayer = DBZMOD.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));

                TransformationHandler.TransformKey = (ModKeybind)MyPlayer.GetField("transform").GetValue(null);
                TransformationHandler.PowerDownKey = (ModKeybind)MyPlayer.GetField("powerDown").GetValue(null);
                TransformationHandler.EnergyChargeKey = (ModKeybind)MyPlayer.GetField("energyCharge").GetValue(null);
            }
            if(ModLoader.TryGetMod("dbzcalamity", out Mod dbca))
            {
                DBCAMOD = dbca;
            }
        }
        
        public override void Unload()
        {
            Instance = null;
            DBZMOD = null;
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI) => NetworkHelper.HandlePacket(reader, whoAmI);
    }
}