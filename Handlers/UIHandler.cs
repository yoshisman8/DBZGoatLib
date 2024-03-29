﻿using DBZGoatLib.Model;
using DBZGoatLib.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace DBZGoatLib.Handlers
{
    public class UIHandler : ModSystem
    {
        internal TransformationMenu transformationMenu;
        internal KiBar kiBar;

        private UserInterface _transUserInterface;
        private UserInterface _kiUserInterface;
        public static List<TransformationPanel> Panels { get; private set; } = new();
        public static int ActivePanel = 0;
        public static bool Dirty;
        public static bool Loaded;

        internal static List<TransformationPanel> TruePanels => Panels.Where(x => x.Complete).ToList();

        /// <summary>
        /// Registers a new transformation panel.
        /// </summary>
        public static void RegisterPanel(TransformationPanel panel) => Panels.Add(panel);
        
        /// <summary>
        /// Unregister a transformation panel. 
        /// </summary>
        public static void UnregisterPanel(TransformationPanel panel) => Panels.Remove(panel);
        
        public static void TryChangePanel(string Name)
        {
            if (TruePanels.Any(x=>x.Name == Name))
            {
                int index = TruePanels.FindIndex(x => x.Name == Name);
                if (TruePanels[index].Predicate == null)
                    ActivePanel = index;
                else if (TruePanels[index].Predicate.Invoke(Main.CurrentPlayer))
                    ActivePanel = index;
                else
                    ActivePanel = 0;
            }
            else
            {
                ActivePanel = 0;
            }
        }
        public override void Load()
        {
            base.Load();

            //if(Main.netMode != NetmodeID.Server)
            //{
            //    transformationMenu = new TransformationMenu(Defaults.DefaultPanel);
            //    transformationMenu.Activate();

            //    _transUserInterface = new UserInterface();
            //    _transUserInterface.SetState(transformationMenu);

            //    kiBar = new KiBar();
            //    kiBar.Activate();

            //    _kiUserInterface = new UserInterface();
            //    _kiUserInterface.SetState(kiBar);
            //}
        }
        internal static void PrevTree()
        {
            if (ActivePanel - 1 < 0)
                ActivePanel = TruePanels.Count - 1;
            else
                ActivePanel--;
            Dirty = true;
        }
        internal static void NextTree()
        {
            if (ActivePanel + 1 >= TruePanels.Count)
                ActivePanel = 0;
            else
                ActivePanel++;
            Dirty = true;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            base.UpdateUI(gameTime);
            
            if (Dirty)
            {
                if (!TruePanels[ActivePanel].Predicate.Invoke(Main.CurrentPlayer))
                {
                    NextTree();
                    return;
                }

                if (Loaded)
                {
                    transformationMenu?.Remove();
                    transformationMenu = new TransformationMenu(TruePanels[ActivePanel]);
                    transformationMenu.Activate();

                    _transUserInterface = new UserInterface();
                    _transUserInterface.SetState(transformationMenu);

                    kiBar?.Remove();
                    kiBar = new KiBar();
                    kiBar.Activate();

                    _kiUserInterface = new UserInterface();
                    _kiUserInterface.SetState(kiBar);
                }
                
                Dirty = false;
            }

            if(TransformationMenu.Visible)
                _transUserInterface?.Update(gameTime);

            _kiUserInterface?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int menuIndex = layers.FindIndex(layer => layer.Name.Contains("Resource Bars"));
            if (menuIndex != -1)
            {
                layers.Insert(menuIndex, 
                    new LegacyGameInterfaceLayer(
                        "DBZGoatLib: TransformationMenu", 
                        delegate 
                        { 
                            _transUserInterface?.Draw(Main.spriteBatch, new GameTime()); 
                            return true; 
                        }, 
                    InterfaceScaleType.UI)
                );
            }
            int kiBarIndex = layers.FindIndex(a => a.Name.Equals("DBZMODPORT: Ki Bar"));
            if(kiBarIndex != -1)
            {
                layers[kiBarIndex] = new LegacyGameInterfaceLayer("DBZGoatLib: KiBar",
                    delegate
                    {
                        _kiUserInterface?.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                InterfaceScaleType.UI);
            }
        }
    }
}
