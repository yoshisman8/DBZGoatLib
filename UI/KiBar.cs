using DBZGoatLib.Handlers;
using DBZGoatLib.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace DBZGoatLib.UI
{
    public class KiBar : UIState
    {
        private static Gradient BarGradient;
        private static Gradient TransformedGradient;
        private static Gradient DefaultColor;

        private KiBarContainer Container;
        private AnimatedImage KiBarFrame;
        private KiResourceBar Bar;
        private AnimatedImage Sparks;

        private bool Hovering;
        private List<float> CleanAverageKi = new();
        public static float AverageKi = 0;
        public static int MaxKi = 1;
        /// <summary>
        /// Sets the ki bar's color to a new gradient.
        /// </summary>
        /// <param name="gradient">Gradient object.</param>
        public static void SetColor(Gradient gradient)
        {
            BarGradient = gradient;
        }

        public static void SetTransformationColor(Gradient gradient)
        {
            TransformedGradient = gradient;
        }

        /// <summary>
        /// Resets the Ki bar to its default color.
        /// </summary>
        public static void ResetTransformationColor()
        {
            TransformedGradient = null;
        }

        /// <summary>
        /// Resets the Ki bar to its default color.
        /// </summary>
        public static void ResetColor()
        {
            BarGradient = DefaultColor;
        }

        public static Gradient GetColor()
        {
            Player player = Main.CurrentPlayer;

            if (TransformationHandler.IsTransformed(player))
            {
                var transForm = TransformationHandler.GetAllCurrentForms(Main.CurrentPlayer);
                if (transForm.Any(x => x.KiBarGradient != null))
                    return transForm.First(x => x.KiBarGradient != null).KiBarGradient;
                else return DefaultColor;
            }
            var trait = TraitHandler.GetTraitByName(Main.CurrentPlayer.GetModPlayer<GPlayer>().Trait);
            if (trait.HasValue)
                if (trait.Value.Color != null)
                    return trait.Value.Color;
                else return DefaultColor;
            else return DefaultColor;
        }
        public override void OnInitialize()
        {
            base.OnInitialize();

            Gradient def = new(new Color(86, 238, 242));
            def.AddStop(1f, new Color(53, 146, 183));

            DefaultColor = def;

            Container = new KiBarContainer(TextureAssets.MagicPixel);
            Container.Width.Set(136, 0f);
            Container.Height.Set(24, 0f);
            Container.OnMouseOver += (o, e) => { Hovering = true; };
            Container.OnMouseOut += (o, e) => { Hovering = false; };

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
            Sparks.Left.Set(8  , 0f);
            Sparks.Top.Set(-1, 0f);
            Sparks.Width.Set(130, 0f);
            Sparks.Height.Set(60, 0f);
            Sparks.IgnoresMouseInteraction = true;

            Append(Container);
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Hovering)
                Main.instance.MouseText($"{(int)AverageKi}/{MaxKi}");
            var playerClass = DBZGoatLib.DBZMOD.Value.mod.Code.DefinedTypes.First(x => x.Name.Equals("MyPlayer"));
            dynamic modPlayer = playerClass.GetMethod("ModPlayer").Invoke(null, new object[] { Main.CurrentPlayer });
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
            if (TransformationHandler.IsTransformed(Main.CurrentPlayer))
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
