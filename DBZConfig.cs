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
    [Label("Client Settings")]
    public class DBZConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        public static DBZConfig Instance;

        [Header("Ki Bar Position")]
        [Label("Position X")]
        [DefaultValue(515f)]
        [Tooltip("The X position of the Ki Bar.")]
        public float KiBarX { get; set; }

        [Label("Position Y")]
        [DefaultValue(49f)]
        [Tooltip("The Y position of the Ki Bar.")]
        public float KiBarY { get; set; }

        [Label("Show ki value underneath bar")]
        [DefaultValue(false)]
        [Tooltip("Constantly show the Ki values underneath the Ki Bar.")]
        public bool ShowKi;

        [Label("Use new Ki Bar")]
        [DefaultValue(true)]
        [Tooltip("Use the new Ki Bar sprites. Cannot be disabled if Dragon Ball Terraria is not installed.")]
        public bool UseNewKiBar;

        [Header("Transformation Menu Position")]
        [Label("Position X")]
        [Tooltip("The X position of the Transformation Menu.")]
        [DefaultValue(661f)]
        public float TransMenuX { get; set; }

        [Label("Position Y")]
        [Tooltip("The Y position of the Transformation Menu.")]
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
