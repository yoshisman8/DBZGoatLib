using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;
using DBZGoatLib.Model;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria;
using DBZGoatLib.Handlers;
using Terraria.GameContent;

namespace DBZGoatLib.UI
{
    public class TransformationMenu : UIState
    {
        public static TransformationPanel transformationPanel;

        public static string ActiveForm;
        public static string HoveredForm;
        public static bool Visible;
        public static bool Dirty;
        public static string Tooltip;

        public DragableUIPanel Panel;
        public UIElement Grid;
        public UIElement MasteryBar;
        public UIElement PrevTreeButton;
        public UIElement NextTreeButton;
        public List<TransConnector> Connections = new();
        public List<TransNode> Nodes = new();

        private Color GradientA = new Color(0,0, 184);
        private Color GradientB = new Color(96,248,248);

        public TransformationMenu(TransformationPanel _panel)
        {
            transformationPanel = _panel;
        }
        public override void OnInitialize()
        {
            BuildComponents();
            this.IgnoresMouseInteraction = false;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!string.IsNullOrEmpty(Tooltip))
                Main.instance.MouseText(Tooltip);

            if (Dirty)
            {
                Nodes = new();
                Connections = new();

                Dirty = false;
                BuildComponents();
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);
            DrawMasteryBar(spriteBatch);
            
        }

        private void DrawMasteryBar(SpriteBatch spriteBatch)
        {
            var Player = Main.CurrentPlayer.GetModPlayer<GPlayer>();

            if (string.IsNullOrEmpty(ActiveForm) && string.IsNullOrEmpty(HoveredForm))
                return;

            string form = string.IsNullOrEmpty(HoveredForm) ? ActiveForm : HoveredForm;

            if (string.IsNullOrEmpty(form))
                return;

            float Quotient = 0f;
            if (TransformationHandler.DBTForms.Contains(form))
                Quotient = (float)Player.GetMastery(form) / 1f;
            else
                Quotient = (float)Player.GetMastery(TransformationHandler.GetTransformation(form).buffID) / 1f;

            Quotient = Utils.Clamp(Quotient, 0f, 1f);

            Rectangle hitbox = MasteryBar.GetDimensions().ToRectangle();

            hitbox.X += 2;
            hitbox.Y += 2;
            hitbox.Width -= 4;
            hitbox.Height -= 4;

            int left = hitbox.Left;
            int right = hitbox.Right;
            int steps = (int)((right - left) * Quotient);
            for (int i = 0; i < steps; i++)
            {
                float percent = (float)i / (right - left);
                spriteBatch.Draw((Texture2D)TextureAssets.MagicPixel, new Rectangle(left + i, hitbox.Y, 1 , hitbox.Height), Color.Lerp(GradientA, GradientB, percent));
            }
        }
        private void MasteryBarMouseOver(UIMouseEvent evt, UIElement listeningElement)
        {
            if (string.IsNullOrEmpty(ActiveForm))
                return;
            else
            {
                if (Defaults.MasteryPaths.TryGetValue(ActiveForm, out string path))
                {
                    Tooltip = string.Format("{0:P2} Mastery", Defaults.GetMastery(Main.CurrentPlayer, ActiveForm));
                }
                else
                    Tooltip = string.Format("{0:P2} Mastery", Main.CurrentPlayer.GetModPlayer<GPlayer>().GetMastery(TransformationHandler.GetTransformation(ActiveForm).buffID));
            }
                
        }
        private void PrevTree(UIMouseEvent evt, UIElement listeningElement)
        {
            UIHandler.PrevTree();
        }
        private void NextTree(UIMouseEvent evt, UIElement listeningElement)
        {
            UIHandler.NextTree();
        }
        private void BuildComponents()
        {
            Panel = new TransformationPanelComponent(ModContent.Request<Texture2D>("DBZGoatLib/Assets/UI/TransformationUI"));
            Panel.SetPadding(0);
            Panel.Width.Set(360, 0f);
            Panel.Height.Set(296, 0f);
            Append(Panel);

            PrevTreeButton = new UIElement();
            PrevTreeButton.Width.Set(36, 0f);
            PrevTreeButton.Height.Set(36, 0f);
            PrevTreeButton.Left.Set(22, 0f);
            PrevTreeButton.Top.Set(8, 0f);
            PrevTreeButton.OnClick += PrevTree;
            PrevTreeButton.OnMouseOver += (o, e) => { Tooltip = "Previous Transformation Tree"; };
            PrevTreeButton.OnMouseOut += (o, e) => { Tooltip = null; };
            Panel.Append(PrevTreeButton);

            NextTreeButton = new UIElement();
            NextTreeButton.Width.Set(36, 0f);
            NextTreeButton.Height.Set(36, 0f);
            NextTreeButton.Left.Set(306, 0f);
            NextTreeButton.Top.Set(8, 0f);
            NextTreeButton.OnClick += NextTree;
            NextTreeButton.OnMouseOver += (o, e) => { Tooltip = "Next Transformation Tree"; };
            NextTreeButton.OnMouseOut += (o, e) => { Tooltip = null; };
            Panel.Append(NextTreeButton);

            var Title = new UIText(transformationPanel.Name);
            Title.Height.Set(24, 0f);
            Title.Width.Set(200, 0f);
            Title.Left.Set(82, 0f);
            Title.Top.Set(14, 0f);

            Panel.Append(Title);

            
            MasteryBar = new UIElement();
            MasteryBar.Left.Set(10, 0f);
            MasteryBar.Top.Set(264, 0f);
            MasteryBar.Width.Set(340f, 0f);
            MasteryBar.Height.Set(12, 0f);
            MasteryBar.OnMouseOver += MasteryBarMouseOver;
            MasteryBar.OnMouseOut += (o, e) => { Tooltip = null; };

            Panel.Append(MasteryBar);

            Grid = new();
            Grid.Left.Set(24, 0f);
            Grid.Top.Set(50, 0f);
            Grid.Width.Set(312, 0f);
            Grid.Height.Set(192, 0f);

            Panel.Append(Grid);

            if (!transformationPanel.Complete && transformationPanel.Name != Defaults.DefaultPanel.Name)
            {
                var defPanel = Defaults.DefaultPanel;
                foreach (var connection in defPanel.Connections)
                {
                    var line = new TransConnector(connection);
                    Connections.Add(line);
                    Grid.Append(line);
                }
            }

            foreach (var connection in transformationPanel.Connections)
            {
                var line = new TransConnector(connection);
                Connections.Add(line);
                Grid.Append(line);
            }

            if (!transformationPanel.Complete && transformationPanel.Name != Defaults.DefaultPanel.Name)
            {
                var defPanel = Defaults.DefaultPanel;
                foreach (var node in defPanel.Nodes)
                {
                    var Button = new TransNode(ModContent.Request<Texture2D>(node.IconPath), node);
                    Nodes.Add(Button);
                    Grid.Append(Button);
                }
            }

            foreach (var node in transformationPanel.Nodes)
            {
                var Button = new TransNode(ModContent.Request<Texture2D>(node.IconPath), node);
                Nodes.Add(Button);
                Grid.Append(Button);
            }

        }

    }
}
