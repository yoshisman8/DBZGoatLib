﻿using DBZGoatLib.Handlers;
using DBZGoatLib.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using DBZGoatLib.UI.Components;

namespace DBZGoatLib.UI
{
    public class KiBar : UIState
    {
        private static Gradient BarGradient;
        private static Gradient TransformedGradient;
        private static Gradient DefaultColor;
        private static Color KiBlobColor;

        private KiBarContainer Container;
        private AnimatedImage KiBarFrame;
        private KiResourceBar Bar;
        private AnimatedImage Sparks;
        private UnderSparks UnderSparks;
        private KiTextElement KiValues;
        private KiBlobComponent KiBlob;

        private bool Hovering;
        private List<float> CleanAverageKi = [];
        public static float AverageKi = 0;
        public static int MaxKi = 1;

        internal static Gradient GetColor()
        {
            Player player = Main.CurrentPlayer;

            if (TransformationHandler.IsTransformed(player))
            {
                return GetTransformationColor() ?? GetTraitColor() ?? DefaultColor;
            }

            return GetTraitColor() ?? DefaultColor;
        }
        internal static Gradient GetTraitColor()
        {
            if (DBZConfig.Instance.UseNewKiBar)
                return null;

            TraitInfo? trait = TraitHandler.GetTraitByName(Main.CurrentPlayer.GetModPlayer<GPlayer>().Trait);
            if (trait.HasValue)
                if (trait.Value.Color != null)
                    return trait.Value.Color;
                else return null;
            return null;
        }
        internal static Color GetBlobColor()
        {
            TraitInfo? trait = TraitHandler.GetTraitByName(Main.CurrentPlayer.GetModPlayer<GPlayer>().Trait);
            if (trait.HasValue)
                if (trait.Value.Color != null)
                    return trait.Value.Color.GetColor(0f);
                else return DefaultColor.GetColor(0f);
            return DefaultColor.GetColor(0f);
        }
        internal static Gradient GetTransformationColor()
        {
            TransformationInfo[] transForm = TransformationHandler.GetAllCurrentForms(Main.CurrentPlayer);
            if (transForm.Any(x => x.KiBarGradient != null))
                return transForm.First(x => x.KiBarGradient != null).KiBarGradient;
            return null;
        }

        public override void OnInitialize()
        {
            base.OnInitialize();

            Gradient def = new(new Color(86, 238, 242));
            def.AddStop(1f, new Color(53, 146, 183));

            DefaultColor = def;
            KiBlobColor = def.GetColor(0f);

            Container = new KiBarContainer(ModContent.Request<Texture2D>("DBZGoatLib/Assets/UI/KiBarBackground"));
            Container.Width.Set(DBZConfig.Instance.UseNewKiBar ? 159 : 136, 0f);
            Container.Height.Set(24, 0f);
            Container.OnMouseOver += (o, e) => { Hovering = true; };
            Container.OnMouseOut += (o, e) => { Hovering = false; };

            if (DBZConfig.Instance.UseNewKiBar || !ModLoader.HasMod("DBZMODPORT"))
            {
                UnderSparks = new(ModContent.Request<Texture2D>("DBZGoatLib/Assets/UI/Form-Under"), 4);
                UnderSparks.Left.Set(0, 0f);
                UnderSparks.Top.Set(4, 0f);
                UnderSparks.Width.Set(136, 0f);
                UnderSparks.Height.Set(96, 0f);
                UnderSparks.IgnoresMouseInteraction = true;
                Container.Append(UnderSparks);

                Bar = new KiResourceBar();
                Bar.Left.Set(22, 0f);
                Bar.Top.Set(10, 0f);
                Bar.Height.Set(4, 0f);
                Bar.Width.Set(114, 0f); // 90 true size
                Bar.IgnoresMouseInteraction = true;
                Container.Append(Bar);


                KiBarFrame = new(ModContent.Request<Texture2D>("DBZGoatLib/Assets/UI/KiBarForeground"), 4);
                KiBarFrame.Left.Set(0, 0f);
                KiBarFrame.Top.Set(0, 0f);
                KiBarFrame.Width.Set(159, 0f);
                KiBarFrame.Height.Set(96, 0f);
                KiBarFrame.IgnoresMouseInteraction = true;
                Container.Append(KiBarFrame);

                KiBlob = new(ModContent.Request<Texture2D>("DBZGoatLib/Assets/UI/KiBarBlob"), 4);
                KiBlob.Left.Set(0, 0f);
                KiBlob.Top.Set(0, 0f);
                KiBlob.Width.Set(159, 0f);
                KiBlob.Height.Set(96, 0f);
                KiBlob.IgnoresMouseInteraction = true;
                Container.Append(KiBlob);
                

                Sparks = new(ModContent.Request<Texture2D>("DBZGoatLib/Assets/UI/Form-Over"), 4);
                Sparks.Left.Set(0, 0f);
                Sparks.Top.Set(4, 0f);
                Sparks.Width.Set(136, 0f);
                Sparks.Height.Set(96, 0f);
                Sparks.IgnoresMouseInteraction = true;
            }
            else
            { 
                Bar = new KiResourceBar();
                Bar.Left.Set(36, 0f);
                Bar.Top.Set(6, 0f);
                Bar.Height.Set(6, 0f);
                Bar.Width.Set(90, 0f); // 90 true size
                Bar.IgnoresMouseInteraction = true;
                Container.Append(Bar);

                KiBarFrame = new(ModContent.Request<Texture2D>("DBZMODPORT/UI/KiBar"), 4);
                KiBarFrame.Left.Set(0, 0f);
                KiBarFrame.Top.Set(0, 0f);
                KiBarFrame.Width.Set(136, 0f);
                KiBarFrame.Height.Set(96, 0f);
                KiBarFrame.IgnoresMouseInteraction = true;
                Container.Append(KiBarFrame);

                Sparks = new(ModContent.Request<Texture2D>("DBZMODPORT/UI/KiBarLightning"), 3);
                Sparks.Left.Set(8, 0f);
                Sparks.Top.Set(-1, 0f);
                Sparks.Width.Set(130, 0f);
                Sparks.Height.Set(60, 0f);
                Sparks.IgnoresMouseInteraction = true;
            }

            KiValues = new("0/0");
            KiValues.Left.Set(20, 0f);
            KiValues.Top.Set(25, 0f);
            KiValues.Width.Set(136, 0f);
            Container.Append(KiValues);

            Append(Container);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Hovering && !DBZConfig.Instance.ShowKi)
                Main.instance.MouseText($"{(int)AverageKi}/{MaxKi}");
            TypeInfo playerClass = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));
            dynamic modPlayer = playerClass.GetMethod("ModPlayer").Invoke(null, [Main.CurrentPlayer]);
            int maxKi = (int)playerClass.GetMethod("OverallKiMax").Invoke(modPlayer, null);
            float currentKi = (float)playerClass.GetMethod("GetKi").Invoke(modPlayer, null);

            CleanAverageKi.Add(currentKi);
            if (CleanAverageKi.Count > 15)
                CleanAverageKi.RemoveRange(0, CleanAverageKi.Count - 15);
            AverageKi = CleanAverageKi.Sum() / 15f;

            MaxKi = maxKi;
        }
        protected override void DrawChildren(SpriteBatch spriteBatch)
        {
            if (TransformationHandler.IsTransformed(Main.CurrentPlayer, true))
            {
                Container.Append(Sparks);
            }
            else
            {
                if (Container.Children.Contains(Sparks))
                    Container.RemoveChild(Sparks);
            }

            base.DrawChildren(spriteBatch);
        }
    }
}
