using DBZGoatLib.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace DBZGoatLib.Model
{
    public static class Defaults
    {
        public static Node[] DefaultNodes =
        {
            new Node(0, 2, "SSJ1Buff", "DBZMODPORT/UI/Buttons/SSJ1ButtonImage", "Only through failure with a powerful foe will true power awaken.", SSJ1Condition, (Player p) => true),
            new Node(1, 2, "SSJ2Buff", "DBZMODPORT/UI/Buttons/SSJ2ButtonImage", "One may awaken their true power through extreme pressure while ascended.", SSJ2Condition, SSJLineDiscovered),
            new Node(2, 2, "SSJ3Buff", "DBZMODPORT/UI/Buttons/SSJ3ButtonImage", "The power of an ancient foe may be the key to unlocking greater power.", SSJ3Condition, SSJLineDiscovered),
            new Node(3, 2, "SSJGBuff", "DBZMODPORT/UI/Buttons/SSJGButtonImage", "The godlike power of the lunar star could awaken something beyond mortal comprehension.", SSJGCondition, SSJLineDiscovered),
            new Node(1, 3, "LSSJBuff", "DBZMODPORT/UI/Buttons/LSSJButtonImage", "The rarest saiyans may be able to achieve a form beyond anything a normal saiyan could obtain.", LSSJ1Condition, LSSJLineDiscovered),
            new Node(2, 3, "LSSJ2Buff", "DBZMODPORT/UI/Buttons/LSSJ2ButtonImage", "A legendary saiyan sometimes may lose complete control upon being pushed into a critical state.", LSSJ2Condition, LSSJLineDiscovered),
            new Node(3, 3, "LSSJ3Buff", "DBZMODPORT/UI/Buttons/LSSJ3ButtonImage", "People of an occult nature hold the secret to this form.", LSSJ3Condition, LSSJLineDiscovered),
            new Node(4, 2, "SSJBBuff", "DBZMODPORT/UI/Buttons/SSJBButtonImage", "The experience from battling a galactic being could bring forth this strength.", SSJBCondition, SSJBRDiscovered),
            new Node(4, 1, "SSJRBuff", "DBZMODPORT/UI/Buttons/SSJRButtonImage", "The experience from battling a galactic being could bring forth this strength.", SSJRCondition, SSJBRDiscovered)
        };

        public static Connection[] Connections =
        {
            new Connection(0, 2, 2, false, new Gradient(Color.Yellow)),
            new Connection(0, 2, 1, true, new Gradient(Color.Yellow).AddStop(1f,Color.White)),
            new Connection(0, 3, 1, false, new Gradient(Color.White).AddStop(0.25f, Color.LightGreen)),
            new Connection(1, 3, 2, false, new Gradient(Color.LightGreen)),
            new Connection(2, 2, 1, false, new Gradient(Color.Yellow).AddStop(1f, Color.Red)),
            new Connection(3, 2, 1, false, new Gradient(Color.Red).AddStop(0.75f, Color.LightBlue)),
            new Connection(4, 1, 1, true, new Gradient(Color.Purple).AddStop(0.75f, Color.LightBlue))
        };

        public static TransformationPanel DefaultPanel = new TransformationPanel("Dragon Ball Terraria", true, DefaultNodes, Connections, (p) => { return true; });

        public static List<TransformationChain> Chains = new List<TransformationChain>()
        {
            new TransformationChain("SSJ1Buff", true, "ASSJBuff"),
            new TransformationChain("ASSJBuff", true, "USSJBuff", "SSJ1Buff"),
            new TransformationChain("USSJBuff", true, null, "ASSJBuff"),
            new TransformationChain("SSJ1Buff", false, "SSJ2Buff"),
            new TransformationChain("SSJ1Buff", false, "LSSJBuff"),
            new TransformationChain("SSJ2Buff", false, "SSJ3Buff", "SSJ1Buff"),
            new TransformationChain("SSJ3Buff", false, "SSJGBuff", "SSJ2Buff"),
            new TransformationChain("LSSJBuff", false, "LSSJ2Buff","SSJ1Buff"),
            new TransformationChain("LSSJ2Buff", false, "LSSJ3Buff", "LSSJBuff"),
            new TransformationChain("LSSJ3Buff", false, null, "LSSJ2Buff"),
            new TransformationChain("SSJGBuff", false, null, "SSJ3Buff"),
            new TransformationChain("SSJRBuff", false, null, "SSJGBuff"),
            new TransformationChain("SSJBBuff", false, null, "SSJGBuff")
        };

        public static bool SSJ1Condition(Player player)
        {
            dynamic modPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x=>x.Name.Equals("MyPlayer")).GetMethod("ModPlayer").Invoke(null, new object[] { player });

            return (bool)modPlayer.ssj1Achieved;
        }
        public static bool SSJ2Condition(Player player)
        {
            dynamic modPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetMethod("ModPlayer").Invoke(null, new object[] { player });

            return (bool)modPlayer.ssj2Achieved && !(bool)modPlayer.IsPlayerLegendary();
        }
        public static bool SSJ3Condition(Player player)
        {
            dynamic modPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetMethod("ModPlayer").Invoke(null, new object[] { player });

            return (bool)modPlayer.ssj3Achieved && !(bool)modPlayer.IsPlayerLegendary();
        }
        public static bool SSJGCondition(Player player)
        {
            dynamic modPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetMethod("ModPlayer").Invoke(null, new object[] { player });

            return (bool)modPlayer.ssjgAchieved && !(bool)modPlayer.IsPlayerLegendary();
        }
        public static bool SSJLineDiscovered(Player player)
        {
            dynamic modPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetMethod("ModPlayer").Invoke(null, new object[] { player });

            return (bool)modPlayer.ssj1Achieved && !(bool)modPlayer.IsPlayerLegendary();
        }
        public static bool SSJBRDiscovered(Player player)
        {
            dynamic modPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetMethod("ModPlayer").Invoke(null, new object[] { player });

            return (bool)modPlayer.ssjgAchieved && !(bool)modPlayer.IsPlayerLegendary();
        }
        public static bool LSSJLineDiscovered(Player player)
        {
            dynamic modPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetMethod("ModPlayer").Invoke(null, new object[] { player });

            return (bool)modPlayer.ssj1Achieved && (bool)modPlayer.IsPlayerLegendary();
        }
        public static bool LSSJ1Condition(Player player)
        {
            dynamic modPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetMethod("ModPlayer").Invoke(null, new object[] { player });

            return (bool)modPlayer.lssjAchieved && (bool)modPlayer.IsPlayerLegendary();
        }
        public static bool LSSJ2Condition(Player player)
        {
            dynamic modPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetMethod("ModPlayer").Invoke(null, new object[] { player });

            return (bool)modPlayer.lssj2Achieved && (bool)modPlayer.IsPlayerLegendary();
        }
        public static bool LSSJ3Condition(Player player)
        {
            dynamic modPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetMethod("ModPlayer").Invoke(null, new object[] { player });

            return (bool)modPlayer.lssj3Achieved && (bool)modPlayer.IsPlayerLegendary();
        }
        public static bool SSJBCondition(Player player)
        {
            dynamic modPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetMethod("ModPlayer").Invoke(null, new object[] { player });

            return (bool)modPlayer.ssjbAchieved && !(bool)modPlayer.IsPlayerLegendary();
        }
        public static bool SSJRCondition(Player player)
        {
            dynamic modPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer")).GetMethod("ModPlayer").Invoke(null, new object[] { player });

            return (bool)modPlayer.ssjrAchieved && !(bool)modPlayer.IsPlayerLegendary();
        }

        public static float GetMastery(Player player, string buffName)
        {
            var modPlayerClass = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));
            var field = modPlayerClass.GetField(MasteryPaths[buffName]);
            dynamic modPlayer = modPlayerClass.GetMethod("ModPlayer").Invoke(null, new object[] { player });

            return (float)field.GetValue(modPlayer);
        }
        

        public static Dictionary<string, string> MasteryPaths = new()
        {
            { "SSJ1Buff", "masteryLevel1" },
            { "SSJ2Buff", "masteryLevel2" },
            { "SSJ3Buff", "masteryLevel3" },
            { "SSJGBuff", "masteryLevelGod" },
            { "SSJBBuff", "masteryLevelBlue" },
            { "SSJRBuff", "masteryLevelRose" },
            { "LSSJBuff", "masteryLevelLeg" },
            { "LSSJ2Buff", "masteryLevelLeg2" },
            { "LSSJ3Buff", "masteryLevelLeg3" }
        };
        public static Dictionary<string,string> FormNames  = new()
        {
            { "SSJ1Buff", "Super Saiyan" },
            { "SuperKaiokenBuff", "Super Kaio-ken"},
            { "SSJ2Buff", "Super Saiyan 2" },
            { "SSJ3Buff", "Super Saiyan 3" },
            { "ASSJBuff", "Ascended Super Saiyan"},
            { "USSJBuff", "Ultra Super Saiyan" },
            { "SSJGBuff", "Super Saiyan God" },
            { "SSJBBuff", "Super Saiyan Blue" },
            { "SSJRBuff", "Super Saiyan Rosé" },
            { "LSSJBuff", "Legendary Super Saiyan" },
            { "LSSJ2Buff", "Legendary Super Saiyan 2" },
            { "LSSJ3Buff", "Legendary Super Saiyan 3" }
        };
        public static Dictionary<string, int> FormIDs = new()
        {
            { "None", 0 },
            { "SSJ1Buff", 1 },
            { "SSJ2Buff", 2 },
            { "SSJ3Buff", 3 },
            { "LSSJBuff", 4 },
            { "SSJGBuff", 5 },
            { "LSSJ2Buff", 6 },
            { "LSSJ3Buff", 7 },
            { "SSJBBuff", 8 },
            { "SSJRBuff", 9 },
        };
        public static Dictionary<string, string> FormPaths = new()
        {
            { "SSJ1Buff", "SSJ1" },
            { "SuperKaiokenBuff", "superKaioken" },
            { "ASSJBuff", "Assj" },
            { "USSJBuff", "Ussj" },
            { "SSJ2Buff", "SSJ2" },
            { "SSJ3Buff", "SSJ3" },
            { "LSSJBuff", "LSSJ" },
            { "SSJGBuff", "SSJG" },
            { "LSSJ2Buff", "LSSJ2" },
            { "LSSJ3Buff", "LSSJ3" },
            { "SSJBBuff", "SSJB" },
            { "SSJRBuff", "SSJR" },
        };

        public static Dictionary<string, Color> FormColors = new()
        {
            { "SSJ1Buff", Color.Yellow },
            { "SuperKaiokenBuff", Color.Red },
            { "ASSJBuff", Color.Yellow },
            { "USSJBuff", Color.Yellow },
            { "SSJ2Buff", Color.Yellow },
            { "SSJ3Buff", Color.Yellow },
            { "LSSJBuff", Color.LightGreen },
            { "SSJGBuff", Color.Red },
            { "LSSJ2Buff", Color.LightGreen },
            { "LSSJ3Buff", Color.LightGreen },
            { "SSJBBuff", new Color(15, 121, 219) },
            { "SSJRBuff", new Color(217, 39, 78) },
        };

        public static Gradient GetFormKiBar(string buffKeyName)
        {
            if (buffKeyName == "LSSJBuff" || buffKeyName == "LSSJ2Buff" || buffKeyName == "LSSJ3Buff")
            {
                var g = new Gradient(new Color(104, 237, 121));
                g.AddStop(1f, new Color(4, 184, 27));
                return g;
            }
            else if (buffKeyName == "SSJGBuff" || buffKeyName == "SuperKaiokenBuff")
            {
                var g = new Gradient(new Color(219, 83, 83));
                g.AddStop(1f, new Color(245, 0, 0));
                return g;
            }
            else if (buffKeyName == "SSJBBuff")
            {
                var g = new Gradient(new Color(104, 110, 222));
                g.AddStop(1f, new Color(0, 13, 255));
                return g;
            }
            else if (buffKeyName == "SSJRBuff")
            {
                var g = new Gradient(new Color(240, 86, 183));
                g.AddStop(1f, new Color(255, 0, 128));
                return g;
            }
            else
            {
                var g = new Gradient(new Color(245, 224, 91));
                g.AddStop(1f, new Color(240, 207, 0));
                return g;
            }
        }

        public static TraitInfo[] DBT_Traits =
        {
            new TraitInfo("Legendary", 0.05f, new Gradient(new Color(221, byte.MaxValue, 28)).AddStop(1f, new Color(70, 150, 93)), Legendary, UnTrait),
            new TraitInfo("Prodigy", 0.15f, new Gradient(new Color(0, 104, 249)).AddStop(1f, new Color(7, 28, 76)), Prodigy, UnTrait)
        };

        private static void Legendary(Player player)
        {
            var MyPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));

            var playerTrait = MyPlayer.GetField("playerTrait");

            var modPlayer = MyPlayer.GetMethod("ModPlayer").Invoke(null,new object[] { player });

            playerTrait.SetValue(modPlayer, "Legendary");
        }
        private static void UnTrait(Player player)
        {
            var MyPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));

            var playerTrait = MyPlayer.GetField("playerTrait");

            var modPlayer = MyPlayer.GetMethod("ModPlayer").Invoke(null, new object[] { player });

            playerTrait.SetValue(modPlayer, "");

        }
        private static void Prodigy(Player player)
        {
            var MyPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));

            var playerTrait = MyPlayer.GetField("playerTrait");

            var modPlayer = MyPlayer.GetMethod("ModPlayer").Invoke(null, new object[] { player });

            playerTrait.SetValue(modPlayer, "Prodigy");

            
        }

        public static void DoGeneticWish_Detour()
        {
            var MyPlayer = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));

            var playerTrait = MyPlayer.GetField("playerTrait");

            var wishActive = MyPlayer.GetField("wishActive");

            dynamic modPlayer = MyPlayer.GetMethod("ModPlayer").Invoke(null, new object[] { Main.LocalPlayer });

            playerTrait.SetValue(modPlayer, "");

            GPlayer player = Main.LocalPlayer.GetModPlayer<GPlayer>();

            player.RerollTraits();

            var WishMenu = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("WishMenu"));

            var menuVisible = WishMenu.GetField("menuVisible");

            menuVisible.SetValue(null, false);

            wishActive.SetValue(modPlayer, false);
        }
    }
}
