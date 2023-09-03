using DBZGoatLib.Handlers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader.Config;

namespace DBZGoatLib
{
    public class DBZConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        public static DBZConfig Instance;

        [Header("$Mods.DBZGoatLib.Configs.Headers.Bar")]
        [LabelKey("$Mods.DBZGoatLib.Configs.KiBarX.Label")]
        [TooltipKey("$Mods.DBZGoatLib.Configs.KiBarX.Tooltip")]
        [DefaultValue(515f)]
        public float KiBarX { get; set; }


        [LabelKey("$Mods.DBZGoatLib.Configs.KiBarY.Label")]
        [TooltipKey("$Mods.DBZGoatLib.Configs.KiBarY.Tooltip")]
        [DefaultValue(49f)]
        public float KiBarY { get; set; }


        [LabelKey("$Mods.DBZGoatLib.Configs.ShowKi.Label")]
        [TooltipKey("$Mods.DBZGoatLib.Configs.ShowKi.Tooltip")]
        [DefaultValue(false)]
        public bool ShowKi;

        [LabelKey("$Mods.DBZGoatLib.Configs.UseNewKiBar.Label")]
        [TooltipKey("$Mods.DBZGoatLib.Configs.UseNewKiBar.Tooltip")]
        [DefaultValue(true)]
        public bool UseNewKiBar;

        [Header("$Mods.DBZGoatLib.Configs.Headers.Menu")]
        [LabelKey("$Mods.DBZGoatLib.Configs.TransMenuX.Label")]
        [TooltipKey("$Mods.DBZGoatLib.Configs.TransMenuX.Tooltip")]
        [DefaultValue(661f)]
        public float TransMenuX { get; set; }


        [LabelKey("$Mods.DBZGoatLib.Configs.TransMenuY.Label")]
        [TooltipKey("$Mods.DBZGoatLib.Configs.TransMenuY.Tooltip")]
        [DefaultValue(396f)]
        public float TransMenuY { get; set; }
        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
        {
            return true;
        }

        public override void OnChanged()
        {
            base.OnChanged();
            UIHandler.Dirty = true;
        }
        
    }
}
