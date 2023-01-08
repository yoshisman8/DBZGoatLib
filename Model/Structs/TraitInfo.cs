using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace DBZGoatLib.Model
{
    public readonly struct TraitInfo
    {
        public readonly double Weight;
        public readonly string Name;
        public readonly Action<Player> IfTrait;
        public readonly Action<Player> IfUntrait;
        public readonly Gradient Color;

        /// <param name="name">Name of the trait.</param>
        /// <param name="chance">Chance of the trait from 0f to 1f.</param>
        /// <param name="color">Color gradient for the Ki Bar when this trait is active. Set to Null to not change the Ki Bar color.</param>
        /// <param name="ifTrait">Delegate which runs once when the user gains this trait and during player connecting.</param>
        /// <param name="ifUntrait">Delegate which runs once when the user loses this trait.</param>
        public TraitInfo(string name, double chance, Gradient color, Action<Player> ifTrait, Action<Player> ifUntrait)
        {
            Name = name;
            Weight = chance;
            IfTrait = ifTrait;
            IfUntrait = ifUntrait;
            Color = color;
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj.GetType() != typeof(TraitInfo))
                return false;
            TraitInfo other = (TraitInfo)obj;

            return Name == other.Name && Weight == other.Weight && Color == other.Color;
        }

    }
}
