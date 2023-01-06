using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace DBZGoatLib.Model
{
    public static class DefaultPanels
    {
        public static TransformationPanel DBTPanel = new TransformationPanel();
    }

    public readonly struct TransformationPanel
    {
        public readonly Connection[] Connections;
        public readonly Node[] Nodes;

        /// <summary>
        /// The display name for this node, this will be displayed at the top of the transformation UI.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Whether this panel is a complete panel (true) or whether it is meant to be appended to the base DBT Panel (false).
        /// </summary>
        public readonly bool Complete;

        /// <param name="name">The display name for this node, this will be displayed at the top of the transformation UI.</param>
        public TransformationPanel(string name, bool complete, Node[] nodes, Connection[] connections)
        {
            Name = name;
            Nodes = nodes;
            Connections = connections;
            Complete = complete;
        }
    }

    public readonly struct Connection
    {
        public readonly float StartPosX;
        public readonly float StartPosY;

        public readonly float Length;
        public readonly bool Veritcal;
        

        /// <summary>
        /// The color gradient of this connection. See Documentation for a guide on how to create Gradient objects.
        /// </summary>
        public readonly Gradient Color;

        /// <param name="StartPositionX">The X Position in the 8 x 5 grid this connection starts on.</param>
        /// <param name="StartPositionY">The Y Position in the 8 x 5 grid this connection starts on.</param>
        /// <param name="length">The length in grid units this connector is.</param>
        /// <param name="vertical">Whether this is a vertical (true) or horizontal (false) connector.</param>
        /// <param name="color">The color of this connection.</param>
        public Connection(float StartPositionX, float StartPositionY, float length, bool vertical, Gradient color)
        {
            StartPosX = StartPositionX;
            StartPosY = StartPositionY;
            Color = color;
            Veritcal = vertical;
            Length = length;
        }
    }

    // Credits to KosmicShovel for providing this amazing gradient code.
    public class Gradient
    {
        private readonly List<Tuple<float, Color>> gradientStops;

        public Gradient()
        {
            gradientStops = new List<Tuple<float, Color>>();
        }

        /// <param name="color">Starting color of this gradient.</param>
        public Gradient(Color color)
        {
            gradientStops = new List<Tuple<float, Color>>();
            gradientStops.Add(new Tuple<float, Color>(0f, color));
        }
        public Gradient(Color startColor, params (float percent, Color color)[] subsequentColors)
        { 
            gradientStops = new List<Tuple<float, Color>>() {
                new Tuple<float, Color>(0, startColor)
            };
            gradientStops.AddRange(from color_value_pair in subsequentColors select new Tuple<float, Color>(color_value_pair.percent, color_value_pair.color));
        }

        /// <summary>
        /// Adds a new color point to this gradient at a specified percent point in its length.
        /// </summary>
        /// <param name="percent">0 to 1 value on where this new color kicks in.</param>
        /// <param name="color">Color value.</param>
        public Gradient AddStop(float percent, Color color)
        {
            gradientStops.Add(new Tuple<float, Color>(percent, color));
            return this;
        }

        public Color GetColor(double percent)
        {
            if (percent < 0 || percent > 1)
                throw new ArgumentOutOfRangeException(nameof(percent), "Percent must be between 0 and 1");

            if (gradientStops.Count == 0)
                return Color.Black;

            if (gradientStops.Count == 1)
                return gradientStops[0].Item2;

            // Find the two nearest gradient stops
            Tuple<float, Color> stop1 = null!;
            Tuple<float, Color> stop2 = null!;
            for (int i = 0; i < gradientStops.Count; i++)
            {
                if (gradientStops[i].Item1 <= percent)
                {
                    stop1 = gradientStops[i];
                }
                else
                {
                    stop2 = gradientStops[i];
                    break;
                }
            }

            // If there is no stop2, then the percent is greater than all of the gradient stops, so return the last stop's color
            if (stop2 == null)
                return stop1.Item2;

            // Find the percent distance between the two nearest stops
            double percentDistance = stop2.Item1 - stop1.Item1;
            double percentThroughStops = (percent - stop1.Item1) / percentDistance;

            // Find the R, G, and B values of the color between the two stops
            double r = stop1.Item2.R + (stop2.Item2.R - stop1.Item2.R) * percentThroughStops;
            double g = stop1.Item2.G + (stop2.Item2.G - stop1.Item2.G) * percentThroughStops;
            double b = stop1.Item2.B + (stop2.Item2.B - stop1.Item2.B) * percentThroughStops;

            return new Color((int)r, (int)g, (int)b);
        }
    }

    public readonly struct Node
    {
        public readonly float PosX;
        public readonly float PosY;

        public readonly string IconPath;

        public readonly bool ViewOnly;

        /// <summary>
        /// Buff Key Name as defined in the TransformationInfo. Can also be used to link to DBT's own transformations, see Documentation for more information.
        /// </summary>
        public readonly string BuffKeyName;

        /// <summary>
        /// A predicate which is ran each time the transformation menu is opened to check whether this node. If this predicate returns false, the node will have a lock displayed over it.
        /// </summary>
        public readonly Predicate<Player> UnlockCondition;

        /// <summary>
        /// A predicate which is ran each time the transformation menu is opened to check whether this node. If this predicate returns false, the node will be a black square rather than show its icon.
        /// </summary>
        public readonly Predicate<Player> DiscoverCondition;

        /// <summary>
        /// (Optional) A delegate which is executed when this node is successfully selected.
        /// </summary>
        public readonly Action<Player>? OnSelect;

        /// <summary>
        /// The hint which is displayed when the user clicks on this node while undiscovered or unlocked.
        /// </summary>
        public readonly string UnlockHint;

        /// <param name="PositionX">The X position in the 8 x 5 this node is located.</param>
        /// <param name="PositionY">The Y position in the 8 x 5 this node is located.</param>
        /// <param name="buffKeyName">The form's buffKeyName as defined in its registerd TransformationInfo.</param>
        /// <param name="iconPath">The string path to the sprite to be used for this node.</param>
        /// <param name="unlockHint">The hint to be displayed when the user clicks on this node while undiscovered or unlocked.</param>
        /// <param name="unlockCondition">Delegate which returns whether this node has been unlocked or not (controls lock icon).</param>
        /// <param name="discoverCondition">Delegate which returns whether this mode has been discovered or not (controls sprite display).</param>
        /// <param name="onSelect">(Optional) Delegate which is ran when the user successfully selects this node.</param>
        /// <param name="viewOnly">If set to true, this node can never be selected, and instead will only be used to see the Mastery value of the form when hovered.</param>
        public Node(float PositionX, float PositionY, string buffKeyName, string iconPath, string unlockHint, Predicate<Player> unlockCondition, Predicate<Player> discoverCondition, bool viewOnly = false, Action<Player> onSelect = null)
        {
            PosX = PositionX;
            PosY = PositionY;
            BuffKeyName = buffKeyName;
            IconPath = iconPath;
            UnlockHint = unlockHint;
            UnlockCondition = unlockCondition;
            DiscoverCondition = discoverCondition;
            OnSelect = onSelect;
            ViewOnly = viewOnly; 
        }
    }
}
