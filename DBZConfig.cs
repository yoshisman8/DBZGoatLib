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

        [Header("$Mods.DBZGoatLib.ConfigHeaders.KiBarPos")]
        [LabelKey("$Mods.DBZGoatLib.ConfigLabels.KiBarX")]
        [DefaultValue(515f)]
        [TooltipKey("$Mods.DBZGoatLib.ConfigToolTips.KiBarX")]
        public float KiBarX { get; set; }

        [LabelKey("$Mods.DBZGoatLib.ConfigLabels.KiBarY")]
        [DefaultValue(49f)]
        [TooltipKey("$Mods.DBZGoatLib.ConfigToolTips.KiBarY")]
        public float KiBarY { get; set; }

        [LabelKey("$Mods.DBZGoatLib.ConfigLabels.ShowKi")]
        [DefaultValue(false)]
        [TooltipKey("$Mods.DBZGoatLib.ConfigToolTips.ShowKi")]
        public bool ShowKi;

        [LabelKey("$Mods.DBZGoatLib.ConfigLabels.UseNewKiBar")]
        [DefaultValue(true)]
        [TooltipKey("$Mods.DBZGoatLib.ConfigToolTips.UseNewKiBar")]
        public bool UseNewKiBar;

        [Header("$Mods.DBZGoatLib.ConfigHeaders.TransMenuPos")]
        [LabelKey("$Mods.DBZGoatLib.ConfigLabels.TransMenuX")]
        [TooltipKey("$Mods.DBZGoatLib.ConfigToolTips.TransMenuX")]
        [DefaultValue(661f)]
        public float TransMenuX { get; set; }

        [LabelKey("$Mods.DBZGoatLib.ConfigLabels.TransMenuY")]
        [TooltipKey("$Mods.DBZGoatLib.ConfigToolTips.TransMenuX")]
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
